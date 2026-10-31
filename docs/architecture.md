# Stage 02 architecture

XboxLag is a modular monolith with Clean Architecture dependency direction and
hexagonal ports and adapters. Functional modules live within technical assemblies;
there is no project per module and no combined Core assembly.

## Assemblies and boundaries

- Domain owns entities, immutable values, invariants and deterministic policy. It
  has no production project references.
- Application references Domain and owns orchestration and focused ports.
- Infrastructure currently references Application and is reserved for low-level
  technical primitives and platform integration.
- Adapters currently references Application (Domain is transitively available).
  It will translate external systems into application contracts. No artificial
  pass-through implementations are included.
- Future executable hosts are composition roots that construct concrete services.

Dependencies point inward. Domain and Application must never depend on either
outer assembly. Architecture tests inspect the project graph for cycles and
forbidden references, and verify functional namespaces against source folders.
Infrastructure primitives should not duplicate adapters: a Windows networking
primitive belongs in Infrastructure; an implementation translating its output
into an application port belongs in Adapters when that translation is useful.

## Functional modules

- Games owns profiles, executable and process identification, and observed flows.
  Traffic types live here because their purpose is game connection discovery,
  while their endpoints reuse Routing values. This is an explicit intra-domain
  dependency, not an operating system dependency.
- Routing owns endpoints, path descriptions, metrics and selection policy.
- Relays owns intermediary node identities, endpoints and availability.
- Measurements owns traceable observations and compatibility contexts.
- Sessions owns lifecycle state for a game, destination and selected route.

Executable names identify candidates; process association does not prove gameplay.
Flow classification defaults to Unknown. Discovery filters observations by
detected process and rejects contradictory game associations. A future classifier
must use evidence before assigning Gameplay or Auxiliary.

## Measurements and selection

A route destination is the final game server or peer endpoint. A relay node has a
separate intermediary endpoint. RTT to a relay is not RTT to the final destination.
No game addresses are hardcoded.

Measurements always carry route identity, actual target, timestamp, status and
context. Success requires finite, validated metrics; failure and cancellation
forbid metrics. Unknown metrics are absent, never manufactured as zero.

The caller requests an explicit target and context. Context equality requires
the same method and conditions key, which must encode protocol, payload, vantage
and sampling settings. The provider is responsible for assigning these keys
accurately. Only available paths to that target with matching successful
observations are eligible.

For each path the latest matching observation is authoritative, including a
failure. Multiple observations at the same latest timestamp are ambiguous and
exclude that path. Candidate route identities must be unique.

Explicit finite nonnegative weights must include at least one positive weight.
Weights are divided by the maximum weight. Score is the weighted sum of latency,
jitter and percentage-point packet loss, divided by three for numeric headroom.
Lower scores win; exact ties use ordinal stable route identity. Scores express
policy preferences, not measured percentage improvements. Both direct and relay
paths can win. Null is an explicit no-selection outcome. Stage 02 does not enforce
a measurement expiry window or implement live switching.

## Ports and orchestration

Application defines IGameProcessDetector, IGameConnectionMonitor,
IRouteRepository, IRelayRepository, IRouteProbe and IRoutingTransport. They use
platform-neutral domain types and cancellation tokens. Expected probe failures
return failed observations; transport operations return Succeeded or Failed.
Unexpected technical exceptions and cancellation propagate.

DiscoverGameConnections detects configured processes and observes their flows;
absence produces an empty result. MeasureRoutes preserves expected success and
failure outcomes and rejects mismatched probe responses. SelectBestRoute delegates
to the domain without changing networking.

StartRoutingSession accepts a validated Created session with matching available
route and destination. It moves through Starting to Active only on confirmed
transport success. StopRoutingSession moves through Stopping to Stopped only on
confirmed shutdown. Failure, unexpected exception or cancellation during either
operation leaves Failed. Failed means transport state is uncertain, not inactive;
the retained session can retry shutdown. Pre-cancellation does not mutate state.

The normal lifecycle is Created -> Starting -> Active -> Stopping -> Stopped.
Starting and Stopping can fail. Failed -> Stopping permits cleanup retries.
Invalid transitions are rejected. Session operations require serialized ownership
by the caller; there is no concurrent session manager in this stage.

## Future integration and limitations

Future Windows integration will supply process discovery, connection monitoring
and traffic transport outside Domain/Application. No Windows handles are exposed
by these contracts. Future Linux relay hosts can implement relay transport and
protocols using the same platform-neutral definitions, with credentials managed
outside domain entities.

There is no real interception, capture, WFP, WinDivert, tunnel, UDP relay, probe,
SQLite persistence, desktop UI or real game discovery. No optimizer is operational
yet. The empty outer assemblies intentionally avoid fake production adapters.
