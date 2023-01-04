using System;

public class NSLEndPoint
{
    public enum Type
    {
        Unknown,
        TCP,
        UDP,
        WS
    }

    public Type ProtocolType { get; } = Type.Unknown;

    public string EndPoint { get; }

    public string Address { get; }

    public int Port { get; }

    private NSLEndPoint() { }

    public NSLEndPoint(string endPoint)
    {
        this.EndPoint = endPoint;

        var uri = new Uri(endPoint);

        ProtocolType = Enum.Parse<Type>(uri.Scheme, true);

        Address = uri.Host;

        Port = uri.Port;
    }

    public static NSLEndPoint Parse(string endPoint) => new NSLEndPoint(endPoint);
}