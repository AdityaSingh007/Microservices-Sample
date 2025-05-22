FROM node:alpine AS builder
WORKDIR /app
COPY . .

#stage 2
FROM nginx:latest
EXPOSE 80
EXPOSE 443
COPY ./common/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=builder app/dist/common /usr/nginx/html
