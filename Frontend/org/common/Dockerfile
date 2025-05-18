FROM node:latest as builder
WORKDIR /app
COPY . .

#stage 2
FROM nginx:latest
EXPOSE 80
EXPOSE 443
COPY ./common/nginx.conf /etc/nginx/nginx.conf
COPY --from=builder app/dist/common /usr/share/nginx/html
