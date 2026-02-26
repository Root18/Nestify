using System;
using System.Collections.Generic;
using System.IO;

namespace Nestify
{
    internal static class FileNestingHelper
    {
        private static readonly HashSet<string> SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".vb", ".fs",
            ".js", ".jsx", ".ts", ".tsx",
            ".css", ".scss", ".less",
            ".html", ".htm",
            ".json", ".xml", ".config",
            ".resx", ".xaml",
            ".razor", ".cshtml"
        };

        private static readonly HashSet<string> ExcludedFromPicker = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "package.json",
            "package-lock.json",
            "tsconfig.json",
            "tsconfig.app.json",
            "tsconfig.spec.json",
            "jsconfig.json",
            "eslint.config.js",
            "eslint.config.mjs",
            "eslint.config.cjs",
            ".eslintrc",
            ".eslintrc.js",
            ".eslintrc.cjs",
            ".eslintrc.json",
            ".eslintignore",
            ".prettierrc",
            ".prettierrc.js",
            ".prettierrc.json",
            ".prettierignore",
            "prettier.config.js",
            "prettier.config.mjs",
            "babel.config.js",
            "babel.config.json",
            ".babelrc",
            ".babelrc.json",
            "webpack.config.js",
            "webpack.config.ts",
            "vite.config.js",
            "vite.config.ts",
            "vite.config.mjs",
            "rollup.config.js",
            "rollup.config.mjs",
            "jest.config.js",
            "jest.config.ts",
            "jest.config.json",
            "karma.conf.js",
            "vitest.config.js",
            "vitest.config.ts",
            ".browserslistrc",
            ".editorconfig",
            ".npmrc",
            ".nvmrc",
            ".env",
            ".env.local",
            ".env.development",
            ".env.production",
            ".gitignore",
            ".gitattributes",
            "angular.json",
            "next.config.js",
            "next.config.mjs",
            "nuxt.config.js",
            "nuxt.config.ts",
            "svelte.config.js",
            "tailwind.config.js",
            "tailwind.config.ts",
            "postcss.config.js",
            "postcss.config.mjs",
            "nodemon.json",
            "yarn.lock",
            "pnpm-lock.yaml",
        };

        public static bool IsSupportedFile(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            return !string.IsNullOrEmpty(ext) && SupportedExtensions.Contains(ext);
        }

        public static bool IsPickerCandidate(string fileName)
        {
            return IsSupportedFile(fileName) && !ExcludedFromPicker.Contains(fileName);
        }

        public static string GetRelativePath(string basePath, string fullPath)
        {
            if (!basePath.EndsWith("\\", StringComparison.Ordinal))
                basePath += "\\";

            if (fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(basePath.Length);
            }

            return Path.GetFileName(fullPath);
        }
    }
}
