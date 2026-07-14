import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Fail loudly if 5173 is taken instead of silently drifting to another
    // port - the backend's CORS policy only allows localhost:5173, so a
    // silent port change would otherwise show up later as a confusing
    // "network error" on login instead of an obvious "port in use" message.
    port: 5173,
    strictPort: true,
  },
})
