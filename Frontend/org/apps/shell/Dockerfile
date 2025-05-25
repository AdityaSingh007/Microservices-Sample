FROM node:latest AS builder
# stage 1
WORKDIR /app
COPY package*.json /app
COPY . .

RUN rm -rf node_modules
RUN npm install --verbose --force
RUN npm install -g @nrwl/cli
RUN npx nx build shell --configuration=production
#RUN pwd && ls

#stage 2
FROM nginx:latest
EXPOSE 80
EXPOSE 443
COPY ./apps/shell/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=builder app/dist/apps/shell /usr/nginx/html
