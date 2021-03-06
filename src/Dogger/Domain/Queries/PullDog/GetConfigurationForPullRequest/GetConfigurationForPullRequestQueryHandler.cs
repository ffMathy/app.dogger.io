﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dogger.Domain.Services.PullDog;
using MediatR;

namespace Dogger.Domain.Queries.PullDog.GetConfigurationForPullRequest
{
    public class GetConfigurationForPullRequestQueryHandler : IRequestHandler<GetConfigurationForPullRequestQuery, ConfigurationFile>
    {
        private readonly IPullDogFileCollectorFactory pullDogFileCollectorFactory;

        public GetConfigurationForPullRequestQueryHandler(
            IPullDogFileCollectorFactory pullDogFileCollectorFactory)
        {
            this.pullDogFileCollectorFactory = pullDogFileCollectorFactory;
        }

        public async Task<ConfigurationFile> Handle(GetConfigurationForPullRequestQuery request, CancellationToken cancellationToken)
        {
            var client = await this.pullDogFileCollectorFactory.CreateAsync(request.PullRequest);

            var configuration = await client.GetConfigurationFileAsync() ?? new ConfigurationFile();

            var configurationOverride = request.PullRequest.ConfigurationOverride;
            if (configurationOverride == null)
                return configuration;

            ApplyOverridesToConfiguration(
                configurationOverride, 
                configuration);

            return configuration;
        }

        private static void ApplyOverridesToConfiguration(
            ConfigurationFileOverride configurationOverride, 
            ConfigurationFile configuration)
        {
            configuration.IsLazy = false;

            if (configurationOverride.BuildArguments != null)
                configuration.BuildArguments = configurationOverride.BuildArguments;

            if (configurationOverride.ConversationMode != default)
                configuration.ConversationMode = configurationOverride.ConversationMode;

            if (configurationOverride.Expiry != default)
                configuration.Expiry = configurationOverride.Expiry;
        }
    }
}
