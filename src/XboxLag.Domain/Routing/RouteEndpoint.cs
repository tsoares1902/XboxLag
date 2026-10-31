using System.Net;

namespace XboxLag.Domain.Routing;

/// <summary>
/// Represents a network endpoint identified by a host and port.
/// </summary>
public sealed record RouteEndpoint
{
    /// <summary>
    /// Gets the endpoint host.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Gets the endpoint port.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Initializes a network endpoint.
    /// </summary>
    /// <param name="host">The IP address or hostname.</param>
    /// <param name="port">The network port.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the host is empty or invalid.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the port is outside the valid range.
    /// </exception>
    public RouteEndpoint(string host, int port)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new ArgumentException(
                "The endpoint host cannot be empty.",
                nameof(host));
        }

        string normalizedHost = host.Trim();

        if (IPAddress.TryParse(normalizedHost, out IPAddress? address))
        {
            normalizedHost = address.ToString();
        }
        else
        {
            if (Uri.CheckHostName(normalizedHost) != UriHostNameType.Dns)
            {
                throw new ArgumentException(
                    "The endpoint host must be a valid IP address or hostname.",
                    nameof(host));
            }

            normalizedHost = normalizedHost.ToLowerInvariant();
        }

        if (port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(
                nameof(port),
                port,
                "The endpoint port must be between 1 and 65535.");
        }

        Host = normalizedHost;
        Port = port;
    }
}
