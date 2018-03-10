using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ECInternet.Connectors45.Authorize_Net.Test
{
    /// <summary>
    /// Unit tests for the LicenseProvider class.
    /// </summary>
    [TestClass]
    public class LicenseProviderTest : ConnectorTest
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        [TestMethod]
        public void EncryptActivationTest()
        {
            const string ACTIVATION_CODE = "This is a test String";

            //var licenseProvider = new ConnectorLicenseProvider();
            //string result = licenseProvider.EncryptActivation(ACTIVATION_CODE);

            //Assert.IsTrue(result != ACTIVATION_CODE);
        }

        [TestMethod]
        public void ValidateTest()
        {
            //Instantiate the connector license. Even of there have been no license details saved this will return a valid
            //license object.
            //CustomConnectorLicense persistedLicense = CustomConnectorFactory.GetConnectorLicense(TestConstants.ASSEMBLY_NAME, string.Empty);

            //Perform the test.
            //var licenseProvider = new ConnectorLicenseProvider();
            //licenseProvider.Validate(persistedLicense, false);
        }
    }
}
