# Multi-stage build for the Angular SPA: build with Node, serve static files with nginx.
# Build context is the web/ directory (see docker-compose build.context).

FROM node:22-alpine AS build
WORKDIR /app

# Install deps first for layer-cached builds.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
# Production build swaps in environment.prod.ts (relative "/api" base).
RUN npm run build -- --configuration production

FROM nginx:1.27-alpine AS runtime
# SPA fallback routing + /api proxy to the API container.
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/contactmanager-web/browser /usr/share/nginx/html

EXPOSE 80
