using Nestify.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nestify.Services
{
    internal class SiblingFileProvider : ISiblingFileProvider
    {
        private readonly IFileValidator _fileValidator;

        public SiblingFileProvider(IFileValidator fileValidator)
        {
            _fileValidator = fileValidator ?? throw new ArgumentNullException(nameof(fileValidator));
        }

        public List<string> GetSiblingFiles(string directory, HashSet<string> excludeNames)
        {
            var result = new List<string>();

            if (!Directory.Exists(directory))
                return result;

            foreach (string filePath in Directory.GetFiles(directory))
            {
                string fileName = Path.GetFileName(filePath);
                if (!excludeNames.Contains(fileName) && _fileValidator.IsPickerCandidate(fileName))
                {
                    result.Add(fileName);
                }
            }

            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }
    }
}
