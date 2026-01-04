import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { useEffect, useState } from 'react'

// https://stackoverflow.com/a/74603295
function useLiveUpdates(connectionRef: HubConnection | undefined) {
    useEffect(() => {
        if (connectionRef) {
            try {
                connectionRef
                    .start()
                    .then(() => {
console.log('started');
connectionRef.send('FromBrowser', "hello", 666); // If we don't actually trigger it it doesn't seem to get created...
                    })
                    .catch((err) => {
                        console.error('cannot connect', err);
                    });
            } catch (error) {
                console.error('cannot setup', error);
            }
        }

        return () => {
            connectionRef?.stop();
console.log('stopped');
        };
    }, [connectionRef]);
};

export default function Testy() {
    const [connectionRef, setConnection] = useState<HubConnection>();

    useEffect(() => {
        const con = new HubConnectionBuilder()
            .withUrl('/hub/testy')
            .withAutomaticReconnect()
            .build();
        con.on('ToBrowser', function (a, b) {
console.log('got message', a, b);
            con.send('FromBrowser', b, a);
        });
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setConnection(con);
    }, []);

    useLiveUpdates(connectionRef);

    const [count, setCount] = useState(0);
    return (
        <button onClick={() => setCount((count) => count + 1)}>
            count is {count} - click to increase
        </button>);
}
