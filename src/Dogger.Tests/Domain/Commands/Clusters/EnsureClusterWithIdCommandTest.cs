﻿using System.Threading.Tasks;
using Dogger.Domain.Commands.Clusters.EnsureClusterWithId;
using Dogger.Domain.Models;
using Dogger.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dogger.Tests.Domain.Commands.Clusters
{
    [TestClass]
    public class EnsureClusterWithIdCommandTest
    {
        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_MultipleClusters_ReturnsDemoCluster()
        {
            //Arrange
            await using var environment = await IntegrationTestEnvironment.CreateAsync();

            await environment.WithFreshDataContext(async dataContext =>
            {
                await dataContext.Clusters.AddAsync(new Cluster());

                await dataContext.Clusters.AddAsync(new Cluster()
                {
                    Id = DataContext.DemoClusterId
                });
            });

            //Act
            var cluster = await environment.Mediator.Send(new EnsureClusterWithIdCommand(DataContext.DemoClusterId));

            //Assert
            Assert.IsNotNull(cluster);
            Assert.AreEqual(DataContext.DemoClusterId, cluster.Id);
        }

        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_NoClusters_CreatesDemoCluster()
        {
            //Arrange
            await using var environment = await IntegrationTestEnvironment.CreateAsync();

            //Act
            var cluster = await environment.Mediator.Send(new EnsureClusterWithIdCommand(DataContext.DemoClusterId));

            //Assert
            Assert.IsNotNull(cluster);
            Assert.AreEqual(DataContext.DemoClusterId, cluster.Id);
        }
    }
}
