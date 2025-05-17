FROM node:latest as node
WORKDIR /app
COPY . .

#stage 2
FROM nginx:alpine
COPY ./common/nginx.conf /etc/nginx/nginx.conf
COPY --from=node app/common /usr/share/nginx/html
