﻿using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.JetBridge
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct JetBridgeDownlinkData
    {
        [MarshalAs(UnmanagedType.I4)]
        [ClientVar(0.5f)]
        public Int32 id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        [ClientVar(0f)]
        public string data;
    };

    [Component]
    public class JetBridgeListener : DataListener<JetBridgeDownlinkData>, IClientData, IRequestDataOnOpen
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        private const string UplinkClientDataName = "theomessin.jetbridge.downlink";

        public string GetClientDataName() => UplinkClientDataName;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => (SIMCONNECT_PERIOD)SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET;

        public override void Process(ExtendedSimConnect simConnect, JetBridgeDownlinkData data)
        {
System.Console.Error.WriteLine($"JetBridge reply ID {data.id} = '{data.data}'");
        }
    }

    //TODO: work out how to share the definition between the two halves - this is why you can have different client IDs but the same definition ID
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct JetBridgeNoUplinkData
    {
        [MarshalAs(UnmanagedType.I4)]
        [ClientVar(0.5f)]
        public Int32 id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        [ClientVar(0f)]
        public string data;
    };

    [Component]
    public class JetBridgeSender : DataSender<JetBridgeNoUplinkData>, IClientData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        private const string DownlinkClientDataName = "theomessin.jetbridge.uplink";

        private readonly Random random = new();

        public string GetClientDataName() => DownlinkClientDataName;

        public int Execute(ExtendedSimConnect simConnect, string code)
        {
            var data = new JetBridgeNoUplinkData { id = random.Next(), data = $"x{code}" };
            simConnect.SendDataOnSimObject(data);
            return data.id;
        }
    }

    [Component]
    public class JetBridge : ISettable<string?>
    {
        private readonly JetBridgeSender sender;

        public JetBridge(JetBridgeSender sender)
        {
            this.sender = sender;
        }

        public string GetId() => "jetbridge";

        public void SetInSim(ExtendedSimConnect simConnect, string? codeToExecute)
        {
            sender.Execute(simConnect, codeToExecute!);
        }
    }
}