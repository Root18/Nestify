import * as assert from "node:assert/strict";
import { test } from "node:test";
import {
  addNestPattern,
  AUTO_NEST_RULES,
  containsAnyChild,
  mergePatterns,
  removeAutoNestRules,
  removeNestPattern,
  splitChildren
} from "../patterns";

test("addNestPattern creates a new entry", () => {
  const result = addNestPattern({}, "app.js", ["app.min.js"]);
  assert.deepEqual(result, { "app.js": "app.min.js" });
});

test("addNestPattern appends to an existing entry without duplicates", () => {
  const result = addNestPattern({ "app.js": "app.min.js" }, "app.js", [
    "app.min.js",
    "app.bundle.js"
  ]);
  assert.deepEqual(result, { "app.js": "app.min.js, app.bundle.js" });
});

test("addNestPattern moves a child from its previous parent", () => {
  const result = addNestPattern({ "old.js": "shared.md, keep.js" }, "new.js", ["shared.md"]);
  assert.deepEqual(result, { "old.js": "keep.js", "new.js": "shared.md" });
});

test("addNestPattern drops entries that become empty", () => {
  const result = addNestPattern({ "old.js": "shared.md" }, "new.js", ["shared.md"]);
  assert.deepEqual(result, { "new.js": "shared.md" });
});

test("addNestPattern ignores the parent listed among the children", () => {
  const result = addNestPattern({}, "app.js", ["app.js", "app.min.js"]);
  assert.deepEqual(result, { "app.js": "app.min.js" });
});

test("addNestPattern prevents cycles when the new child was the parent's parent", () => {
  const result = addNestPattern({ "b.js": "a.js" }, "a.js", ["b.js"]);
  assert.deepEqual(result, { "a.js": "b.js" });
});

test("removeNestPattern removes children and drops empty entries", () => {
  const result = removeNestPattern(
    { "app.js": "app.min.js, app.bundle.js", "lib.js": "lib.min.js" },
    ["app.min.js", "lib.min.js"]
  );
  assert.deepEqual(result, { "app.js": "app.bundle.js" });
});

test("removeNestPattern leaves unrelated entries untouched", () => {
  const patterns = { "app.js": "app.min.js" };
  const result = removeNestPattern(patterns, ["other.js"]);
  assert.deepEqual(result, patterns);
});

test("containsAnyChild matches exact child names only", () => {
  const patterns = { "app.js": "app.min.js, ${capture}.md" };
  assert.equal(containsAnyChild(patterns, ["app.min.js"]), true);
  assert.equal(containsAnyChild(patterns, ["app.md"]), false);
});

test("mergePatterns adds new keys and appends missing children", () => {
  const result = mergePatterns(
    { "*.js": "${capture}.min.js", "custom.txt": "notes.txt" },
    { "*.js": "${capture}.min.js, ${capture}.md", "*.cs": "${capture}.md" }
  );
  assert.deepEqual(result, {
    "*.js": "${capture}.min.js, ${capture}.md",
    "custom.txt": "notes.txt",
    "*.cs": "${capture}.md"
  });
});

test("removeAutoNestRules removes only the built-in rule children", () => {
  const withRules = mergePatterns({ "*.cs": "${capture}.md, Custom.cs" }, AUTO_NEST_RULES);
  const result = removeAutoNestRules(withRules);
  assert.deepEqual(result, { "*.cs": "Custom.cs" });
});

test("removeAutoNestRules on the exact rule set yields an empty object", () => {
  assert.deepEqual(removeAutoNestRules(AUTO_NEST_RULES), {});
});

test("auto-nest rules mirror the Visual Studio rules", () => {
  assert.equal(AUTO_NEST_RULES["I*.cs"], "${capture}.cs");
  for (const key of ["*.cs", "*.vb", "*.ts", "*.tsx", "*.jsx"]) {
    assert.ok(splitChildren(AUTO_NEST_RULES[key]).includes("${capture}.md"), key);
  }
  const jsChildren = splitChildren(AUTO_NEST_RULES["*.js"]);
  for (const child of [
    "${capture}.md",
    "${capture}.min.js",
    "${capture}.bundle.js",
    "${capture}.bundle.min.js"
  ]) {
    assert.ok(jsChildren.includes(child), child);
  }
  assert.equal(AUTO_NEST_RULES["*.bundle.js"], "${capture}.bundle.min.js");
});
