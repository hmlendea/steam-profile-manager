using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;

using NUnit.Framework;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace SteamProfileManager.ArchitectureTests
{
    [TestFixture]
    public sealed class DependencyTests
    {
        private static readonly Architecture architecture = new ArchLoader()
            .LoadAssemblies(typeof(Program).Assembly)
            .Build();

        [TestCase("SteamProfileManager.Configuration", "SteamProfileManager.Service")]
        [TestCase("SteamProfileManager.Configuration", "SteamProfileManager.Logging")]
        [TestCase("SteamProfileManager.Logging", "SteamProfileManager.Configuration")]
        [TestCase("SteamProfileManager.Logging", "SteamProfileManager.Service")]
        public void GivenIndependentComponents_WhenCheckingDependencies_ThenForbiddenDirectionsAreAbsent(
            string sourceNamespace,
            string forbiddenNamespace)
        {
            IObjectProvider<IType> source = Types().That().ResideInNamespace(sourceNamespace);
            IObjectProvider<IType> forbidden = Types().That().ResideInNamespace(forbiddenNamespace);

            Assert.That(source.GetObjects(architecture), Is.Not.Empty, sourceNamespace);
            Assert.That(forbidden.GetObjects(architecture), Is.Not.Empty, forbiddenNamespace);

            Types().That().Are(source).Should().NotDependOnAny(forbidden)
                .Because("Configuration and Logging are independent of services and each other")
                .Check(architecture);
        }

        [TestCase("SteamProfileManager.Configuration")]
        [TestCase("SteamProfileManager.Logging")]
        [TestCase("SteamProfileManager.Service")]
        public void GivenApplicationComponents_WhenCheckingDependencies_ThenTheEntryPointIsNotReferenced(
            string sourceNamespace)
        {
            IObjectProvider<IType> source = Types().That().ResideInNamespace(sourceNamespace);
            IObjectProvider<IType> entryPoint = Types().That().HaveFullName(typeof(Program).FullName!);

            Assert.That(source.GetObjects(architecture), Is.Not.Empty, sourceNamespace);
            Assert.That(entryPoint.GetObjects(architecture), Is.Not.Empty, "Program must be loaded");

            Types().That().Are(source).Should().NotDependOnAny(entryPoint)
                .Because("only Program owns application lifecycle and composition")
                .Check(architecture);
        }

        [TestCase("OpenQA.Selenium")]
        [TestCase("Microsoft.Extensions.Configuration")]
        [TestCase("Microsoft.Extensions.DependencyInjection")]
        [TestCase("System.Net")]
        [TestCase("System.Net.Http")]
        public void GivenServices_WhenCheckingDependencies_ThenInfrastructureIsEncapsulated(
            string forbiddenNamespace)
        {
            IObjectProvider<IType> services = Types().That()
                .ResideInNamespace("SteamProfileManager.Service");

            // FileDownloader is the documented HTTP integration adapter, not a business service.
            if (string.Equals(forbiddenNamespace, "System.Net"))
            {
                services = Types().That().Are(services).And()
                    .DoNotHaveFullName("SteamProfileManager.Service.FileDownloader");
            }

            Assert.That(services.GetObjects(architecture), Is.Not.Empty, "Services must be loaded");

            Types().That().Are(services).Should()
                .NotDependOnAny(Types().That().ResideInNamespace(forbiddenNamespace))
                .Because("services use injected settings and abstractions; only FileDownloader owns HTTP")
                .Check(architecture);
        }
    }
}