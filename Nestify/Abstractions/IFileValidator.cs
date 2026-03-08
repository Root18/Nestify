namespace Nestify.Abstractions;

internal interface IFileValidator
{
    bool IsSupportedFile(string fileName);
    bool IsPickerCandidate(string fileName);
}