using Nestify.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nestify.Services;

internal class SiblingFileProvider(IFileValidator fileValidator) : ISiblingFileProvider
{
    private readonly IFileValidator _fileValidator =
        fileValidator ?? throw new ArgumentNullException(nameof(fileValidator));

    public List<string> GetSiblingFiles(string directory, HashSet<string> excludeNames)
    {
        var result = new List<string>();

        if (!Directory.Exists(directory))
            return result;

        result.AddRange(Directory.GetFiles(directory).Select(Path.GetFileName).Where(fileName =>
            !excludeNames.Contains(fileName) && _fileValidator.IsPickerCandidate(fileName)));

        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }
}