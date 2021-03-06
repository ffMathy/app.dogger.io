﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace Dogger.Controllers.PullDog.Api
{
    [ExcludeFromCodeCoverage]
    public class RepositoryResponse
    {
        public Guid? PullDogId { get; set; }

        public string? Handle { get; set; }
        public string? Name { get; set; }
    }
}
