import { useState } from 'react'

export default function Testy() {
    const [count, setCount] = useState(0);
    return (
        <button onClick={() => setCount((count) => count + 1)}>
            count is {count} - click to increase
        </button>);
}
