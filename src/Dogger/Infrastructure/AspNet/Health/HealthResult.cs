﻿using System;
using System.Collections.Generic;

namespace Dogger.Infrastructure.AspNet.Health
{
    public class HealthResult
    {
        public string? Status { get; set; }
        public TimeSpan? Duration { get; set; }
        public ICollection<HealthInformation>? Information { get; set; }
    }
}
