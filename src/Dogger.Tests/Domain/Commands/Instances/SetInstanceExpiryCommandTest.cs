﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dogger.Domain.Commands.Instances.RegisterInstanceAsProvisioned;
using Dogger.Domain.Commands.Instances.SetInstanceExpiry;
using Dogger.Domain.Models;
using Dogger.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Stripe;

namespace Dogger.Tests.Domain.Commands.Instances
{
    [TestClass]
    public class SetInstanceExpiryCommandTest
    {
        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_InstanceNotFound_ThrowsException()
        {
            //Arrange
            await using var environment = await IntegrationTestEnvironment.CreateAsync();

            //Act
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => 
                await environment.Mediator.Send(new SetInstanceExpiryCommand(
                    "some-instance-name",
                    DateTime.UtcNow.AddDays(1))));

            //Assert
            Assert.IsNotNull(exception);
        }
        
        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_InstanceFound_ExpiryTimeSetInDatabase()
        {
            //Arrange
            await using var environment = await IntegrationTestEnvironment.CreateAsync();

            await environment.WithFreshDataContext(async dataContext =>
            {
                await dataContext.Instances.AddAsync(new Instance()
                {
                    Name = "some-instance-name",
                    PlanId = "dummy",
                    Cluster = new Cluster()
                });
            });

            //Act
            await environment.Mediator.Send(new SetInstanceExpiryCommand(
                "some-instance-name",
                DateTime.UtcNow.AddDays(1)));

            //Assert
            await environment.WithFreshDataContext(async dataContext =>
            {
                var instances = await dataContext
                    .Instances
                    .ToArrayAsync();

                Assert.AreEqual(1, instances.Length);
                Assert.IsNotNull(instances.Single().ExpiresAtUtc);
            });
        }
        
        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_UserNotFound_NoStripeSubscriptionCreated()
        {
            //Arrange
            var fakeSubscriptionService = Substitute.ForPartsOf<SubscriptionService>();
            fakeSubscriptionService
                .CreateAsync(
                    Arg.Any<SubscriptionCreateOptions>(),
                    default,
                    default)
                .Returns((Subscription)null);

            await using var environment = await IntegrationTestEnvironment.CreateAsync(
                new EnvironmentSetupOptions()
                {
                    IocConfiguration = services =>
                    {
                        services.AddSingleton(fakeSubscriptionService);
                    }
                });

            await environment.WithFreshDataContext(async dataContext =>
            {
                await dataContext.Instances.AddAsync(new Instance()
                {
                    Name = "some-instance-name",
                    PlanId = "dummy",
                    Cluster = new Cluster()
                });
            });

            //Act
            await environment.Mediator.Send(new RegisterInstanceAsProvisionedCommand("some-instance-name"));

            //Assert
            await fakeSubscriptionService
                .DidNotReceiveWithAnyArgs()
                .CreateAsync(
                    default,
                    default,
                    default);
        }
        
        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_UserFoundWithNoExistingSubscription_StripeSubscriptionCreatedWithProperDetails()
        {
            //Arrange
            var fakeInstanceId = Guid.NewGuid();
            var fakeClusterId = Guid.NewGuid();

            var fakeSubscriptionService = Substitute.ForPartsOf<SubscriptionService>();
            fakeSubscriptionService
                .CreateAsync(
                    Arg.Any<SubscriptionCreateOptions>(),
                    default,
                    default)
                .Returns(new Subscription()
                {
                    LatestInvoice = new Invoice()
                });

            await using var environment = await IntegrationTestEnvironment.CreateAsync(
                new EnvironmentSetupOptions()
                {
                    IocConfiguration = services =>
                    {
                        services.AddSingleton(fakeSubscriptionService);
                    }
                });

            var existingUser = new User()
            {
                StripeCustomerId = "dummy"
            };
            await environment.WithFreshDataContext(async dataContext =>
            {
                await dataContext.Users.AddAsync(existingUser);

                await dataContext.Instances.AddAsync(new Instance()
                {
                    Id = fakeInstanceId,
                    Name = "some-instance-name",
                    PlanId = "dummy",
                    Cluster = new Cluster()
                    {
                        Id = fakeClusterId,
                        User = existingUser
                    }
                });
            });

            //Act
            await environment.Mediator.Send(new RegisterInstanceAsProvisionedCommand("some-instance-name")
            {
                UserId = existingUser.Id
            });

            //Assert
            await fakeSubscriptionService
                .ReceivedWithAnyArgs(1)
                .CreateAsync(
                    Arg.Is<SubscriptionCreateOptions>(args => 
                        args.Metadata["InstanceId"] == fakeInstanceId.ToString() &&
                        args.Metadata["ClusterId"] == fakeClusterId.ToString() &&
                        args.Metadata["InstanceName"] == "some-instance-name"),
                    default,
                    default);
        }
        
        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_UserFoundWithExistingSubscription_StripeSubscriptionUpdatedWithProperDetails()
        {
            //Arrange
            var fakeInstanceId = Guid.NewGuid();
            var fakeClusterId = Guid.NewGuid();

            var fakeSubscriptionService = Substitute.ForPartsOf<SubscriptionService>();
            fakeSubscriptionService
                .GetAsync(
                    Arg.Is<string>(arg => arg == "some-subscription-id"),
                    default,
                    default)
                .Returns(new Subscription()
                {
                    Items = new StripeList<SubscriptionItem>()
                    {
                        Data = new List<SubscriptionItem>()
                    }
                });

            fakeSubscriptionService
                .UpdateAsync(
                    Arg.Any<string>(),
                    Arg.Any<SubscriptionUpdateOptions>(),
                    default,
                    default)
                .Returns(new Subscription()
                {
                    LatestInvoice = new Invoice()
                });

            await using var environment = await IntegrationTestEnvironment.CreateAsync(
                new EnvironmentSetupOptions()
                {
                    IocConfiguration = services =>
                    {
                        services.AddSingleton(fakeSubscriptionService);
                    }
                });

            var existingUser = new User()
            {
                StripeCustomerId = "dummy",
                StripeSubscriptionId = "some-subscription-id"
            };
            await environment.WithFreshDataContext(async dataContext =>
            {
                await dataContext.Users.AddAsync(existingUser);

                await dataContext.Instances.AddAsync(new Instance()
                {
                    Id = fakeInstanceId,
                    Name = "some-instance-name",
                    PlanId = "dummy",
                    Cluster = new Cluster()
                    {
                        Id = fakeClusterId,
                        User = existingUser
                    }
                });
            });

            //Act
            await environment.Mediator.Send(new RegisterInstanceAsProvisionedCommand("some-instance-name")
            {
                UserId = existingUser.Id
            });

            //Assert
            await fakeSubscriptionService
                .ReceivedWithAnyArgs(1)
                .UpdateAsync(
                    "some-subscription-id",
                    Arg.Is<SubscriptionUpdateOptions>(args =>
                        args.Metadata["InstanceId"] == fakeInstanceId.ToString() &&
                        args.Metadata["ClusterId"] == fakeClusterId.ToString() &&
                        args.Metadata["InstanceName"] == "some-instance-name"),
                    default,
                    default);
        }
        
        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_StripeExceptionThrown_NoDatabaseChangesMade()
        {
            //Arrange
            var fakeSubscriptionService = Substitute.ForPartsOf<SubscriptionService>();
            fakeSubscriptionService
                .CreateAsync(
                    Arg.Any<SubscriptionCreateOptions>(),
                    default,
                    default)
                .Throws(new TestException());

            await using var environment = await IntegrationTestEnvironment.CreateAsync(
                new EnvironmentSetupOptions()
                {
                    IocConfiguration = services =>
                    {
                        services.AddSingleton(fakeSubscriptionService);
                    }
                });

            var existingUser = new User()
            {
                StripeCustomerId = "dummy"
            };
            await environment.WithFreshDataContext(async dataContext =>
            {
                await dataContext.Users.AddAsync(existingUser);

                await dataContext.Instances.AddAsync(new Instance()
                {
                    Name = "some-instance-name",
                    PlanId = "dummy",
                    Cluster = new Cluster()
                    {
                        User = existingUser
                    },
                    IsProvisioned = false
                });
            });

            //Act
            var exception = await Assert.ThrowsExceptionAsync<TestException>(async () =>
                await environment.Mediator.Send(new RegisterInstanceAsProvisionedCommand("some-instance-name")
                {
                    UserId = existingUser.Id
                }));

            //Assert
            await environment.WithFreshDataContext(async dataContext =>
            {
                var instances = await dataContext
                    .Instances
                    .AsNoTracking()
                    .ToArrayAsync();

                Assert.IsNotNull(exception);
                Assert.AreEqual(1, instances.Length);
                Assert.IsFalse(instances.Single().IsProvisioned);
            });
        }
        
        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_SubscriptionRequiresAction_NotImplementedExceptionThrown()
        {
            //Arrange
            var fakeSubscriptionService = Substitute.ForPartsOf<SubscriptionService>();
            fakeSubscriptionService
                .CreateAsync(
                    Arg.Any<SubscriptionCreateOptions>(),
                    default,
                    default)
                .Returns(new Subscription()
                {
                    LatestInvoice = new Invoice()
                    {
                        PaymentIntent = new PaymentIntent()
                        {
                            Status = "requires_action"
                        }
                    }
                });

            await using var environment = await IntegrationTestEnvironment.CreateAsync(
                new EnvironmentSetupOptions()
                {
                    IocConfiguration = services =>
                    {
                        services.AddSingleton(fakeSubscriptionService);
                    }
                });

            var existingUser = new User()
            {
                StripeCustomerId = "dummy"
            };
            await environment.WithFreshDataContext(async dataContext =>
            {
                await dataContext.Users.AddAsync(existingUser);

                await dataContext.Instances.AddAsync(new Instance()
                {
                    Name = "some-instance-name",
                    PlanId = "dummy",
                    Cluster = new Cluster()
                    {
                        User = existingUser
                    }
                });
            });

            //Act
            var exception = await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
                await environment.Mediator.Send(new RegisterInstanceAsProvisionedCommand("some-instance-name")
                {
                    UserId = existingUser.Id
                }));

            //Assert
            Assert.IsNotNull(exception);
        }
    }
}
