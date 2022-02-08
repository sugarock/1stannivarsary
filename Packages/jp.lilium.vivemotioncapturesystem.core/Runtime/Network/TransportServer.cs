using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;

namespace VMCCore.Network
{

    public interface INetworkReactor
    {
        void OnAccepted (NetworkConnection connection);

        void OnReceived (NetworkConnection connection, DataStreamReader stream);

        void OnDisconnected (NetworkConnection connection);
    }

    public class TransportServer : System.IDisposable
    {
        public NetworkDriver driver { get; private set; }
        public NativeList<NetworkConnection> connections = new NativeList<NetworkConnection> ();

        public INetworkReactor listerner;

        public bool isListening => connections.IsCreated;

 
        public bool Listen (ushort port, int maxConnections = 16)
        {
            if (driver.IsCreated) {
                Dispose ();
            }

            driver = NetworkDriver.Create ();
            var endpoint = NetworkEndPoint.AnyIpv4;
            endpoint.Port = port;

            if (driver.Bind (endpoint) != 0) {
                Debug.LogError ($"[VMCCore] Failed to bind to port {port}");
                return false;
            }
            else {
                driver.Listen ();
            }

            connections = new NativeList<NetworkConnection> (maxConnections, Allocator.Persistent);
            return true;
        }

        public void Dispose ()
        {
            driver.Dispose ();
            if (connections.IsCreated) 
                connections.Dispose ();
        }

        public void Close()
        {
            for (int i = 0; i < connections.Length; i++) {
                driver.Disconnect (connections[i]);
             }
             connections.Dispose();
        }

        public void Update ()
        {
            if (!isListening) return;

            driver.ScheduleUpdate ().Complete ();

            // CleanUpConnections
            for (int i = 0; i < connections.Length; i++) {
                if (!connections[i].IsCreated) {
                    connections.RemoveAtSwapBack (i);
                    --i;
                }
            }
            // AcceptNewConnections
            NetworkConnection c;
            while ((c = driver.Accept ()) != default (NetworkConnection)) {
                connections.Add (c);
                listerner?.OnAccepted (c);
            }

            DataStreamReader stream;
            for (int i = 0; i < connections.Length; i++) {
                Assert.IsTrue (connections[i].IsCreated);

                NetworkEvent.Type cmd;
                while ((cmd = driver.PopEventForConnection (connections[i], out stream)) != NetworkEvent.Type.Empty) {
                    if (cmd == NetworkEvent.Type.Data) {
                        listerner?.OnReceived (connections[i], stream);
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect) {
                        listerner?.OnDisconnected (connections[i]);
                        connections[i] = default (NetworkConnection);
                    }
                }
            }
        }

    }

}