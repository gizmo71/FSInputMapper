using Controlzmo.Hubs;
using Controlzmo.Systems.Atc;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Device.Location;
using System.Linq;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FuelLogData
    {
        [SimVar("FUEL TOTAL QUANTITY WEIGHT", "kg", SIMCONNECT_DATATYPE.FLOAT64, 50.0f)]
        public Double kgOnBoard;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class FuelLogListener : DataListener<FuelLogData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        [Property]
        private OfpWaypoint? _waypoint = null;
        private String[] log = new string[] { "", "", "", "", "" };

        public override void Process(ExtendedSimConnect simConnect, FuelLogData data)
        {
            if (_waypoint != null) {
                log[0] = log[1];
                log[1] = log[2];
                log[2] = log[3];
                log[3] = log[4];
                log[4] = $"At {_waypoint.Id}: FOB {Tons(data.kgOnBoard)} (a), {Tons(_waypoint.planFOB)} (p) {Tons(_waypoint.minFOB)} (m); FU {Tons(_waypoint.fuelUsed)} (p)";
                hubContext.Clients.All.SetFromSim("fuelLog", String.Join('\n', log));
            }
        }

        private static String Tons(Double kg)
        {
            return $"{kg / 1000.0:F2}"; //:0.00
        }
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PositionData
    {
        [SimVar("PLANE LATITUDE", "degrees latitude", SIMCONNECT_DATATYPE.FLOAT32, 0.0f)]
        public float latitude;
        [SimVar("PLANE LONGITUDE", "degrees longitude", SIMCONNECT_DATATYPE.FLOAT32, 0.0f)]
        public float longitude;
    };

    internal class Progress
    {
        public double distance = 0.0;
        public Boolean isClosing = false;
        public override string ToString() => $"{distance}{(isClosing ? "--" : "++")}";
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class PositionListener : DataListener<PositionData>, IOnGroundHandler
    {
        private readonly OperationalFlightPlan ofp;
        private readonly FuelLogListener logListener;
        private Dictionary<OfpWaypoint, Progress>? waypointProgress;

        public void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            logListener.Waypoint = null;
            if (isOnGround) waypointProgress = null;
            simConnect.RequestDataOnSimObject(
                this,
                isOnGround ? SIMCONNECT_CLIENT_DATA_PERIOD.NEVER : SIMCONNECT_CLIENT_DATA_PERIOD.SECOND);
        }

        public override void Process(ExtendedSimConnect simConnect, PositionData data)
        {
            if (waypointProgress == null)
                if (ofp.IsValid)
                    waypointProgress = ofp.GetFixes().ToDictionary(w => w, w => new Progress());
                else
                    return;
            var current = new GeoCoordinate(data.latitude, data.longitude);
Console.Error.WriteLine($"**---** current {current}");
            var firstPassed = (OfpWaypoint?)null;
            foreach (var entry in waypointProgress)
            {
                var distance = current.GetDistanceTo(entry.Key.position) / 1852;
                var isClosing = distance < entry.Value.distance;
Console.Error.WriteLine($"   **---** {entry.Key} distance {distance} closing? {isClosing}");
                if (firstPassed == null && !isClosing && entry.Value.isClosing && distance < 2.0)
                    firstPassed = entry.Key;
                entry.Value.isClosing = isClosing;
                entry.Value.distance = distance;
            }
            if (firstPassed != null) {
                logListener.Waypoint = firstPassed;
                simConnect.RequestDataOnSimObject(logListener, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
                var keys = new List<OfpWaypoint>(waypointProgress.Keys);
                foreach (var key in keys)
                {
                    waypointProgress.Remove(key);
                    if (key == firstPassed) break;
                }
            }
        }
    }
}
