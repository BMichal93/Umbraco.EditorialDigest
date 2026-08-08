using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Umbraco.Cms.Tests.Integration;

[assembly: SuppressMessage(
    "Design",
    "CA1050:Declare types in namespaces",
    Justification = "NUnit applies the setup fixture assembly-wide only when it has no namespace.")]

[SetUpFixture]
public sealed class GlobalTestSetup
{
    private GlobalSetupTeardown? _setupTearDown;
    private bool _isInitialized;

    [OneTimeSetUp]
    public void SetUp()
    {
        _setupTearDown = new GlobalSetupTeardown();
        _setupTearDown.SetUp();
        _isInitialized = true;
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        if (_isInitialized)
        {
            _setupTearDown?.TearDown();
        }
    }
}
