FROM node:latest as node
WORKDIR /app
COPY . .

#stage 2
FROM nginx:alpine
COPY ./common/nginx.conf /etc/nginx/nginx.conf
COPY --from=node app/dist/common /usr/share/nginx/html