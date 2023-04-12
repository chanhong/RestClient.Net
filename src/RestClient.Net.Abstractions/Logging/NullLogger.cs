﻿#pragma warning disable IDE0130 // Namespace does not match folder structure

using RestClient.Net;
using System;

namespace Microsoft.Extensions.Logging.Abstractions
{
    public class NullLogger : ILogger
    {
        public static NullLogger Instance { get; } = new();

        public IDisposable BeginScope(string messageFormat, params object[] args) => new DummyDisposable();

        public void LogDebug(string message, params object[] args)
        {
        }

        public void LogError(EventId eventId, Exception exception, string message, params object[] args)
        {
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
        }

        public void LogInformation(string message, params object[] args)
        {
        }

        public void LogTrace(string message, params object[] args) { }

        public void LogWarning(string message, params object[] args)
        {
        }
    }
}
