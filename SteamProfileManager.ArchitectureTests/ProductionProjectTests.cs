using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using NUnit.Framework;

namespace SteamProfileManager.ArchitectureTests
{
    [TestFixture]
    public sealed class ProductionProjectTests
    {
        private static readonly Assembly productionAssembly = typeof(Program).Assembly;

        private static readonly XDocument productionProject = XDocument.Load(
            Path.Combine(AppContext.BaseDirectory, "ProductionProjects", "SteamProfileManager.csproj"));

        [Test]
        public void GivenProductionAssembly_WhenCheckingReferences_ThenTestDependenciesAreAbsent()
        {
            AssemblyName[] references = productionAssembly.GetReferencedAssemblies();

            Assert.That(references, Is.Not.Empty, "Production assembly references must be inspected");
            Assert.That(
                references.Where(reference => IsTestDependency(reference.Name!)),
                Is.Empty,
                "Production must not reference test assemblies or test frameworks");
        }

        [Test]
        public void GivenProductionProject_WhenCheckingReferences_ThenTestDependenciesAreAbsent()
        {
            XElement[] references = productionProject.Descendants()
                .Where(element => element.Name.LocalName is "ProjectReference" or "PackageReference" or "Reference")
                .ToArray();

            Assert.That(references, Is.Not.Empty, "Declared production references must be inspected");
            Assert.That(
                references.Where(reference => IsTestDependency((string?)reference.Attribute("Include") ?? "")),
                Is.Empty,
                "Production project declarations must not reference test projects or test packages");
        }

        [Test]
        public void GivenProductionAssembly_WhenCheckingVisibility_ThenInternalsAreNotExposed()
            => Assert.That(
                productionAssembly.GetCustomAttributes<InternalsVisibleToAttribute>(),
                Is.Empty,
                "Production internals must not be exposed to other assemblies, including tests");

        [Test]
        public void GivenProductionProject_WhenCheckingVisibility_ThenInternalsAreNotExposed()
            => Assert.That(
                productionProject.Descendants().Where(element =>
                    string.Equals(element.Name.LocalName, "InternalsVisibleTo") ||
                    (string.Equals(element.Name.LocalName, "AssemblyAttribute") &&
                    ((string?)element.Attribute("Include") ?? "").Contains("InternalsVisibleTo"))),
                Is.Empty,
                "Production project declarations must not grant access to internal members");

        private static bool IsTestDependency(string dependencyName)
            => dependencyName.Contains("Tests", StringComparison.OrdinalIgnoreCase) ||
                dependencyName.Contains("TestPlatform", StringComparison.OrdinalIgnoreCase) ||
                dependencyName.Contains("Test.Sdk", StringComparison.OrdinalIgnoreCase) ||
                dependencyName.Contains("NUnit", StringComparison.OrdinalIgnoreCase) ||
                dependencyName.Contains("xunit", StringComparison.OrdinalIgnoreCase) ||
                dependencyName.Contains("Moq", StringComparison.OrdinalIgnoreCase) ||
                dependencyName.Contains("ArchUnit", StringComparison.OrdinalIgnoreCase);
    }
}