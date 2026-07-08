using Microsoft.VisualStudio.PlatformUI;
using Nestify.Abstractions;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Nestify.Services;

internal class DialogService : IDialogService
{
    public string ShowParentFilePicker(List<string> files, IntPtr ownerHandle)
    {
        var dialog = new Dialogs.ParentFilePickerDialog(files);

        // Owning the dialog to the VS main window keeps it centered, on the right
        // monitor, and prevents it from falling behind the IDE.
        if (ownerHandle != IntPtr.Zero)
            _ = new WindowInteropHelper(dialog) { Owner = ownerHandle };

        // Follow the active VS theme instead of default WPF white.
        dialog.SetResourceReference(Control.BackgroundProperty, EnvironmentColors.ToolWindowBackgroundBrushKey);
        dialog.SetResourceReference(Control.ForegroundProperty, EnvironmentColors.ToolWindowTextBrushKey);

        if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.SelectedFile))
            return dialog.SelectedFile;

        return null;
    }
}
