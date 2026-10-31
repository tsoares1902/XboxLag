using XboxLag.Domain.Games;
using XboxLag.Domain.Measurements;
using XboxLag.Domain.Relays;
using XboxLag.Domain.Routing;
using XboxLag.Domain.Sessions;

using Xunit;

namespace XboxLag.Domain.Tests.Routing;

public sealed class DomainBehaviorTests
{
    private static RouteEndpoint Target => new("game.example", 3074);
    private static MeasurementContext Context => new("udp-echo", "vantage-a;payload=64;samples=20");

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 0, -1)]
    [InlineData(0, 0, 101)]
    [InlineData(double.NaN, 0, 0)]
    [InlineData(0, double.PositiveInfinity, 0)]
    [InlineData(0, 0, double.NegativeInfinity)]
    public void MetricsRejectInvalidValues(double latency, double jitter, double loss)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RouteMetrics(latency, jitter, loss));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 2, 100)]
    public void MetricsAcceptBoundaries(double latency, double jitter, double loss)
    {
        RouteMetrics metrics = new(latency, jitter, loss);
        Assert.Equal(latency, metrics.LatencyMs);
        Assert.Equal(jitter, metrics.JitterMs);
        Assert.Equal(loss, metrics.PacketLossPercent);
    }

    [Fact]
    public void RouteRequiresExactlyTheAppropriateRelayReference()
    {
        Assert.Throws<ArgumentException>(() => new Route(new("a"), Target, RouteKind.Relay, RouteStatus.Available));
        Assert.Throws<ArgumentException>(() => new Route(new("a"), Target, RouteKind.Direct, RouteStatus.Available, new("relay")));
        Route relay = new(new("a"), Target, RouteKind.Relay, RouteStatus.Available, new("relay"));
        Assert.Equal(new RelayId("relay"), relay.RelayId);
    }

    [Fact]
    public void RelayKeepsItsOwnEndpointAndValidatesMetadata()
    {
        RouteEndpoint endpoint = new("relay.example", 5000);
        RelayNode node = new(new("relay"), endpoint, " Relay ", RelayStatus.Available, " BR ");
        Assert.Equal(endpoint, node.Endpoint);
        Assert.Equal("Relay", node.DisplayName);
        Assert.Equal("BR", node.Region);
        Assert.Throws<ArgumentException>(() => new RelayNode(new("relay"), endpoint, " ", RelayStatus.Unknown));
        Assert.Throws<ArgumentException>(() => new RelayNode(new("relay"), endpoint, "Relay", RelayStatus.Unknown, " "));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void IdentitiesRejectBlankValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new GameId(value));
        Assert.Throws<ArgumentException>(() => new RelayId(value));
        Assert.Throws<ArgumentException>(() => new RouteId(value));
    }

    [Fact]
    public void GameProfileValidatesAndCopiesExecutableMetadata()
    {
        string[] names = ["game.exe"];
        GameProfile profile = new(new("game"), "Game", names);
        names[0] = "other.exe";
        Assert.Equal("game.exe", Assert.Single(profile.ExecutableNames));
        Assert.Equal(GameConnectionMode.Unknown, profile.ConnectionMode);
        Assert.Throws<ArgumentException>(() => new GameProfile(new("game"), "", ["game.exe"]));
        Assert.Throws<ArgumentException>(() => new GameProfile(new("game"), "Game", []));
        Assert.Throws<ArgumentException>(() => new GameProfile(new("game"), "Game", [" "]));
    }

    [Fact]
    public void ProcessAssociationDoesNotImplyGameplay()
    {
        TrafficFlow flow = new(TransportProtocol.Udp, new("localhost", 3000), Target, new("game"), new(42, "game.exe"));
        Assert.Equal(TrafficClassification.Unknown, flow.Classification);
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameProcess(0, "game.exe"));
    }

    [Fact]
    public void MeasurementRequiresMetricsOnlyOnSuccess()
    {
        Assert.Throws<ArgumentException>(() => new RouteMeasurement(new("r"), Target, DateTimeOffset.UtcNow, MeasurementStatus.Success, Context));
        Assert.Throws<ArgumentException>(() => new RouteMeasurement(new("r"), Target, DateTimeOffset.UtcNow, MeasurementStatus.Failed, Context, new(0, 0, 0)));
        RouteMeasurement failed = new(new("r"), Target, DateTimeOffset.UtcNow, MeasurementStatus.Failed, Context);
        Assert.Null(failed.Metrics);
    }

    [Theory]
    [InlineData(10, 30, "direct")]
    [InlineData(30, 10, "relay")]
    public void SelectionAllowsEitherRouteKindToWin(double directLatency, double relayLatency, string winner)
    {
        Route direct = Direct("direct");
        Route relay = new(new("relay"), Target, RouteKind.Relay, RouteStatus.Available, new("node"));
        Route? selected = new RouteSelectionPolicy(1, 1, 10).Select([direct, relay],
            [Observation(direct, directLatency), Observation(relay, relayLatency)], Target, Context);
        Assert.Equal(winner, selected?.Id.Value);
    }

    [Fact]
    public void SelectionUsesJitterAndLossAndDeterministicIdentityTieBreak()
    {
        Route a = Direct("a");
        Route b = Direct("b");
        RouteSelectionPolicy policy = new(1, 1, 10);
        Assert.Equal(a, policy.Select([b, a], [Observation(b, 10), Observation(a, 10)], Target, Context));
        RouteMeasurement poor = new(a.Id, Target, DateTimeOffset.UtcNow, MeasurementStatus.Success, Context, new(1, 20, 5));
        Assert.Equal(b, policy.Select([a, b], [poor, Observation(b, 10)], Target, Context));
    }

    [Fact]
    public void SelectionExcludesMissingUnavailableAndIncomparableObservations()
    {
        Route available = Direct("a");
        Route unavailable = new(new("b"), Target, RouteKind.Direct, RouteStatus.Unavailable);
        RouteMeasurement relayRtt = new(available.Id, new("relay.example", 5000), DateTimeOffset.UtcNow, MeasurementStatus.Success, Context, new(1, 0, 0));
        RouteMeasurement otherContext = new(available.Id, Target, DateTimeOffset.UtcNow, MeasurementStatus.Success, new("icmp", "other"), new(1, 0, 0));
        RouteSelectionPolicy policy = new(1, 1, 1);
        Assert.Null(policy.Select([available, unavailable], [relayRtt, otherContext, Observation(unavailable, 1)], Target, Context));
        Assert.Null(policy.Select([available], [], Target, Context));
    }

    [Fact]
    public void LatestFailurePreventsReuseOfOlderSuccess()
    {
        Route route = Direct("a");
        RouteMeasurement old = Observation(route, 1);
        RouteMeasurement failure = new(route.Id, Target, old.Timestamp.AddSeconds(1), MeasurementStatus.Failed, Context);
        Assert.Null(new RouteSelectionPolicy(1, 1, 1).Select([route], [old, failure], Target, Context));
    }

    [Fact]
    public void AmbiguousMeasurementsAndDuplicateRoutesAreRejected()
    {
        Route route = Direct("a");
        RouteMeasurement observation = Observation(route, 1);
        RouteSelectionPolicy policy = new(1, 1, 1);
        Assert.Null(policy.Select([route], [observation, observation], Target, Context));
        Assert.Throws<ArgumentException>(() => policy.Select([route, route], [observation], Target, Context));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 1, 1)]
    [InlineData(double.NaN, 1, 1)]
    [InlineData(1, double.PositiveInfinity, 1)]
    public void SelectionRejectsInvalidWeights(double latency, double jitter, double loss)
    {
        Assert.Throws<ArgumentException>(() => new RouteSelectionPolicy(latency, jitter, loss));
    }

    [Fact]
    public void SessionEnforcesLifecycleAndDestination()
    {
        RoutingSession session = new(Guid.NewGuid(), new("game"), Target, Direct("a"), DateTimeOffset.UtcNow);
        Assert.Equal(RoutingSessionStatus.Created, session.Status);
        Assert.Throws<InvalidOperationException>(() => session.TransitionTo(RoutingSessionStatus.Active));
        session.TransitionTo(RoutingSessionStatus.Starting);
        session.TransitionTo(RoutingSessionStatus.Active);
        session.TransitionTo(RoutingSessionStatus.Stopping);
        session.TransitionTo(RoutingSessionStatus.Stopped);
        Assert.Throws<InvalidOperationException>(() => session.TransitionTo(RoutingSessionStatus.Starting));
        Assert.Throws<ArgumentException>(() => new RoutingSession(Guid.NewGuid(), new("game"), new("other.example", 3074), Direct("a"), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void FailedSessionCanRetryCleanup()
    {
        RoutingSession session = new(Guid.NewGuid(), new("game"), Target, Direct("a"), DateTimeOffset.UtcNow);
        session.TransitionTo(RoutingSessionStatus.Starting);
        session.TransitionTo(RoutingSessionStatus.Failed);
        session.TransitionTo(RoutingSessionStatus.Stopping);
        session.TransitionTo(RoutingSessionStatus.Failed);
        session.TransitionTo(RoutingSessionStatus.Stopping);
        session.TransitionTo(RoutingSessionStatus.Stopped);
        Assert.Equal(RoutingSessionStatus.Stopped, session.Status);
    }

    private static Route Direct(string id) => new(new(id), Target, RouteKind.Direct, RouteStatus.Available);
    private static RouteMeasurement Observation(Route route, double latency) =>
        new(route.Id, Target, DateTimeOffset.UtcNow, MeasurementStatus.Success, Context, new(latency, 0, 0));
}
