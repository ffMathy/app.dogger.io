﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Dogger.Tests.TestHelpers;

namespace Dogger.Tests.Controllers
{
    [TestClass]
    public class ControllersTest
    {
        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public void AllControllersCanBeResolved()
        {
            //Arrange
            var controllerTypes = typeof(Startup)
                .Assembly
                .GetTypes()
                .Where(x => x.IsClass)
                .Where(x => x.Name.EndsWith(nameof(Controller)));

            var serviceProvider = TestServiceProviderFactory.CreateUsingStartup();

            //Act
            var controllerInstances = controllerTypes
                .Select(stateType => serviceProvider.GetRequiredService(stateType))
                .ToArray();

            //Assert
            Assert.AreNotEqual(0, controllerInstances.Length);

            foreach(var stateInstance in controllerInstances)
            {
                Assert.IsNotNull(stateInstance);
            }
        }
    }
}
