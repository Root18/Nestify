using System.Collections.Generic;

namespace Nestify.Abstractions;

internal interface ISiblingFileProvider
{
    List<string> GetSiblingFiles(string directory, HashSet<string> excludeNames);
}