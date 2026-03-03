using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Nestify.Tests.Dialogs
{
    [TestClass]
    public class ParentFilePickerDialogTests
    {
        private static void RunOnSta(Action action)
        {
            Exception caught = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    caught = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (caught != null)
                throw caught;
        }

        [TestMethod]
        public void Constructor_WithFiles_SetsSelectedFileToNull()
        {
            RunOnSta(() =>
            {
                var files = new List<string> { "Alpha.cs", "Beta.cs" };
                var dialog = new ParentFilePickerDialog(files);

                Assert.IsNull(dialog.SelectedFile);
            });
        }

        [TestMethod]
        public void Constructor_WithNullFiles_DoesNotThrow()
        {
            RunOnSta(() =>
            {
                var dialog = new ParentFilePickerDialog(null);

                Assert.IsNotNull(dialog);
                Assert.IsNull(dialog.SelectedFile);
            });
        }

        [TestMethod]
        public void Constructor_WithEmptyList_DoesNotThrow()
        {
            RunOnSta(() =>
            {
                var dialog = new ParentFilePickerDialog(new List<string>());

                Assert.IsNotNull(dialog);
            });
        }

        [TestMethod]
        public void Constructor_SetsWindowTitle()
        {
            RunOnSta(() =>
            {
                var dialog = new ParentFilePickerDialog(new List<string>());

                Assert.IsFalse(string.IsNullOrEmpty(dialog.Title));
                StringAssert.Contains(dialog.Title, "Nestify");
            });
        }

        [TestMethod]
        public void Constructor_SetsExpectedDimensions()
        {
            RunOnSta(() =>
            {
                var dialog = new ParentFilePickerDialog(new List<string>());

                Assert.AreEqual(400, dialog.Width);
                Assert.AreEqual(350, dialog.Height);
            });
        }
    }
}
