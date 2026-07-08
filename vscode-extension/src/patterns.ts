/**
 * Pure helpers for manipulating VS Code's `explorer.fileNesting.patterns` setting.
 *
 * The setting is an object whose keys are parent file patterns and whose values
 * are comma-separated child patterns, e.g. `{ "*.ts": "${capture}.js" }`.
 * Nestify writes exact file names for manual nesting and glob rules for auto-nest.
 */
export type NestingPatterns = Readonly<Record<string, string>>;

/**
 * Built-in auto-nest rules, mirroring the Visual Studio extension:
 * - C# implementations nest under their interfaces (UserService.cs → IUserService.cs)
 * - Markdown documentation nests under the matching code file (.cs/.vb/.ts/.tsx/.js/.jsx)
 * - Minified/bundled JS nests under its source
 *
 * Note: VS Code file nesting is single-level, so app.bundle.min.js nests directly
 * under app.js when app.js exists; the *.bundle.js rule covers the case where the
 * plain source file does not exist.
 */
export const AUTO_NEST_RULES: NestingPatterns = {
  "I*.cs": "${capture}.cs",
  "*.cs": "${capture}.md",
  "*.vb": "${capture}.md",
  "*.ts": "${capture}.md",
  "*.tsx": "${capture}.md",
  "*.jsx": "${capture}.md",
  "*.js": "${capture}.md, ${capture}.min.js, ${capture}.bundle.js, ${capture}.bundle.min.js",
  "*.bundle.js": "${capture}.bundle.min.js"
};

export function splitChildren(value: string): string[] {
  return value
    .split(",")
    .map((part) => part.trim())
    .filter((part) => part.length > 0);
}

export function joinChildren(children: string[]): string {
  return children.join(", ");
}

/**
 * Nest the given child file names under the given parent file name.
 * A file can only have one parent, so the children are removed from every other
 * entry first; the parent is also removed from the children's own entries so a
 * cycle cannot be created.
 */
export function addNestPattern(
  patterns: NestingPatterns,
  parentName: string,
  childNames: readonly string[]
): Record<string, string> {
  const children = childNames.filter((child) => child !== parentName);
  const result: Record<string, string> = {};

  for (const [key, value] of Object.entries(patterns)) {
    let list = splitChildren(value).filter((child) => !children.includes(child));
    if (children.includes(key)) {
      list = list.filter((child) => child !== parentName);
    }
    if (list.length > 0) {
      result[key] = joinChildren(list);
    }
  }

  const existing = result[parentName] ? splitChildren(result[parentName]) : [];
  for (const child of children) {
    if (!existing.includes(child)) {
      existing.push(child);
    }
  }
  if (existing.length > 0) {
    result[parentName] = joinChildren(existing);
  }

  return result;
}

/** Remove the given file names as children from every entry, dropping empty entries. */
export function removeNestPattern(
  patterns: NestingPatterns,
  fileNames: readonly string[]
): Record<string, string> {
  const result: Record<string, string> = {};

  for (const [key, value] of Object.entries(patterns)) {
    const list = splitChildren(value).filter((child) => !fileNames.includes(child));
    if (list.length > 0) {
      result[key] = joinChildren(list);
    }
  }

  return result;
}

/** True when any entry lists one of the given file names as a child (exact match). */
export function containsAnyChild(
  patterns: NestingPatterns,
  fileNames: readonly string[]
): boolean {
  return Object.values(patterns).some((value) =>
    splitChildren(value).some((child) => fileNames.includes(child))
  );
}

/** Merge additional patterns into the base, appending missing children per key. */
export function mergePatterns(
  base: NestingPatterns,
  additions: NestingPatterns
): Record<string, string> {
  const result: Record<string, string> = { ...base };

  for (const [key, value] of Object.entries(additions)) {
    if (!result[key]) {
      result[key] = value;
      continue;
    }
    const merged = splitChildren(result[key]);
    for (const child of splitChildren(value)) {
      if (!merged.includes(child)) {
        merged.push(child);
      }
    }
    result[key] = joinChildren(merged);
  }

  return result;
}

/** Remove exactly the auto-nest rule children from the given patterns (per key). */
export function removeAutoNestRules(patterns: NestingPatterns): Record<string, string> {
  const result: Record<string, string> = {};

  for (const [key, value] of Object.entries(patterns)) {
    const ruleChildren = AUTO_NEST_RULES[key] ? splitChildren(AUTO_NEST_RULES[key]) : [];
    const list = splitChildren(value).filter((child) => !ruleChildren.includes(child));
    if (list.length > 0) {
      result[key] = joinChildren(list);
    }
  }

  return result;
}
