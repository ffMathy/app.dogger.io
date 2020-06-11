﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dogger.Domain.Commands.Instances.DeleteInstanceByName;
using Dogger.Domain.Models;
using Dogger.Domain.Queries.PullDog.GetAvailableClusterFromPullRequest;
using MediatR;

namespace Dogger.Domain.Commands.PullDog.EnsurePullDogDatabaseInstance
{
    public class EnsurePullDogDatabaseInstanceCommandHandler : IRequestHandler<EnsurePullDogDatabaseInstanceCommand, Instance>
    {
        private readonly IMediator mediator;

        private readonly DataContext dataContext;

        public EnsurePullDogDatabaseInstanceCommandHandler(
            IMediator mediator,
            DataContext dataContext)
        {
            this.mediator = mediator;
            this.dataContext = dataContext;
        }

        public async Task<Instance> Handle(EnsurePullDogDatabaseInstanceCommand request, CancellationToken cancellationToken)
        {
            var pullRequest = request.PullRequest;

            var cluster = await this.mediator.Send(
                new GetAvailableClusterFromPullRequestQuery(pullRequest),
                cancellationToken);

            var settings = pullRequest.PullDogRepository.PullDogSettings;
            var user = settings.User;

            var expiryDuration = request.Configuration.Expiry;
            var expiryTime = expiryDuration.TotalMinutes < 1 ?
                (DateTime?)null :
                DateTime.UtcNow.Add(expiryDuration);

            try
            {
                var existingInstance = cluster
                    .Instances
                    .SingleOrDefault(x =>
                        x.PullDogPullRequest == pullRequest);
                if (existingInstance != null)
                {
                    existingInstance.ExpiresAtUtc = expiryTime;
                    return existingInstance;
                }

                var newInstance = new Instance()
                {
                    Name = $"pull-dog_{user.Id}_{Guid.NewGuid()}",
                    Cluster = cluster,
                    IsProvisioned = false,
                    PlanId = settings.PlanId,
                    Type = InstanceType.DockerCompose,
                    PullDogPullRequest = pullRequest,
                    ExpiresAtUtc = expiryTime
                };
                pullRequest.Instance = newInstance;

                cluster.Instances.Add(newInstance);
                await this.dataContext.Instances.AddAsync(newInstance, cancellationToken);

                return newInstance;
            }
            finally
            {
                await this.dataContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
