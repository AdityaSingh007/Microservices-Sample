FROM node:alpine AS builder
# stage 1
WORKDIR /usr/src/app

COPY . /usr/src/app


RUN npm update -g npm
RUN npm cache clear --force
RUN npm config rm proxy
RUN npm config rm https-proxy
RUN npm config set strict-ssl false
RUN npm install --loglevel verbose -g @nrwl/cli
RUN npm install --loglevel verbose --force
RUN npx nx build transaction --verbose --configuration=production
#RUN pwd && ls

#stage 2
FROM nginx:latest
EXPOSE 80
EXPOSE 443
COPY ./apps/transaction/nginx.conf /etc/nginx/nginx.conf
COPY --from=builder app/dist/apps/transaction /usr/share/nginx/html
