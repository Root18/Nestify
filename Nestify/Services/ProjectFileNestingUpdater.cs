using Nestify.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Nestify.Services;

/// <summary>
/// Direct project-file (XML) fallback used only when neither DTE nor
/// IVsBuildPropertyStorage can apply the nesting metadata.
/// </summary>
internal static class ProjectFileNestingUpdater
{
    // Only project types where Solution Explorer honors DependentUpon and the items
    // actually live in this file. Notably excluded: .csproj/.esproj/.njsproj (handled
    // by the project system before the fallback is reached), .vcxproj (nesting comes
    // from .filters files), .shproj (items live in the companion .projitems file).
    private static readonly HashSet<string> FallbackProjectExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".vbproj", ".fsproj", ".pyproj"
        };

    public static bool ShouldUseDirectProjectFileFallback(string projectPath)
    {
        return FallbackProjectExtensions.Contains(Path.GetExtension(projectPath));
    }

    public static void UpdateNesting(string projectPath, string itemPath, string parentName, bool isNesting)
    {
        var doc = XDocument.Load(projectPath);
        var root = doc.Root;
        if (root == null) return;

        var ns = root.Name.NamespaceName;
        var xns = string.IsNullOrEmpty(ns) ? XNamespace.None : XNamespace.Get(ns);

        var projectDir = Path.GetDirectoryName(projectPath);
        var relativePath = PathUtilities.GetRelativePath(projectDir, itemPath);
        var isNodeJsProject = IsNodeJsProject(projectPath);

        // Match the file under any item type (Compile, Content, None, EmbeddedResource,
        // Page, TypeScriptCompile, ...) so an existing item is always reused instead of
        // adding a duplicate. Items may use Include (classic) or Update (SDK-style).
        var matchingElements = root.Descendants()
            .Where(e => e.Parent?.Name == xns + "ItemGroup")
            .Where(e =>
            {
                var candidate = e.Attribute("Include")?.Value ?? e.Attribute("Update")?.Value;
                return candidate != null && PathsEqual(candidate, relativePath);
            })
            .ToList();

        var itemElement = isNodeJsProject
            ? matchingElements.FirstOrDefault(e => e.Name == xns + "Content")
            : matchingElements.FirstOrDefault(e => e.Name == xns + "None");

        itemElement ??= matchingElements.FirstOrDefault(e => e.Name == xns + "Content");
        itemElement ??= matchingElements.FirstOrDefault(e => e.Name == xns + "None");
        itemElement ??= matchingElements.FirstOrDefault(e => e.Name == xns + "Compile");
        itemElement ??= matchingElements.FirstOrDefault();

        if (itemElement != null &&
            isNodeJsProject &&
            itemElement.Attribute("Include") != null &&
            itemElement.Name != xns + "Content")
        {
            itemElement.Name = xns + "Content";
        }

        if (itemElement == null)
        {
            if (!isNesting) return;

            // Prefer a group that already holds items of the same type; some project
            // formats use their leading ItemGroups for special purposes, so never
            // blindly append to the first one.
            var itemType = GetNewItemType(projectPath);
            var itemGroup = root.Elements(xns + "ItemGroup")
                .FirstOrDefault(g => g.Elements(xns + itemType).Any());
            if (itemGroup == null)
            {
                itemGroup = new XElement(xns + "ItemGroup");
                root.Add(itemGroup);
            }

            itemElement = new XElement(xns + itemType);
            itemElement.SetAttributeValue("Include", relativePath);
            itemGroup.Add(itemElement);
            matchingElements.Add(itemElement);
        }

        if (isNesting)
        {
            foreach (var element in matchingElements.Where(e => e != itemElement))
            {
                element.Element(xns + "DependentUpon")?.Remove();
            }

            var dependentUponElement = itemElement.Element(xns + "DependentUpon");
            if (dependentUponElement != null)
                dependentUponElement.Value = parentName;
            else
                itemElement.Add(new XElement(xns + "DependentUpon", parentName));
        }
        else
        {
            foreach (var element in matchingElements)
            {
                element.Element(xns + "DependentUpon")?.Remove();
            }
        }

        doc.Save(projectPath);
    }

    private static bool PathsEqual(string left, string right)
    {
        return string.Equals(NormalizePath(left), NormalizePath(right), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('/', Path.DirectorySeparatorChar);
    }

    private static bool IsNodeJsProject(string projectPath)
    {
        return string.Equals(Path.GetExtension(projectPath), ".njsproj", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetNewItemType(string projectPath)
    {
        return IsNodeJsProject(projectPath) ? "Content" : "None";
    }
}
