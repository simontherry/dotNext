﻿using DotNext;
using DotNext.Net.Cluster;
using DotNext.Net.Cluster.Consensus.Raft;
using DotNext.Net.Cluster.Consensus.Raft.Http;
using DotNext.Net.Http;
using static System.Globalization.CultureInfo;

namespace RaftNode;

internal sealed class Startup
{
    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration) => this.configuration = configuration;

    private static Task RedirectToLeaderAsync(HttpContext context)
    {
        var cluster = context.RequestServices.GetRequiredService<IRaftCluster>();
        return context.Response.WriteAsync($"Leader address is {cluster.Leader?.EndPoint}. Current address is {context.Connection.LocalIpAddress}:{context.Connection.LocalPort}", context.RequestAborted);
    }

    private static async Task GetValueAsync(HttpContext context)
    {
        var cluster = context.RequestServices.GetRequiredService<IRaftCluster>();
        var provider = context.RequestServices.GetRequiredService<ISupplier<long>>();

        await cluster.ApplyReadBarrierAsync(context.RequestAborted);
        await context.Response.WriteAsync(provider.Invoke().ToString(InvariantCulture), context.RequestAborted);
    }

    public void Configure(IApplicationBuilder app)
    {
        const string LeaderResource = "/leader";
        const string ValueResource = "/value";

        app.UseConsensusProtocolHandler()
            .RedirectToLeader(LeaderResource)
            .UseRouting()
            .UseEndpoints(static endpoints =>
            {
                endpoints.MapGet(LeaderResource, RedirectToLeaderAsync);
                endpoints.MapGet(ValueResource, GetValueAsync);
            });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.UsePersistentConfigurationStorage(Path.Combine(Environment.CurrentDirectory, "state", this.configuration["port"]))
            .ConfigureCluster<ClusterConfigurator>()
            .AddSingleton<IHttpMessageHandlerFactory, RaftClientHandlerFactory>()
            .AddOptions()
            .AddRouting();

        var path = configuration[SimplePersistentState.LogLocation];
        if (!string.IsNullOrWhiteSpace(path))
        {
            services.AddSingleton<AppEventSource>();
            services.UsePersistenceEngine<ISupplier<long>, SimplePersistentState>()
                .AddSingleton<IHostedService, DataModifier>();
        }
    }

    // NOTE: this way of adding members to the cluster is not recommended in production code
    private static void AddClusterMembers(IDictionary<ClusterMemberId, HttpEndPoint> members)
    {
        var address = new HttpEndPoint("localhost", 3262, true);
        members.Add(ClusterMemberId.FromEndPoint(address), address);

        address = new("localhost", 3263, true);
        members.Add(ClusterMemberId.FromEndPoint(address), address);

        address = new("localhost", 3264, true);
        members.Add(ClusterMemberId.FromEndPoint(address), address);
    }
}