import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import Testy from './Testy.tsx'

const root = document.getElementById('root')!;
const mode = root.dataset.mode!;
createRoot(root).render(
    <StrictMode>
        {mode == 'testy' && <Testy />}
    </StrictMode>,
)
