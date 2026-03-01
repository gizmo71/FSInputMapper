import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 58938,
    },
    root: 'src/',
    build: {
        outDir: '../../wwwroot/js/react',
        emptyOutDir: true,
        rollupOptions: {
            input: 'src/main.tsx',
            output: {
                entryFileNames: 'bundle.js',
            },
        },
    },
})
