using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nestify.Abstractions;
using System;
using System.Xml.Linq;
using System.IO;
using System.Linq;

namespace Nestify.Services;

internal class FileNestingService : IFileNestingService
{
    public void NestFile(ProjectItem childItem, ProjectItem parentItem, IVsHierarchy hierarchy,
        IVsBuildPropertyStorage storage)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var childPath = childItem.FileNames[1];
        var projectPath = childItem.ContainingProject?.FullName;
        var enforceNoneItemType = IsCSharpProject(projectPath);

        if (TrySetDependentUponViaDte(childItem, parentItem.Name, enforceNoneItemType))
            return;

        var storageUpdated = false;
        if (storage != null &&
            hierarchy.ParseCanonicalName(childPath, out var itemId) == 0 &&
            itemId != 0)
        {
            storageUpdated = SetDependentUponAttribute(storage, itemId, parentItem.Name);
        }

        if (storageUpdated)
            return;

        // Fall back to direct project file manipulation only when project-system update is unavailable.
        try
        {
            var project = childItem.ContainingProject;
            projectPath = project.FullName;
            if (!File.Exists(projectPath)) return;
            if (!ShouldUseDirectProjectFileFallback(projectPath)) return;

            TryUpdateProjectFileNesting(projectPath, childPath, parentItem.Name, true);
        }
        catch
        {
            // Ignore fallback errors.
        }
    }

    public void UnnestFile(ProjectItem childItem, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var filePath = childItem.FileNames[1];
        var projectPath = childItem.ContainingProject?.FullName;
        var enforceNoneItemType = IsCSharpProject(projectPath);

        if (TrySetDependentUponViaDte(childItem, string.Empty, enforceNoneItemType))
            return;

        var storageUpdated = false;
        if (storage != null &&
            hierarchy.ParseCanonicalName(filePath, out var itemId) == 0 &&
            itemId != 0)
        {
            storageUpdated = SetDependentUponAttribute(storage, itemId, string.Empty);
        }

        if (storageUpdated)
            return;

        // Fall back to direct project file manipulation only when project-system update is unavailable.
        try
        {
            var project = childItem.ContainingProject;
            projectPath = project.FullName;
            if (!File.Exists(projectPath)) return;
            if (!ShouldUseDirectProjectFileFallback(projectPath)) return;

            TryUpdateProjectFileNesting(projectPath, filePath, null, false);
        }
        catch
        {
            // Ignore fallback errors.
        }
    }

    private static bool SetDependentUponAttribute(IVsBuildPropertyStorage storage, uint itemId, string value)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return storage.SetItemAttribute(itemId, "DependentUpon", value) == 0;
    }

    private static bool TrySetDependentUponViaDte(ProjectItem item, string value, bool enforceNoneItemType)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            if (item?.Properties == null)
                return false;

            if (enforceNoneItemType)
            {
                TrySetPropertyValue(item.Properties, "ItemType", "None");
            }

            Property dependentUponProperty;
            try
            {
                dependentUponProperty = item.Properties.Item("DependentUpon");
            }
            catch
            {
                return false;
            }

            if (dependentUponProperty == null)
                return false;

            dependentUponProperty.Value = value;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TrySetPropertyValue(EnvDTE.Properties properties, string propertyName, object value)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            var property = properties.Item(propertyName);
            if (property == null)
                return false;

            property.Value = value;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void TryUpdateProjectFileNesting(string projectPath, string itemPath, string parentName,
        bool isNesting)
    {
        try
        {
            var doc = XDocument.Load(projectPath);
            var root = doc.Root;
            if (root == null) return;

            var ns = root.Name.NamespaceName;
            var xns = string.IsNullOrEmpty(ns) ? XNamespace.None : XNamespace.Get(ns);

            // Get the relative path from the project directory
            var projectDir = Path.GetDirectoryName(projectPath);
            var relativePath = GetRelativePath(projectDir, itemPath);
            var isNodeJsProject = IsNodeJsProject(projectPath);

            // Find all matching item elements in the project file
            var matchingElements = root.Descendants(xns + "Compile")
                .Concat(root.Descendants(xns + "Content"))
                .Concat(root.Descendants(xns + "None"))
                .Where(e =>
                {
                    var includeAttr = e.Attribute("Include");
                    var updateAttr = e.Attribute("Update");
                    var candidate = includeAttr?.Value ?? updateAttr?.Value;
                    return candidate != null &&
                           NormalizePath(candidate) == NormalizePath(relativePath);
                })
                .ToList();

            var includeElements = matchingElements.Where(e =>
                e.Attribute("Include") != null &&
                NormalizePath(e.Attribute("Include")?.Value) == NormalizePath(relativePath)).ToList();

            var itemElement = isNodeJsProject
                ? includeElements.FirstOrDefault(e => e.Name == xns + "Content")
                : includeElements.FirstOrDefault(e => e.Name == xns + "None");

            itemElement ??= includeElements.FirstOrDefault(e => e.Name == xns + "Content");
            itemElement ??= includeElements.FirstOrDefault(e => e.Name == xns + "None");
            itemElement ??= includeElements.FirstOrDefault(e => e.Name == xns + "Compile");
            itemElement ??= includeElements.FirstOrDefault();

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

                var itemType = GetNewItemType(projectPath);
                var attributeName = "Include";
                var itemGroup = root.Elements(xns + "ItemGroup").FirstOrDefault();
                if (itemGroup == null)
                {
                    itemGroup = new XElement(xns + "ItemGroup");
                    root.Add(itemGroup);
                }

                itemElement = new XElement(xns + itemType);
                itemElement.SetAttributeValue(attributeName, relativePath);
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
        catch
        {
            // ignored
        }
    }

    private static string GetRelativePath(string fromPath, string toPath)
    {
        var fromUri = new Uri(fromPath + Path.DirectorySeparatorChar);
        var toUri = new Uri(toPath);

        var relativeUri = fromUri.MakeRelativeUri(toUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        // Convert forward slashes to backslashes for Windows
        return relativePath.Replace('/', Path.DirectorySeparatorChar);
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
    }

    private static bool ShouldUseDirectProjectFileFallback(string projectPath)
    {
        var extension = Path.GetExtension(projectPath);
        return !string.Equals(extension, ".esproj", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(extension, ".csproj", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(extension, ".njsproj", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNodeJsProject(string projectPath)
    {
        if (string.IsNullOrEmpty(projectPath))
            return false;

        return string.Equals(Path.GetExtension(projectPath), ".njsproj", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetNewItemType(string projectPath)
    {
        var extension = Path.GetExtension(projectPath);
        return string.Equals(extension, ".njsproj", StringComparison.OrdinalIgnoreCase)
            ? "Content"
            : "None";
    }

    private static bool IsCSharpProject(string projectPath)
    {
        if (string.IsNullOrEmpty(projectPath))
            return false;

        return string.Equals(Path.GetExtension(projectPath), ".csproj", StringComparison.OrdinalIgnoreCase);
    }
}