﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dogger.Domain.Models;
using Dogger.Domain.Queries.Amazon.Lightsail.GetLightsailInstanceByName;
using Dogger.Domain.Queries.Instances.GetProvisionedClustersWithInstancesForUser;
using Dogger.Tests.TestHelpers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Dogger.Tests.Domain.Queries.Instances
{
    [TestClass]
    public class GetProvisionedClustersWithInstancesForUserQueryTest
    {
        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_NoInstancesPresent_NoLightsailInstancesReturned()
        {
            //Arrange
            await using var environment = await IntegrationTestEnvironment.CreateAsync();

            //Act
            var instances = await environment.Mediator.Send(
                new GetProvisionedClustersWithInstancesForUserQuery(
                    Guid.NewGuid()));

            //Assert
            Assert.IsNotNull(instances);
            Assert.AreEqual(0, instances.Count);
        }

        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_MultipleProvisionedInstancesButNoProvisionedPresent_NoLightsailInstancesReturned()
        {
            //Arrange
            var fakeGetInstanceByNameQueryHandler = Substitute.For<IRequestHandler<GetLightsailInstanceByNameQuery, global::Amazon.Lightsail.Model.Instance>>();
            fakeGetInstanceByNameQueryHandler
                .Handle(
                    Arg.Is<GetLightsailInstanceByNameQuery>(
                        arg => arg.Name == "some-instance-1"),
                    default)
                .Returns(new global::Amazon.Lightsail.Model.Instance()
                {
                    Name = "some-instance-1"
                });

            fakeGetInstanceByNameQueryHandler
                .Handle(
                    Arg.Is<GetLightsailInstanceByNameQuery>(
                        arg => arg.Name == "some-instance-2"),
                    default)
                .Returns(new global::Amazon.Lightsail.Model.Instance()
                {
                    Name = "some-instance-2"
                });

            await using var environment = await IntegrationTestEnvironment.CreateAsync(new EnvironmentSetupOptions()
            {
                IocConfiguration = services =>
                {
                    services.AddSingleton(fakeGetInstanceByNameQueryHandler);
                }
            });

            var userId = Guid.NewGuid();

            await environment.WithFreshDataContext(async dataContext =>
            {
                await dataContext.Users.AddAsync(new User()
                {
                    Id = userId,
                    StripeCustomerId = "dummy",
                    Clusters = new List<Cluster>()
                    {
                        new Cluster()
                        {
                            Instances = new List<Instance>()
                            {
                                new Instance()
                                {
                                    Name = "some-instance-1",
                                    PlanId = "dummy",
                                    IsProvisioned = false
                                },
                                new Instance()
                                {
                                    Name = "some-instance-2",
                                    PlanId = "dummy",
                                    IsProvisioned = false
                                }
                            }
                        }
                    }
                });
            });

            //Act
            var clusters = await environment.Mediator.Send(
                new GetProvisionedClustersWithInstancesForUserQuery(userId));

            //Assert
            Assert.IsNotNull(clusters);
            Assert.AreEqual(0, clusters.Count);

            await fakeGetInstanceByNameQueryHandler
                .DidNotReceive()
                .Handle(
                    Arg.Is<GetLightsailInstanceByNameQuery>(
                        arg => arg.Name == "some-instance-1"),
                    default);

            await fakeGetInstanceByNameQueryHandler
                .DidNotReceive()
                .Handle(
                    Arg.Is<GetLightsailInstanceByNameQuery>(
                        arg => arg.Name == "some-instance-2"),
                    default);
        }

        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_MultipleProvisionedInstancesButOnlyOnceProvisionedPresent_MultipleLightsailInstancesReturned()
        {
            //Arrange
            var fakeGetInstanceByNameQueryHandler = Substitute.For<IRequestHandler<GetLightsailInstanceByNameQuery, global::Amazon.Lightsail.Model.Instance>>();
            fakeGetInstanceByNameQueryHandler
                .Handle(
                    Arg.Is<GetLightsailInstanceByNameQuery>(
                        arg => arg.Name == "some-instance-1"),
                    default)
                .Returns(new global::Amazon.Lightsail.Model.Instance()
                {
                    Name = "some-instance-1"
                });

            fakeGetInstanceByNameQueryHandler
                .Handle(
                    Arg.Is<GetLightsailInstanceByNameQuery>(
                        arg => arg.Name == "some-instance-2"),
                    default)
                .Returns(new global::Amazon.Lightsail.Model.Instance()
                {
                    Name = "some-instance-2"
                });

            await using var environment = await IntegrationTestEnvironment.CreateAsync(new EnvironmentSetupOptions()
            {
                IocConfiguration = services =>
                {
                    services.AddSingleton(fakeGetInstanceByNameQueryHandler);
                }
            });

            var userId = Guid.NewGuid();

            await environment.WithFreshDataContext(async dataContext =>
            {
                await dataContext.Users.AddAsync(new User()
                {
                    Id = userId,
                    StripeCustomerId = "dummy",
                    Clusters = new List<Cluster>()
                    {
                        new Cluster()
                        {
                            Instances = new List<Instance>()
                            {
                                new Instance()
                                {
                                    Name = "some-instance-1",
                                    PlanId = "dummy",
                                    IsProvisioned = true
                                },
                                new Instance()
                                {
                                    Name = "some-instance-2",
                                    PlanId = "dummy",
                                    IsProvisioned = true
                                }
                            }
                        }
                    }
                });
            });

            //Act
            var clusters = await environment.Mediator.Send(
                new GetProvisionedClustersWithInstancesForUserQuery(userId));

            //Assert
            Assert.IsNotNull(clusters);
            Assert.AreEqual(1, clusters.Count);

            var cluster = clusters.Single();
            var instances = cluster.Instances.ToArray();

            Assert.IsNotNull(instances.First().DatabaseModel);
            Assert.IsNotNull(instances.First().AmazonModel);

            Assert.IsNotNull(instances.Last().DatabaseModel);
            Assert.IsNotNull(instances.Last().AmazonModel);

            await fakeGetInstanceByNameQueryHandler
                .Received(1)
                .Handle(
                    Arg.Is<GetLightsailInstanceByNameQuery>(
                        arg => arg.Name == "some-instance-1"),
                    default);

            await fakeGetInstanceByNameQueryHandler
                .Received(1)
                .Handle(
                    Arg.Is<GetLightsailInstanceByNameQuery>(
                        arg => arg.Name == "some-instance-2"),
                    default);
        }

        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_MultipleProvisionedInstancesPresent_MultipleLightsailInstancesReturned()
        {
            //Arrange
            var fakeGetInstanceByNameQueryHandler = Substitute.For<IRequestHandler<GetLightsailInstanceByNameQuery, global::Amazon.Lightsail.Model.Instance>>();
            fakeGetInstanceByNameQueryHandler
                .Handle(
                    Arg.Is<GetLightsailInstanceByNameQuery>(
                        arg => arg.Name == "some-instance-1"),
                    default)
                .Returns(new global::Amazon.Lightsail.Model.Instance()
                {
                    Name = "some-instance-1"
                });

            fakeGetInstanceByNameQueryHandler
                .Handle(
                    Arg.Is<GetLightsailInstanceByNameQuery>(
                        arg => arg.Name == "some-instance-2"),
                    default)
                .Returns(new global::Amazon.Lightsail.Model.Instance()
                {
                    Name = "some-instance-2"
                });

            await using var environment = await IntegrationTestEnvironment.CreateAsync(new EnvironmentSetupOptions()
            {
                IocConfiguration = services =>
                {
                    services.AddSingleton(fakeGetInstanceByNameQueryHandler);
                }
            });

            var userId = Guid.NewGuid();

            await environment.WithFreshDataContext(async dataContext =>
            {
                await dataContext.Users.AddAsync(new User()
                {
                    Id = userId,
                    StripeCustomerId = "dummy",
                    Clusters = new List<Cluster>()
                    {
                        new Cluster()
                        {
                            Instances = new List<Instance>()
                            {
                                new Instance()
                                {
                                    Name = "some-instance-1",
                                    PlanId = "dummy",
                                    IsProvisioned = true
                                },
                                new Instance()
                                {
                                    Name = "some-instance-2",
                                    PlanId = "dummy",
                                    IsProvisioned = true
                                }
                            }
                        }
                    }
                });
            });

            //Act
            var clusters = await environment.Mediator.Send(
                new GetProvisionedClustersWithInstancesForUserQuery(userId));

            //Assert
            Assert.IsNotNull(clusters);
            Assert.AreEqual(1, clusters.Count);

            var cluster = clusters.Single();
            var instances = cluster.Instances.ToArray();

            Assert.IsNotNull(instances.First().DatabaseModel);
            Assert.IsNotNull(instances.First().AmazonModel);

            Assert.IsNotNull(instances.Last().DatabaseModel);
            Assert.IsNotNull(instances.Last().AmazonModel);

            await fakeGetInstanceByNameQueryHandler
                .Received(1)
                .Handle(
                    Arg.Is<GetLightsailInstanceByNameQuery>(
                        arg => arg.Name == "some-instance-1"),
                    default);

            await fakeGetInstanceByNameQueryHandler
                .Received(1)
                .Handle(
                    Arg.Is<GetLightsailInstanceByNameQuery>(
                        arg => arg.Name == "some-instance-2"),
                    default);
        }
    }
}
