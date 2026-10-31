using XboxLag.Domain.Routing;

using Xunit;

namespace XboxLag.Domain.Tests.Routing;

public sealed class RouteEndpointTests
{
    [Theory]
    [InlineData("127.0.0.1", 3074)]
    [InlineData("192.168.1.1", 8080)]
    [InlineData("example.com", 443)]
    [InlineData("::1", 3074)]
    [InlineData("2001:db8::1", 443)]
    public void ConstructorShouldAcceptValidEndpoints(
        string host,
        int port)
    {
        RouteEndpoint endpoint = new(host, port);

        Assert.Equal(port, endpoint.Port);
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Host));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ConstructorShouldRejectEmptyHosts(string? host)
    {
        Assert.Throws<ArgumentException>(
            () => new RouteEndpoint(host!, 3074));
    }

    [Theory]
    [InlineData("invalid host")]
    [InlineData("https://example.com")]
    [InlineData("example.com/path")]
    public void ConstructorShouldRejectInvalidHosts(string host)
    {
        Assert.Throws<ArgumentException>(
            () => new RouteEndpoint(host, 3074));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(65536)]
    [InlineData(100000)]
    public void ConstructorShouldRejectInvalidPorts(int port)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RouteEndpoint("127.0.0.1", port));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(65535)]
    public void ConstructorShouldAcceptPortBoundaries(int port)
    {
        RouteEndpoint endpoint = new("127.0.0.1", port);

        Assert.Equal(port, endpoint.Port);
    }

    [Fact]
    public void ConstructorShouldNormalizeHostname()
    {
        RouteEndpoint endpoint = new("  EXAMPLE.COM  ", 443);

        Assert.Equal("example.com", endpoint.Host);
    }

    [Fact]
    public void ConstructorShouldNormalizeIpv6Address()
    {
        RouteEndpoint endpoint = new("0:0:0:0:0:0:0:1", 3074);

        Assert.Equal("::1", endpoint.Host);
    }

    [Fact]
    public void EqualityShouldReturnTrueForEquivalentEndpoints()
    {
        RouteEndpoint first = new("EXAMPLE.COM", 443);
        RouteEndpoint second = new("example.com", 443);

        Assert.Equal(first, second);
    }

    [Fact]
    public void EqualityShouldReturnFalseForDifferentPorts()
    {
        RouteEndpoint first = new("example.com", 443);
        RouteEndpoint second = new("example.com", 8080);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void EqualityShouldReturnFalseForDifferentHosts()
    {
        RouteEndpoint first = new("example.com", 443);
        RouteEndpoint second = new("another.example.com", 443);

        Assert.NotEqual(first, second);
    }
}
