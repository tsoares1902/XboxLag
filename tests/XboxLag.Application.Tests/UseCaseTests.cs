using XboxLag.Application.Games.Ports;
using XboxLag.Application.Games.UseCases;
using XboxLag.Application.Measurements.Ports;
using XboxLag.Application.Measurements.UseCases;
using XboxLag.Application.Routing.Ports;
using XboxLag.Application.Routing.UseCases;
using XboxLag.Application.Sessions.UseCases;
using XboxLag.Domain.Games;
using XboxLag.Domain.Measurements;
using XboxLag.Domain.Routing;
using XboxLag.Domain.Sessions;

using Xunit;

namespace XboxLag.Application.Tests;

public sealed class UseCaseTests
{
    private static RouteEndpoint Target => new("game.example", 3074);
    private static MeasurementContext Context => new("echo", "same-conditions");
    private static Route Candidate => new(new("direct"), Target, RouteKind.Direct, RouteStatus.Available);
    private static GameProfile Game => new(new("game"), "Game", ["game.exe"]);
    private static RoutingSession Session => new(Guid.NewGuid(), Game.Id, Target, Candidate, DateTimeOffset.UtcNow);

    [Fact]
    public async Task DiscoveryHandlesGameNotRunning()
    {
        DiscoveryFake fake = new() { Processes = [] };
        Assert.Empty(await new DiscoverGameConnections(fake, fake).ExecuteAsync(Game));
        Assert.False(fake.MonitorCalled);
    }

    [Fact]
    public async Task DiscoveryPreservesUnknownClassificationAndFiltersUnrelatedProcesses()
    {
        DiscoveryFake fake = new();
        fake.Flows = [
            new(TransportProtocol.Udp, Target, Target, Game.Id, fake.Processes[0]),
            new(TransportProtocol.Tcp, Target, Target, Game.Id, new(999, "other.exe")),
            new(TransportProtocol.Udp, Target, Target, new("other"), fake.Processes[0])];
        TrafficFlow flow = Assert.Single(await new DiscoverGameConnections(fake, fake).ExecuteAsync(Game));
        Assert.Equal(TrafficClassification.Unknown, flow.Classification);
    }

    [Fact]
    public async Task DiscoveryPropagatesCancellationToken()
    {
        using CancellationTokenSource source = new();
        DiscoveryFake fake = new();
        await new DiscoverGameConnections(fake, fake).ExecuteAsync(Game, source.Token);
        Assert.Equal(source.Token, fake.SeenToken);
        source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new DiscoverGameConnections(fake, fake).ExecuteAsync(Game, source.Token));
    }

    [Fact]
    public async Task MeasurementPreservesSuccessAndFailure()
    {
        ProbeFake probe = new();
        Route second = new(new("failed"), Target, RouteKind.Direct, RouteStatus.Available);
        IReadOnlyList<RouteMeasurement> results = await new MeasureRoutes(probe).ExecuteAsync([Candidate, second], Target, Context);
        Assert.NotNull(results[0].Metrics);
        Assert.Equal(MeasurementStatus.Failed, results[1].Status);
        Assert.Null(results[1].Metrics);
    }

    [Fact]
    public async Task MeasurementRejectsMismatchedProbeResponse()
    {
        ProbeFake probe = new() { WrongTarget = true };
        await Assert.ThrowsAsync<InvalidOperationException>(() => new MeasureRoutes(probe).ExecuteAsync([Candidate], Target, Context));
    }

    [Fact]
    public async Task MeasurementPropagatesCancellation()
    {
        using CancellationTokenSource source = new();
        ProbeFake probe = new() { Cancel = source };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new MeasureRoutes(probe).ExecuteAsync([Candidate], Target, Context, source.Token));
        Assert.Equal(source.Token, probe.SeenToken);
    }

    [Fact]
    public void SelectionReturnsExplicitNoSelectionWithoutMeasurements()
    {
        SelectBestRoute useCase = new(new(1, 1, 1));
        Assert.Null(useCase.Execute([Candidate], [], Target, Context));
        RouteMeasurement observation = new(Candidate.Id, Target, DateTimeOffset.UtcNow, MeasurementStatus.Success, Context, new(10, 1, 0));
        Assert.Equal(Candidate, useCase.Execute([Candidate], [observation], Target, Context));
    }

    [Theory]
    [InlineData(TransportOutcome.Succeeded, RoutingSessionStatus.Active)]
    [InlineData(TransportOutcome.Failed, RoutingSessionStatus.Failed)]
    public async Task StartReflectsConfirmedTransportOutcome(TransportOutcome outcome, RoutingSessionStatus expected)
    {
        TransportFake fake = new() { Outcome = outcome };
        RoutingSession session = Session;
        Assert.Equal(outcome, await new StartRoutingSession(fake).ExecuteAsync(session));
        Assert.Equal(expected, session.Status);
    }

    [Theory]
    [InlineData(TransportOutcome.Succeeded, RoutingSessionStatus.Stopped)]
    [InlineData(TransportOutcome.Failed, RoutingSessionStatus.Failed)]
    public async Task StopReflectsConfirmedTransportOutcome(TransportOutcome outcome, RoutingSessionStatus expected)
    {
        RoutingSession session = Session;
        TransportFake fake = new();
        await new StartRoutingSession(fake).ExecuteAsync(session);
        fake.Outcome = outcome;
        Assert.Equal(outcome, await new StopRoutingSession(fake).ExecuteAsync(session));
        Assert.Equal(expected, session.Status);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TransportCancellationLeavesFailedSessionForCleanup(bool starting)
    {
        RoutingSession session = Session;
        TransportFake fake = new();
        if (!starting) { await new StartRoutingSession(fake).ExecuteAsync(session); }
        using CancellationTokenSource source = new();
        fake.Cancel = source;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => starting
            ? new StartRoutingSession(fake).ExecuteAsync(session, source.Token)
            : new StopRoutingSession(fake).ExecuteAsync(session, source.Token));
        Assert.Equal(source.Token, fake.SeenToken);
        Assert.Equal(RoutingSessionStatus.Failed, session.Status);
        fake.Cancel = null;
        await new StopRoutingSession(fake).ExecuteAsync(session);
        Assert.Equal(RoutingSessionStatus.Stopped, session.Status);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TransportExceptionsLeaveFailedSession(bool starting)
    {
        RoutingSession session = Session;
        TransportFake fake = new();
        if (!starting) { await new StartRoutingSession(fake).ExecuteAsync(session); }
        fake.Throw = true;
        await Assert.ThrowsAsync<IOException>(() => starting
            ? new StartRoutingSession(fake).ExecuteAsync(session)
            : new StopRoutingSession(fake).ExecuteAsync(session));
        Assert.Equal(RoutingSessionStatus.Failed, session.Status);
    }

    [Fact]
    public async Task PreCancelledStartDoesNotTouchTransportOrSession()
    {
        using CancellationTokenSource source = new();
        source.Cancel();
        RoutingSession session = Session;
        TransportFake fake = new();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new StartRoutingSession(fake).ExecuteAsync(session, source.Token));
        Assert.Equal(RoutingSessionStatus.Created, session.Status);
        Assert.Equal(0, fake.Calls);
    }

    private sealed class DiscoveryFake : IGameProcessDetector, IGameConnectionMonitor
    {
        public IReadOnlyList<GameProcess> Processes { get; set; } = [new(42, "game.exe")];
        public IReadOnlyList<TrafficFlow> Flows { get; set; } = [];
        public bool MonitorCalled { get; private set; }
        public CancellationToken SeenToken { get; private set; }
        public Task<IReadOnlyList<GameProcess>> DetectAsync(GameProfile game, CancellationToken cancellationToken)
        {
            SeenToken = cancellationToken;
            return Task.FromResult(Processes);
        }
        public Task<IReadOnlyList<TrafficFlow>> ObserveAsync(GameProfile game, IReadOnlyList<GameProcess> processes, CancellationToken cancellationToken)
        {
            MonitorCalled = true;
            SeenToken = cancellationToken;
            return Task.FromResult(Flows);
        }
    }

    private sealed class ProbeFake : IRouteProbe
    {
        public bool WrongTarget { get; init; }
        public CancellationTokenSource? Cancel { get; init; }
        public CancellationToken SeenToken { get; private set; }
        public Task<RouteMeasurement> MeasureAsync(Route route, RouteEndpoint target, MeasurementContext context, CancellationToken cancellationToken)
        {
            SeenToken = cancellationToken;
            Cancel?.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            MeasurementStatus status = route.Id.Value == "failed" ? MeasurementStatus.Failed : MeasurementStatus.Success;
            return Task.FromResult(new RouteMeasurement(route.Id, WrongTarget ? new("other.example", 3074) : target,
                DateTimeOffset.UtcNow, status, context, status == MeasurementStatus.Success ? new(10, 1, 0) : null));
        }
    }

    private sealed class TransportFake : IRoutingTransport
    {
        public TransportOutcome Outcome { get; set; } = TransportOutcome.Succeeded;
        public bool Throw { get; set; }
        public CancellationTokenSource? Cancel { get; set; }
        public CancellationToken SeenToken { get; private set; }
        public int Calls { get; private set; }
        public Task<TransportOutcome> StartAsync(RoutingSession session, CancellationToken cancellationToken) => Complete(cancellationToken);
        public Task<TransportOutcome> StopAsync(RoutingSession session, CancellationToken cancellationToken) => Complete(cancellationToken);
        private Task<TransportOutcome> Complete(CancellationToken cancellationToken)
        {
            Calls++;
            SeenToken = cancellationToken;
            Cancel?.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            if (Throw) { throw new IOException("Transport failed."); }
            return Task.FromResult(Outcome);
        }
    }
}
