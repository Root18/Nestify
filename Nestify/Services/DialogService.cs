using Nestify.Abstractions;
using System;
using System.Collections.Generic;

namespace Nestify.Services;

internal class DialogService : IDialogService
{
    public string ShowParentFilePicker(List<string> files, IntPtr ownerHandle)
    {
        var dialog = new Dialogs.ParentFilePickerDialog(files);
        if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.SelectedFile))
            return dialog.SelectedFile;

        return null;
    }
}