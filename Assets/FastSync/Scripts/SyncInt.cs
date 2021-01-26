﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Dustuu.VRChat.FastSync
{
    public class SyncInt : UdonSharpBehaviour
    {
        private SyncByte[] syncBytes;

        protected void Start() { syncBytes = GetComponentsInChildren<SyncByte>(); }

        // Call this method to request a new value for the SyncInt
        public void RequestInt(int request)
        {
            byte[] requestBytes = Int32ToBytes(request);
            if (syncBytes.Length == requestBytes.Length)
            {
                for (int i = 0; i < syncBytes.Length; i++) { syncBytes[i].RequestByte(requestBytes[i]); }
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Convert");
            }
            else { Debug.LogError("[FastSync] request bytes length mismatch!"); }
        }

        // Called from RequestInt via SendCustomNetworkEvent
        public void Convert() { foreach (SyncByte syncByte in syncBytes) { syncByte.Convert(); } }

        public int GetUdonSynced() { return GetSynced(false); }
        public int GetFastSynced() { return GetSynced(true); }
        private int GetSynced(bool fast)
        {
            if (syncBytes == null) { return 0; }

            byte[] bytes = new byte[syncBytes.Length];
            for (int i = 0; i < bytes.Length; i++) { bytes[i] = fast ? syncBytes[i].GetFastSynced() : syncBytes[i].GetUdonSynced(); }
            return BytesToInt32(bytes);
        }

        // Assumes Big-Endian order
        private int BytesToInt32(byte[] input) { return (input[0] << 24) | (input[1] << 16) | (input[2] << 8) | (input[3]); }

        // Assumes Big-Endian order
        private byte[] Int32ToBytes(int input)
        {
            byte[] output = new byte[4];
            output[0] = (byte)((input >> 24) % 256);
            output[1] = (byte)((input >> 16) % 256);
            output[2] = (byte)((input >> 8) % 256);
            output[3] = (byte)(input % 256);
            return output;
        }
    }
}