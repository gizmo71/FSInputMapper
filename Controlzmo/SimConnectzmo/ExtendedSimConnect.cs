using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectzmo
{
    internal enum REQUEST { }
    internal enum STRUCT { }
    internal enum EVENT { }

    public class ExtendedSimConnect : SimConnect
    {
        private static readonly IntPtr hWnd = IntPtr.Zero;

        internal Dictionary<Type, STRUCT>? typeToStruct;
        //internal Dictionary<IDataListener, REQUEST>? typeToRequest;
        //internal Dictionary<IEventNotification, EVENT>? notificationsToEvent;
        //internal Dictionary<IEvent, EVENT>? eventToEnum;

        internal ExtendedSimConnect(string szName, uint UserEventWin32, WaitHandle waitHandle)
            : base(szName, hWnd, UserEventWin32, waitHandle, 0) // 6 for over IP - can we make it timeout easier?
        {
        }
    }
}
