using Controlzmo;
using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using System;

namespace SimConnectzmo
{
    [Component, RequiredArgsConstructor]
    public partial class Wibbleator : IButtonCallback<T16000mHotas>, IEvent, IEventNotification
    {
        private readonly ILogger<Wibbleator> log;
        public int GetButton() => T16000mHotas.BUTTON_FRONT_LEFT_RED;

        public virtual void OnPress(ExtendedSimConnect sc) {
            //TODO: can we use this to find new events, if there are any? And perhaps avoid vJoy?
            sc.EnumerateControllers();
            log.LogCritical($"Asked for wibbleations 1 => {sc.GetLastSentPacketID()}");
        }

        public void OnRecvControllersList(ExtendedSimConnect sc, SIMCONNECT_RECV_CONTROLLERS_LIST data)
        {
try {
            log.LogCritical($"wibbleate 1 @ {data.dwEntryNumber}! {data.rgData.Length} v {data.dwArraySize}");
            for (var i = 0 ; i < data.dwArraySize; ++i)
            {
                var item = data.rgData[i] as SIMCONNECT_CONTROLLER_ITEM;
                // Name  is whatever random text you've altered MSFS to carry for that device.
                // DevID is just a sequential number starting from 0 and probably reflecting the order they show up in in the control options screen.
                // ProductID is the same as the USB one. No VendorID though, making it not far off useless!
                // CompositeID always seems to be 0.
                log.LogCritical($"wibbleate 1 [{i}] = {item.DeviceName}@{item.DeviceId} = {item.ProductId} and {item.CompositeID}");
                if ("TWCS Throttle" == item.DeviceName)
                {
                    EVENT @event = sc.eventToEnum![this];
                    var inputDefinition = $"joystick:{item.DeviceId}:button:{T16000mHotas.BUTTON_FRONT_LEFT_RED}";
                    sc.RemoveInputEvent(GROUP.JUST_MASKABLE, inputDefinition);
//TODO: how do we avoid having down AND up events? Can i use 0 in one of them?
                    sc.MapInputEventToClientEvent_EX1(GROUP.JUST_MASKABLE, inputDefinition, @event, 0, @event, 0, true);
                    log.LogCritical($"Mapped event => {sc.GetLastSentPacketID()}");
                }
            }
} catch (Exception e) { log.LogCritical(e, "Bugger"); }
        }

        public IEvent GetEvent() => this;

        public void OnRecieve(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            log.LogCritical("Got the test event!");
        }

        public string SimEvent() => "gizmo.test1";
    }
}
