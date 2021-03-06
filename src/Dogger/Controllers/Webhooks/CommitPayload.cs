﻿using System.Diagnostics.CodeAnalysis;

namespace Dogger.Controllers.Webhooks
{
    [ExcludeFromCodeCoverage]
    public class CommitPayload
    {
        public string? Message { get; set; }

        public string[]? Added { get; set; }
        public string[]? Removed { get; set; }
        public string[]? Modified { get; set; }
    }
}
