using System;
using System.Collections.Generic;

namespace Nestify.Abstractions
{
    internal interface IDialogService
    {
        string ShowParentFilePicker(List<string> files, IntPtr ownerHandle);
    }
}
