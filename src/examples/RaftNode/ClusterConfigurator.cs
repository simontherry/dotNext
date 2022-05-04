﻿using DotNext.Net.Cluster;
using DotNext.Net.Cluster.Consensus.Raft;
using System.Diagnostics;

using DotNext.Net;

namespace RaftNode;

internal sealed class ClusterConfigurator : IClusterMemberLifetime
{
    internal static void LeaderChanged(ICluster cluster, IClusterMember? leader)
    {
        Debug.Assert(cluster is IRaftCluster);
        var term = ((IRaftCluster)cluster).Term;
        var timeout = ((IRaftCluster)cluster).ElectionTimeout;
        Console.WriteLine(leader is null
            ? "Consensus cannot be reached"
            : $"New cluster leader is elected. Leader address is {leader.EndPoint}");
        Console.WriteLine($"Term of local cluster member is {term}. Election timeout {timeout}");
    }

    private static void PeerDiscovered(IPeerMesh mesh, PeerEventArgs args)
    {
        var peers = mesh.Peers.Select(x => x.ToString());
        Console.WriteLine($"{args.PeerAddress} discovered. Current peers: {string.Join(", ", peers)}");
    }

    public void OnStart(IRaftCluster cluster, IDictionary<string, string> metadata)
    {
        cluster.LeaderChanged += LeaderChanged;
        cluster.PeerDiscovered += PeerDiscovered;
    }

    public void OnStop(IRaftCluster cluster)
    {
        cluster.LeaderChanged -= LeaderChanged;
        cluster.PeerDiscovered -= PeerDiscovered;
    }
}