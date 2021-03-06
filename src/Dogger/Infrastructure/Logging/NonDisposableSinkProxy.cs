﻿using System;
using Serilog.Core;
using Serilog.Events;

namespace Dogger.Infrastructure.Logging
{
    public class NonDisposableSinkProxy : ILogEventSink, IDisposable
    {
        private readonly ILogEventSink inner;

        public NonDisposableSinkProxy(
            ILogEventSink inner)
        {
            this.inner = inner;
        }

        public void Emit(LogEvent logEvent)
        {
            this.inner.Emit(logEvent);
        }

        public void Dispose()
        {
        }
    }
}
