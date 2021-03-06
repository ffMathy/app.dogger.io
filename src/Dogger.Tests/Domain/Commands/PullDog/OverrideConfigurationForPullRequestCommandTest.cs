﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dogger.Domain.Commands.PullDog.OverrideConfigurationForPullRequest;
using Dogger.Domain.Models;
using Dogger.Domain.Services.PullDog;
using Dogger.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dogger.Tests.Domain.Commands.PullDog
{
    [TestClass]
    public class OverrideConfigurationForPullRequestCommandTest
    {
        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        public async Task Handle_PullRequestGiven_ConfigurationOverrideUpdatedOnPullRequest()
        {
            //Arrange
            await using var environment = await IntegrationTestEnvironment.CreateAsync();

            var pullRequest = new PullDogPullRequest()
            {
                Handle = "some-handle",
                PullDogRepository = new PullDogRepository()
                {
                    Handle = "dummy",
                    PullDogSettings = new PullDogSettings()
                    {
                        EncryptedApiKey = Array.Empty<byte>(),
                        PlanId = "dummy",
                        User = new User()
                        {
                            StripeCustomerId = "dummy"
                        }
                    }
                }
            };

            await environment.WithFreshDataContext(async dataContext =>
            {
                await dataContext.PullDogPullRequests.AddAsync(pullRequest);
            });

            //Act
            await environment.Mediator.Send(new OverrideConfigurationForPullRequestCommand(
                pullRequest.Id,
                new ConfigurationFileOverride()
                {
                    BuildArguments = new Dictionary<string, string>()
                {
                    {
                        "foo", "bar"
                    }
                }
                }));

            //Assert
            await environment.WithFreshDataContext(async dataContext =>
            {
                var refreshedPullRequest = await dataContext
                    .PullDogPullRequests
                    .SingleAsync();
                Assert.IsNotNull(refreshedPullRequest);

                Assert.AreEqual("some-handle", refreshedPullRequest.Handle);
                Assert.AreEqual("bar", refreshedPullRequest.ConfigurationOverride?.BuildArguments["foo"]);
            });
        }
    }
}
