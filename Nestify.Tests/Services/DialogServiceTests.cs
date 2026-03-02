using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nestify.Abstractions;
using Nestify.Services;

namespace Nestify.Tests.Services
{
    [TestClass]
    public class DialogServiceTests
    {
        [TestMethod]
        public void Constructor_CreatesInstance()
        {
            var service = new DialogService();

            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Constructor_ImplementsIDialogService()
        {
            var service = new DialogService();

            Assert.IsInstanceOfType(service, typeof(IDialogService));
        }
    }
}
