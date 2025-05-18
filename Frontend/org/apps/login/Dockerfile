FROM node:latest as builder
# stage 1
WORKDIR /app
COPY . .

RUN npm update -g npm
RUN npm config set strict-ssl false
RUN npm install -g @nrwl/cli
RUN npm install --force
RUN npx nx build login --configuration=production
#RUN pwd && ls

#stage 2
FROM nginx:latest
EXPOSE 80
EXPOSE 443
COPY ./apps/login/nginx.conf /etc/nginx/nginx.conf
COPY --from=builder app/dist/apps/login /usr/share/nginx/html
