import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    watch: {
      usePolling: true,
    },
    host: true, // allow access from Docker container
    strictPort: true,
    port: 5173, // match your Docker Compose port
    proxy: {
      '/api': {
        target: 'http://web:80', // 'web' is your backend service name in Docker Compose
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
