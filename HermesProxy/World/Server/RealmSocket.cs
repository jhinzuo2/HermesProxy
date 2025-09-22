using System.Net.Sockets;

namespace HermesProxy.World.Server;

public sealed class RealmSocket : WorldSocket
{
    public RealmSocket(Socket socket) : base(socket)
    {
    }
}
