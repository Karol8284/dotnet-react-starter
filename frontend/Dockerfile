# Stage 1: Builder - Budowanie aplikacji React
FROM node:20-alpine AS builder

# Ustawiamy folder roboczy
WORKDIR /app

# Kopiujemy package.json i package-lock.json
COPY package*.json ./

# Instalujemy wszystkie zależności (npm packages)
RUN npm ci

# Kopiujemy kod źródłowy React
COPY . .

# Budujemy aplikację React (tworzy folder build/)
RUN npm run build


# Stage 2: Runtime - Serwowanie aplikacji za pomocą Nginx
FROM nginx:alpine

# Kopiujemy konfigurację nginx (będzie w docker/nginx.conf)
COPY docker/nginx.conf /etc/nginx/nginx.conf

# Usuwamy domyślny HTML nginx
RUN rm -rf /usr/share/nginx/html/*

# Kopiujemy zbudowaną aplikację React z etapu builder
COPY --from=builder /app/build /usr/share/nginx/html

# Otwieramy port 3000 (na tym porcie nasłuchuje aplikacja)
EXPOSE 3000

# Uruchamiamy nginx
CMD ["nginx", "-g", "daemon off;"]
