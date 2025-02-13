﻿using Controlzmo.GameControllers;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;
using Windows.Devices.Geolocation;

namespace Controlzmo.Systems.Spoilers
{
    /* A32NX rules:
       You cannot arm the spoilers unless the handle is RETRACTED.
       If the new position is anything but that, the arming state is false.
       Ground spoilers can be active during RTO even if not armed, in which case handle position becomes FULL.
       Similar but more complex rules during landing, and position can be PARTIAL without showing active.
       The "SPOILERS HANDLE POSITION" is the sim position, NOT the visible position, A32NX_SPOILERS_HANDLE_POSITION (0-1). */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SpoilerData
    {
        [SimVar("SPOILERS ARMED", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 armed;
        [SimVar("SPOILERS HANDLE POSITION", "percent", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 position;
        [SimVar("L:A32NX_SPOILERS_ARMED", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 a32nxArmed;
        [SimVar("L:A32NX_SPOILERS_GROUND_SPOILERS_ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 a32nxGroundSpoilersActive;
        [SimVar("L:A32NX_SPOILERS_HANDLE_POSITION", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.025f)]
        public float a32nxPosition;
        [SimVar("L:A32NX_IS_READY", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 a32nxReady;
        [SimVar("L:A_FC_SPEEDBRAKE", "Bool", SIMCONNECT_DATATYPE.FLOAT32, 0.5f)]
        public float fenix; // Fenix LVar: A_FC_SPEEDBRAKE, 0 = spoilers armed, 1 to 3 = speedbrake positions
        [SimVar("L:INI_SPOILERS_ARMED", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 iniArmed;
        [SimVar("L:INI_SPOILERS_HANDLE_POSITION", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.025f)]
        public float iniPosition;
/* Asobo B38M UI code (event HANDLING_SPEED_BRAKES) is:
   p0 100 * 0 max 100 min 0 100 (F:Clamp)
   (>O:XMLVAR_Speedbrakes_Position, percent) (O:XMLVAR_Speedbrakes_Position, percent) sp0
   l0 15 > (>K:SPOILERS_ARM_SET)
   l0 30 80 0 1 (F:MapRange) 16384 * (>K:SPOILERS_SET)
   l0 30 > if{ (E:SIMULATION TIME, second) (>L:1:XMLVAR_Speedbrakes_Activation_Time) } */
    };

    [Component] public class SpoilerArmOnEvent : IEvent { public string SimEvent() => "SPOILERS_ARM_ON"; }
    [Component] public class SpoilerArmOffEvent : IEvent { public string SimEvent() => "SPOILERS_ARM_OFF"; }
    [Component] public class SetSpoilerHandleEvent : IEvent { public string SimEvent() => "SPOILERS_SET"; }

    public abstract class AbstractSpoilerDataListener : DataListener<SpoilerData>, IButtonCallback<T16000mHotas>
    {
        protected readonly ILogger _logger;
        protected readonly SetSpoilerHandleEvent setEvent;
        protected readonly JetBridgeSender sender;
        protected readonly InputEvents inputEvents;
        protected readonly SpoilerArmOffEvent armOffEvent;
        protected readonly SpoilerArmOnEvent armOnEvent;

        protected AbstractSpoilerDataListener(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().FullName!);
            setEvent = sp.GetRequiredService<SetSpoilerHandleEvent>();
            sender = sp.GetRequiredService<JetBridgeSender>();
            inputEvents = sp.GetRequiredService<InputEvents>();
            armOffEvent = sp.GetRequiredService<SpoilerArmOffEvent>();
            armOnEvent = sp.GetRequiredService<SpoilerArmOnEvent>();
        }

        public abstract int GetButton();
        public void OnPress(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);

        public override void Process(ExtendedSimConnect simConnect, SpoilerData data)
        {
            _logger.LogError($"Wants spoiler; raw data pos {data.position} armed {data.armed} A32NX ready? {data.a32nxReady} A32NX armed {data.a32nxArmed} active {data.a32nxGroundSpoilersActive} handle {data.a32nxPosition} ini armed {data.iniArmed} ini pos {data.iniPosition}");
            if (simConnect.IsIniBuilds)
            {
                data.armed = data.iniArmed;
                data.position = (int)(100 * data.iniPosition);
            }
            else if (simConnect.IsAsoboB38M)
            {
                //data.armed = data.position > 0 ? 0 : data.armed; // Yes, this is daft.
                //data.position = (Int32) Math.Ceiling(Math.Max(0, (data.asoboB38M - 30) / 0.7));
            }
            else if (data.a32nxReady == 1)
            {
                data.armed = data.a32nxArmed == 1 || data.a32nxGroundSpoilersActive == 1 ? 1 : 0;
                data.position = (int)(100 * data.a32nxPosition);
            }
            _logger.LogDebug("... processed data pos {} armed {}; Fenix {}", data.position, data.armed, data.fenix);

            ProcessSpoilerDemand(simConnect, data);
        }

        protected void SetAsoboMaxPosition(ExtendedSimConnect simConnect, int? newPosition)
        {
            var demand = newPosition! * 0.005 + 0.3;
            _logger.LogDebug("Now demand Max position {} or as event value {}", newPosition, demand);
            inputEvents.Send(simConnect, "HANDLING_SPEED_BRAKES", demand);
        }

        protected abstract void ProcessSpoilerDemand(ExtendedSimConnect simConnect, SpoilerData data);
    }

    [Component]
    public class MoreSpoiler : AbstractSpoilerDataListener
    {
        public MoreSpoiler(IServiceProvider sp) : base(sp) { }

        public override int GetButton() => T16000mHotas.BUTTON_BOTTOM_HAT_AFT;

        protected override void ProcessSpoilerDemand(ExtendedSimConnect simConnect, SpoilerData data)
        {
            if (simConnect.IsFenix)
            {
                var value = data.fenix < 1 ? 1 : Math.Min(data.fenix + 0.5, 3);
                sender.Execute(simConnect, $"{value} (>L:A_FC_SPEEDBRAKE)");
                return;
            }

            int? newPosition = null;
            IEvent? toSend = null;
            if (simConnect.IsAsoboB38M && data.armed == 0)
                toSend = armOnEvent;
            else if (!simConnect.IsAsoboB38M && data.armed != 0)
                toSend = armOffEvent;
            else if (data.position < 100)
                newPosition = Math.Min(data.position + 25, 100);

            if (toSend != null)
            {
                _logger.LogDebug("Now send {}", toSend);
                simConnect.SendEvent(toSend);
            }

            if (newPosition != null)
            {
                if (simConnect.IsAsoboB38M)
                    SetAsoboMaxPosition(simConnect, newPosition);
                else
                {
                    uint eventData = (uint)newPosition * 164;
                    eventData = Math.Min(eventData, 16384u);
                    _logger.LogDebug("Now demand position {} or as U {}", newPosition, eventData);
                    simConnect.SendEvent(setEvent, eventData);
                }
            }
        }
    }

    [Component]
    public class LessSpoiler : AbstractSpoilerDataListener
    {
        public LessSpoiler(IServiceProvider sp) : base(sp) { }

        public override int GetButton() => T16000mHotas.BUTTON_BOTTOM_HAT_FORE;

        protected override void ProcessSpoilerDemand(ExtendedSimConnect simConnect, SpoilerData data)
        {
            if (simConnect.IsFenix)
            {
                var value = data.fenix > 1 ? Math.Max(data.fenix - 0.5, 1) : 0;
                sender.Execute(simConnect, $"{value} (>L:A_FC_SPEEDBRAKE)");
                return;
            }

            if (data.position > 0)
            {
                int newPosition = Math.Max(data.position - 25, 0);
                if (simConnect.IsAsoboB38M)
                    SetAsoboMaxPosition(simConnect, newPosition);
                else
                {
                    uint eventData = (uint)newPosition * 164u;
                    eventData = Math.Min(eventData, 16384u);
                    _logger.LogDebug($"Now demand position {newPosition} or as U {eventData}");
                    simConnect.SendEvent(setEvent, eventData);
                }
            }
            else if (simConnect.IsAsoboB38M &&  data.armed != 0)
            {
                simConnect.SendEvent(armOffEvent);
            }
            else if (!simConnect.IsAsoboB38M && data.armed == 0)
            {
                _logger.LogDebug($"Now send {armOnEvent}");
                simConnect.SendEvent(armOnEvent);
            }
        }
    }

    //TODO: index finger on right side of stick head: fore/aft/click=less->arm/disarm-more/stow spoibrakes, up/down flaps
}
