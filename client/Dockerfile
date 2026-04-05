# Stage 1: Build the Angular app
FROM node:18-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./

# Tell npm to ignore peer dependency conflicts
RUN npm config set legacy-peer-deps true

# Install dependencies (now npm ci will respect the config)
RUN npm ci

# Copy the rest of the source and build
COPY . .
RUN npm run build --prod

# Stage 2: Serve with Nginx
FROM nginx:alpine
COPY --from=build /app/dist/social-network-client/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80