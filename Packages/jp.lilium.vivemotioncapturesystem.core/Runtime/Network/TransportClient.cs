using UnityEngine;

using Unity.Networking.Transport;
using System.Net;

namespace VMCCore.Network
{

    public class TransportClient : System.IDisposable
    {
        public NetworkDriver driver { get; private set; }
        public NetworkConnection connection { get; private set; }
        public INetworkReactor listerner;
        public bool isConnected { get; private set; } = false;
        public bool Connect (string address, ushort port)
        {
            if (driver.IsCreated) {
                Dispose ();
            }

            var ep = NetworkEndPoint.Parse (address, port);

            driver = NetworkDriver.Create ();

            connection = driver.Connect (ep);
            return true;
        }

        public void Disconnect()
        {
            driver.Disconnect (connection);
        }

        public void Dispose ()
        {
            driver.Dispose ();
        }

        public void Update ()
        {
            driver.ScheduleUpdate ().Complete ();

            if (!connection.IsCreated) {
                return;
            }

            DataStreamReader stream;
            NetworkEvent.Type cmd;

            while ((cmd = connection.PopEvent (driver, out stream)) != NetworkEvent.Type.Empty) {
                if (cmd == NetworkEvent.Type.Connect) {
                    isConnected = true;
                    listerner?.OnAccepted (connection);
                }
                else if (cmd == NetworkEvent.Type.Data) {
                    listerner?.OnReceived (connection, stream);
                }
                else if (cmd == NetworkEvent.Type.Disconnect) {
                    listerner?.OnDisconnected (connection);
                    connection = default (NetworkConnection);
                    isConnected = false;
                }
            }
        }
    }

}
