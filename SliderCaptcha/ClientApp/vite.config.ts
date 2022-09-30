import { fileURLToPath, URL } from "node:url";

import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";

const options =
  process.env.NODE_ENV === "production"
    ? {
        plugins: [vue()],
        resolve: {
          alias: {
            "@": fileURLToPath(new URL("./src", import.meta.url)),
          },
        },
        build: {
          outDir: "../wwwroot",
          emptyOutDir: true,
        },
      }
    : {
        plugins: [vue()],
        resolve: {
          alias: {
            "@": fileURLToPath(new URL("./src", import.meta.url)),
          },
        },
        server: {
          proxy: {
            "/api": {
              target: "https://localhost:7098/api/",
              secure: false,
              changeOrigin: true,
              rewrite: (path) => path.replace(/^\/api/, ""),
            },
          },
        },
      };

// https://vitejs.dev/config/
export default defineConfig(options);
