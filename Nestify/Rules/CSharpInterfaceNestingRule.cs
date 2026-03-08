using Nestify.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nestify.Rules;

internal class CSharpInterfaceNestingRule : INestingRule
{
    public bool CanHandle(string fileName)
    {
        return fileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase);
    }

    public string FindParent(string fileName, HashSet<string> availableFiles)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

        // Don't nest interfaces themselves (files starting with I + uppercase letter)
        if (nameWithoutExt.Length > 1 && nameWithoutExt[0] == 'I' && char.IsUpper(nameWithoutExt[1]))
            return null;

        // ClassName.cs → IClassName.cs
        var interfaceName = "I" + nameWithoutExt + ".cs";

        return availableFiles.Contains(interfaceName) ? interfaceName : null;
    }
}