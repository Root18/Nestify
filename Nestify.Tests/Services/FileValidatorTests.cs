using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Services;

namespace Nestify.Tests.Services
{
    [TestClass]
    public class FileValidatorTests
    {
        private FileValidator _validator;

        [TestInitialize]
        public void Setup()
        {
            _validator = new FileValidator();
        }

        [TestMethod]
        [DataRow(".cs")]
        [DataRow(".vb")]
        [DataRow(".fs")]
        [DataRow(".js")]
        [DataRow(".jsx")]
        [DataRow(".ts")]
        [DataRow(".tsx")]
        [DataRow(".css")]
        [DataRow(".scss")]
        [DataRow(".less")]
        [DataRow(".html")]
        [DataRow(".htm")]
        [DataRow(".json")]
        [DataRow(".xml")]
        [DataRow(".config")]
        [DataRow(".resx")]
        [DataRow(".xaml")]
        [DataRow(".razor")]
        [DataRow(".cshtml")]
        public void IsSupportedFile_SupportedExtension_ReturnsTrue(string extension)
        {
            Assert.IsTrue(_validator.IsSupportedFile("file" + extension));
        }

        [TestMethod]
        [DataRow(".txt")]
        [DataRow(".png")]
        [DataRow(".exe")]
        [DataRow(".dll")]
        [DataRow(".md")]
        [DataRow(".yml")]
        [DataRow(".sln")]
        public void IsSupportedFile_UnsupportedExtension_ReturnsFalse(string extension)
        {
            Assert.IsFalse(_validator.IsSupportedFile("file" + extension));
        }

        [TestMethod]
        public void IsSupportedFile_NoExtension_ReturnsFalse()
        {
            Assert.IsFalse(_validator.IsSupportedFile("Makefile"));
        }

        [TestMethod]
        public void IsSupportedFile_EmptyString_ReturnsFalse()
        {
            Assert.IsFalse(_validator.IsSupportedFile(""));
        }

        [TestMethod]
        public void IsSupportedFile_CaseInsensitive_ReturnsTrue()
        {
            Assert.IsTrue(_validator.IsSupportedFile("File.CS"));
            Assert.IsTrue(_validator.IsSupportedFile("app.Json"));
        }

        [TestMethod]
        public void IsPickerCandidate_SupportedNonExcluded_ReturnsTrue()
        {
            Assert.IsTrue(_validator.IsPickerCandidate("MyClass.cs"));
            Assert.IsTrue(_validator.IsPickerCandidate("app.module.ts"));
            Assert.IsTrue(_validator.IsPickerCandidate("styles.css"));
        }

        [TestMethod]
        [DataRow("package.json")]
        [DataRow("tsconfig.json")]
        [DataRow("eslint.config.js")]
        [DataRow(".eslintrc.json")]
        [DataRow("webpack.config.js")]
        [DataRow("vite.config.ts")]
        [DataRow("jest.config.js")]
        [DataRow(".gitignore")]
        [DataRow(".editorconfig")]
        [DataRow("tailwind.config.js")]
        [DataRow("angular.json")]
        public void IsPickerCandidate_ExcludedFile_ReturnsFalse(string fileName)
        {
            Assert.IsFalse(_validator.IsPickerCandidate(fileName));
        }

        [TestMethod]
        public void IsPickerCandidate_UnsupportedExtension_ReturnsFalse()
        {
            Assert.IsFalse(_validator.IsPickerCandidate("readme.txt"));
        }

        [TestMethod]
        public void IsPickerCandidate_ExcludedFileCaseInsensitive_ReturnsFalse()
        {
            Assert.IsFalse(_validator.IsPickerCandidate("Package.JSON"));
            Assert.IsFalse(_validator.IsPickerCandidate("TSCONFIG.JSON"));
        }
    }
}
