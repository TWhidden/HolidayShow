import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000', // ASP.NET Core backend URL
        changeOrigin: true,
        secure: false,
      },
    },
  },
  build: {
    outDir: '../wwwroot', // Place the build output in the ASP.NET Core `wwwroot` folder
  },
});
