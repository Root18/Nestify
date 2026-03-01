using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Nestify.Dialogs
{
    public class ParentFilePickerDialog : Window
    {
        private readonly List<string> _allFiles;
        private readonly ListBox _fileListBox;
        private readonly TextBox _filterTextBox;

        public string SelectedFile { get; private set; }

        public ParentFilePickerDialog(List<string> files)
        {
            _allFiles = files ?? new List<string>();

            Title = "Nestify \u2013 Select Parent File";
            Width = 400;
            Height = 350;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.CanResizeWithGrip;
            ShowInTaskbar = false;

            var grid = new Grid();
            grid.Margin = new Thickness(12);
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var label = new TextBlock
            {
                Text = "Select the parent file to nest under:",
                Margin = new Thickness(0, 0, 0, 8)
            };
            Grid.SetRow(label, 0);
            grid.Children.Add(label);

            _filterTextBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 8),
                ToolTip = "Type to filter files"
            };
            _filterTextBox.TextChanged += FilterTextBox_TextChanged;
            Grid.SetRow(_filterTextBox, 1);
            grid.Children.Add(_filterTextBox);

            _fileListBox = new ListBox { SelectionMode = SelectionMode.Single };
            _fileListBox.ItemsSource = _allFiles;
            _fileListBox.MouseDoubleClick += FileListBox_MouseDoubleClick;
            Grid.SetRow(_fileListBox, 2);
            grid.Children.Add(_fileListBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 75,
                IsDefault = true,
                Margin = new Thickness(0, 0, 8, 0)
            };
            okButton.Click += OkButton_Click;
            buttonPanel.Children.Add(okButton);

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 75,
                IsCancel = true
            };
            cancelButton.Click += CancelButton_Click;
            buttonPanel.Children.Add(cancelButton);

            Grid.SetRow(buttonPanel, 3);
            grid.Children.Add(buttonPanel);

            Content = grid;

            if (_allFiles.Count > 0)
                _fileListBox.SelectedIndex = 0;
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = _filterTextBox.Text;
            if (string.IsNullOrWhiteSpace(filter))
            {
                _fileListBox.ItemsSource = _allFiles;
            }
            else
            {
                _fileListBox.ItemsSource = _allFiles
                    .Where(f => f.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            if (_fileListBox.Items.Count > 0)
                _fileListBox.SelectedIndex = 0;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (_fileListBox.SelectedItem != null)
            {
                SelectedFile = _fileListBox.SelectedItem.ToString();
                DialogResult = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void FileListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_fileListBox.SelectedItem != null)
            {
                SelectedFile = _fileListBox.SelectedItem.ToString();
                DialogResult = true;
            }
        }
    }
}
