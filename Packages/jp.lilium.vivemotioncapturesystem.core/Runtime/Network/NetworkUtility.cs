using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Networking.Transport;

namespace VMCCore
{
    public static unsafe class NetworkUtility
    {
        public static void WriteStruct<T> (this ref DataStreamWriter @this, ref T value) where T : unmanaged
        {
            fixed (T* ptr = &value) {
                @this.WriteBytes ((byte*)ptr, UnsafeUtility.SizeOf<T> ());
            }
        }

        public static void WriteNativeArray<T> (this ref DataStreamWriter @this, NativeArray<T> value) where T : struct
        {
            @this.WriteInt (value.Length);
            @this.WriteBytes ((byte*)value.GetUnsafePtr (), value.Length * UnsafeUtility.SizeOf<T> ());
        }

        public static void ReadStruct<T> (this ref DataStreamReader @this, ref T value) where T : struct
        {
            @this.ReadBytes ( (byte*)UnsafeUtility.AddressOf (ref value), UnsafeUtility.SizeOf<T> ());
        }

        public static void ReadNativeArray<T> (this ref DataStreamReader @this, NativeArray<T> value) where T : struct
        {
            var length = @this.ReadInt ();
            if (value.Length < length) {
                Debug.LogError ("Cannot read because of insufficient capacity.");
                return;
            }
            @this.ReadBytes ((byte*)value.GetUnsafePtr (), value.Length * UnsafeUtility.SizeOf<T> ());
        }

        public static NativeArray<T> ReadAndCrateNativeArray<T> (this ref DataStreamReader @this, Allocator allocator) where T : struct
        {
            var length = @this.ReadInt ();
            var value = new NativeArray<T> (length, allocator);
            @this.ReadBytes ((byte*)value.GetUnsafePtr (), value.Length * UnsafeUtility.SizeOf<T> ());
            return value;
        }

      public static bool ParseIPAddress (string endpoint, int defaultPort, out IPAddress address, out int port)
        {
            string address_part;
            address = null;
            port = 0;

            if (endpoint.Contains (":")) {
                int.TryParse (endpoint.AfterLast (":"), out port);
                address_part = endpoint.BeforeFirst (":");
            }
            else
                address_part = endpoint;

            if (port == 0)
                port = defaultPort;

            try {
                address = IPAddress.Parse(address_part);
                return true;
            }
            catch (FormatException e) {
                return false;
            }
        }
    }

}
