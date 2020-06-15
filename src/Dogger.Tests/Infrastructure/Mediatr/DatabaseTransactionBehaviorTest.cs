﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dogger.Domain.Models;
using Dogger.Tests.TestHelpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dogger.Tests.Infrastructure.Mediatr
{
    [TestClass]
    public class DatabaseTransactionBehaviorTest
    {
        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task Handle_NestedTransactionsOuterExceptionThrown_InnerAndOuterTransactionContentsReverted()
        {
            //Arrange
            await using var environment = await IntegrationTestEnvironment.CreateAsync();

            //Act
            var exception = await Assert.ThrowsExceptionAsync<TestException>(async () =>
            {
                await environment.Mediator.Send(new TestCommand(async () =>
                {
                    await environment.DataContext.Clusters.AddAsync(new Cluster()
                    {
                        Name = "outer"
                    });
                    await environment.DataContext.SaveChangesAsync();

                    await environment.Mediator.Send(new TestCommand(async () =>
                    {
                        await environment.DataContext.Clusters.AddAsync(new Cluster()
                        {
                            Name = "inner"
                        });
                        await environment.DataContext.SaveChangesAsync();
                    }));

                    throw new TestException();
                }));
            });

            //Assert
            Assert.IsNotNull(exception);

            await environment.WithFreshDataContext(async (dataContext) =>
            {
                var clusterCount = await dataContext.Clusters.CountAsync();
                Assert.AreEqual(0, clusterCount);
            });
        }

        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task Handle_NestedTransactionsWithNoException_InnerAndOuterTransactionContentsSaved()
        {
            //Arrange
            await using var environment = await IntegrationTestEnvironment.CreateAsync();

            //Act
            await environment.Mediator.Send(new TestCommand(async () =>
            {
                await environment.DataContext.Clusters.AddAsync(new Cluster()
                {
                    Name = "outer"
                });
                await environment.DataContext.SaveChangesAsync();

                await environment.Mediator.Send(new TestCommand(async () =>
                {
                    await environment.DataContext.Clusters.AddAsync(new Cluster()
                    {
                        Name = "inner"
                    });
                    await environment.DataContext.SaveChangesAsync();
                }));
            }));

            //Assert
            await environment.WithFreshDataContext(async (dataContext) =>
            {
                var clusterCount = await dataContext.Clusters.CountAsync();
                Assert.AreEqual(2, clusterCount);
            });
        }

        [TestMethod]
        [TestCategory(TestCategories.IntegrationCategory)]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task Handle_NestedTransactionsInnerExceptionThrown_InnerAndOuterTransactionContentsReverted()
        {
            //Arrange
            await using var environment = await IntegrationTestEnvironment.CreateAsync();

            //Act
            var exception = await Assert.ThrowsExceptionAsync<TestException>(async () =>
            {
                await environment.Mediator.Send(new TestCommand(async () =>
                {
                    await environment.DataContext.Clusters.AddAsync(new Cluster()
                    {
                        Name = "outer"
                    });
                    await environment.DataContext.SaveChangesAsync();

                    await environment.Mediator.Send(new TestCommand(async () =>
                    {
                        await environment.DataContext.Clusters.AddAsync(new Cluster()
                        {
                            Name = "inner"
                        });
                        await environment.DataContext.SaveChangesAsync();

                        throw new TestException();
                    }));
                }));
            });

            //Assert
            Assert.IsNotNull(exception);

            await environment.WithFreshDataContext(async (dataContext) =>
            {
                var clusterCount = await dataContext.Clusters.CountAsync();
                Assert.AreEqual(0, clusterCount);
            });
        }

        public class TestCommand : IRequest
        {
            public Func<Task> Action { get; }

            public TestCommand(
                Func<Task> action)
            {
                this.Action = action;
            }
        }

        public class TestCommandHandler : IRequestHandler<TestCommand>
        {
            public async Task<Unit> Handle(TestCommand request, CancellationToken cancellationToken)
            {
                await request.Action();
                return Unit.Value;
            }
        }
    }
}
