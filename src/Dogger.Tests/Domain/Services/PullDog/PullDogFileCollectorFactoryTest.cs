﻿using System.Threading.Tasks;
using Dogger.Domain.Models;
using Dogger.Domain.Services.PullDog;
using Dogger.Infrastructure.Docker.Yml;
using Dogger.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Serilog;

namespace Dogger.Tests.Domain.Services.PullDog
{
    [TestClass]
    public class PullDogFileCollectorFactoryTest
    {
        [TestMethod]
        [TestCategory(TestCategories.UnitCategory)]
        public async Task Create_PullRequestGiven_CallsRepositoryClientFactory()
        {
            //Arrange
            var fakePullDogRepositoryClientFactory = Substitute.For<IPullDogRepositoryClientFactory>();
            var fakeDockerComposeParserFactory = Substitute.For<IDockerComposeParserFactory>();

            var factory = new PullDogFileCollectorFactory(
                fakePullDogRepositoryClientFactory,
                fakeDockerComposeParserFactory,
                Substitute.For<ILogger>());

            var pullDogPullRequest = new PullDogPullRequest();

            //Act
            var collector = await factory.CreateAsync(pullDogPullRequest);

            //Assert
            Assert.IsNotNull(collector);

            await fakePullDogRepositoryClientFactory
                .Received(1)
                .CreateAsync(pullDogPullRequest);
        }
    }
}
