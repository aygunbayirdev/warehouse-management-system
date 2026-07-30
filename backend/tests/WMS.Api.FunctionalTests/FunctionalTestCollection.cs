namespace WMS.Api.FunctionalTests;

/// <summary>
/// A single shared <see cref="CustomWebApplicationFactory"/> (one host, one Postgres container) for
/// every functional test class. xUnit never runs classes within the same collection in parallel
/// with each other, which also works around WebApplicationFactory's host-interception mechanism
/// getting confused when multiple factories are built concurrently in the same process.
/// </summary>
[CollectionDefinition("Functional")]
public sealed class FunctionalTestCollection : ICollectionFixture<CustomWebApplicationFactory>;
