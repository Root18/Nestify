import * as path from "path";
import * as vscode from "vscode";
import {
  addNestPattern,
  AUTO_NEST_RULES,
  containsAnyChild,
  mergePatterns,
  NestingPatterns,
  removeAutoNestRules,
  removeNestPattern
} from "./patterns";

const PATTERNS_KEY = "explorer.fileNesting.patterns";
const ENABLED_KEY = "explorer.fileNesting.enabled";

export function activate(context: vscode.ExtensionContext): void {
  context.subscriptions.push(
    vscode.commands.registerCommand("nestify.nestUnder", nestUnder),
    vscode.commands.registerCommand("nestify.unnest", unnest),
    vscode.commands.registerCommand("nestify.autoNest", autoNest),
    vscode.commands.registerCommand("nestify.removeAutoNestRules", removeRules),
    vscode.commands.registerCommand("nestify.toggleFileNesting", toggleFileNesting)
  );
}

export function deactivate(): void {
  // Nothing to clean up: all state lives in the user's settings.
}

/** Explorer context menus pass (clickedUri, allSelectedUris); fall back to the active editor. */
function selectedFiles(uri?: vscode.Uri, uris?: vscode.Uri[]): vscode.Uri[] {
  const selection = uris && uris.length > 0 ? uris : uri ? [uri] : [];
  const files = selection.filter((u) => u.scheme === "file");
  if (files.length > 0) {
    return files;
  }
  const active = vscode.window.activeTextEditor?.document.uri;
  return active?.scheme === "file" ? [active] : [];
}

function configFor(resource: vscode.Uri | undefined): vscode.WorkspaceConfiguration {
  return vscode.workspace.getConfiguration(undefined, resource);
}

function writeTarget(): vscode.ConfigurationTarget {
  return vscode.workspace.workspaceFolders?.length
    ? vscode.ConfigurationTarget.Workspace
    : vscode.ConfigurationTarget.Global;
}

/** Read the patterns object as stored at the given scope (not the merged view). */
function patternsAt(
  config: vscode.WorkspaceConfiguration,
  target: vscode.ConfigurationTarget
): NestingPatterns {
  const inspected = config.inspect<Record<string, string>>(PATTERNS_KEY);
  const value =
    target === vscode.ConfigurationTarget.Workspace
      ? inspected?.workspaceValue
      : inspected?.globalValue;
  return value ?? {};
}

async function updatePatterns(
  resource: vscode.Uri | undefined,
  transform: (current: NestingPatterns) => Record<string, string>
): Promise<void> {
  const config = configFor(resource);
  const target = writeTarget();
  await config.update(PATTERNS_KEY, transform(patternsAt(config, target)), target);
}

async function ensureNestingEnabled(resource: vscode.Uri | undefined): Promise<void> {
  const config = configFor(resource);
  if (config.get<boolean>(ENABLED_KEY) !== true) {
    await config.update(ENABLED_KEY, true, writeTarget());
  }
}

async function nestUnder(uri?: vscode.Uri, uris?: vscode.Uri[]): Promise<void> {
  const files = selectedFiles(uri, uris);
  if (files.length === 0) {
    void vscode.window.showInformationMessage("Nestify: select one or more files in the Explorer.");
    return;
  }

  const directory = path.dirname(files[0].fsPath);
  if (files.some((file) => path.dirname(file.fsPath) !== directory)) {
    void vscode.window.showWarningMessage(
      "Nestify: files can only be nested under a file in the same folder."
    );
    return;
  }

  const selectedNames = files.map((file) => path.basename(file.fsPath));

  // Multi-select: any selected file can be the parent of the others.
  // Single file: pick a sibling from the same folder.
  let candidates: string[];
  if (selectedNames.length > 1) {
    candidates = [...selectedNames].sort((a, b) => a.localeCompare(b));
  } else {
    const entries = await vscode.workspace.fs.readDirectory(vscode.Uri.file(directory));
    candidates = entries
      .filter(([name, type]) => type === vscode.FileType.File && !selectedNames.includes(name))
      .map(([name]) => name)
      .sort((a, b) => a.localeCompare(b));
  }

  if (candidates.length === 0) {
    void vscode.window.showInformationMessage("Nestify: no sibling files found to nest under.");
    return;
  }

  const parentName = await vscode.window.showQuickPick(candidates, {
    placeHolder: "Select the parent file to nest under"
  });
  if (!parentName) {
    return;
  }

  const childNames = selectedNames.filter((name) => name !== parentName);
  if (childNames.length === 0) {
    return;
  }

  await updatePatterns(files[0], (current) => addNestPattern(current, parentName, childNames));
  await ensureNestingEnabled(files[0]);
}

async function unnest(uri?: vscode.Uri, uris?: vscode.Uri[]): Promise<void> {
  const files = selectedFiles(uri, uris);
  if (files.length === 0) {
    void vscode.window.showInformationMessage("Nestify: select one or more files in the Explorer.");
    return;
  }

  const names = files.map((file) => path.basename(file.fsPath));
  const config = configFor(files[0]);
  let removed = false;

  // Nesting entries may live in workspace or user settings; clean both.
  for (const target of [vscode.ConfigurationTarget.Workspace, vscode.ConfigurationTarget.Global]) {
    if (target === vscode.ConfigurationTarget.Workspace && !vscode.workspace.workspaceFolders?.length) {
      continue;
    }
    const current = patternsAt(config, target);
    if (!containsAnyChild(current, names)) {
      continue;
    }
    await config.update(PATTERNS_KEY, removeNestPattern(current, names), target);
    removed = true;
  }

  if (!removed) {
    void vscode.window.showInformationMessage(
      "Nestify: the selected files are not nested by an exact-name pattern. " +
        "Files nested by glob rules can be released via \"Nestify: Remove auto-nest rules\"."
    );
  }
}

async function autoNest(): Promise<void> {
  const resource = vscode.workspace.workspaceFolders?.[0]?.uri;
  await updatePatterns(resource, (current) => mergePatterns(current, AUTO_NEST_RULES));
  await ensureNestingEnabled(resource);
  void vscode.window.showInformationMessage(
    "Nestify: auto-nest rules applied. Related files now nest automatically as they are created."
  );
}

async function removeRules(): Promise<void> {
  const resource = vscode.workspace.workspaceFolders?.[0]?.uri;
  await updatePatterns(resource, removeAutoNestRules);
  void vscode.window.showInformationMessage("Nestify: auto-nest rules removed.");
}

async function toggleFileNesting(): Promise<void> {
  const resource = vscode.workspace.workspaceFolders?.[0]?.uri;
  const config = configFor(resource);
  const enabled = config.get<boolean>(ENABLED_KEY) === true;
  await config.update(ENABLED_KEY, !enabled, writeTarget());
  void vscode.window.showInformationMessage(
    `Nestify: file nesting ${enabled ? "disabled" : "enabled"}.`
  );
}
