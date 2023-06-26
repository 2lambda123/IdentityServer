using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Duende.IdentityServer;

/// <summary>
/// Constants for tracing
/// </summary>
public static class Telemetry
{
    private static readonly Version AssemblyVersion = typeof(Telemetry).Assembly.GetName().Version;
    public static string ServiceName => "Duende.IdentityServer";

    /// <summary>
    /// Service version
    /// </summary>
    public static string ServiceVersion => $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}.{AssemblyVersion.Build}";

    /// <summary>
    /// Base ActivitySource
    /// </summary>
    public static ActivitySource BasicActivitySource { get; } = new(
        TraceNames.Basic,
        ServiceVersion);

    /// <summary>
    /// Store ActivitySource
    /// </summary>
    public static ActivitySource StoreActivitySource { get; } = new(
        TraceNames.Store,
        ServiceVersion);
    
    /// <summary>
    /// Cache ActivitySource
    /// </summary>
    public static ActivitySource CacheActivitySource { get; } = new(
        TraceNames.Cache,
        ServiceVersion);
    
    /// <summary>
    /// Cache ActivitySource
    /// </summary>
    public static ActivitySource ServiceActivitySource { get; } = new(
        TraceNames.Services,
        ServiceVersion);
    
    /// <summary>
    /// Detailed validation ActivitySource
    /// </summary>
    public static ActivitySource ValidationActivitySource { get; } = new(
        TraceNames.Validation,
        ServiceVersion);
    
    public static class Metrics
    {
        public static readonly Meter Meter = new(Telemetry.ServiceName, Telemetry.ServiceVersion);
    
        public static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>("requests.total");
        public static readonly Counter<long> DiscoveryRequestCounter = Meter.CreateCounter<long>("discovery_requests.total");
        
        public static readonly Counter<long> TokenIssuedSuccess = Meter.CreateCounter<long>("token_issued_success.total");
        
        
    }
    
    public static class TraceNames
    {
        /// <summary>
        /// Service name for base traces
        /// </summary>
        public static string Basic => Telemetry.ServiceName;

        /// <summary>
        /// Service name for store traces
        /// </summary>
        public static string Store => Basic + ".Stores";
    
        /// <summary>
        /// Service name for caching traces
        /// </summary>
        public static string Cache => Basic + ".Cache";
    
        /// <summary>
        /// Service name for caching traces
        /// </summary>
        public static string Services => Basic + ".Services";
        
        /// <summary>
        /// Service name for detailed validation traces
        /// </summary>
        public static string Validation => Basic + ".Validation";
    }
    
    public static class Properties
    {
        public const string EndpointType = "endpoint_type";

        public const string ClientId = "client_id";
        public const string GrantType = "grant_type";
        public const string Scope = "scope";
        public const string Resource = "resource";
        public const string TokenType = "token_type";
        
        public const string Origin = "origin";
        public const string Scheme = "scheme";
        public const string Type = "type";
        public const string Id = "id";
        public const string ScopeNames = "scope_names";
        public const string ApiResourceNames = "api_resource_names";
        
    }
}