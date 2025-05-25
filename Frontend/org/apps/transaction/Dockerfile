FROM node:latest AS builder
# stage 1
WORKDIR /app
COPY . .

RUN rm -rf node_modules
RUN npm install --verbose --force
RUN npm install -g @nrwl/cli
RUN npx nx build transaction --configuration=production
#RUN pwd && ls

#stage 2
FROM nginx:latest
EXPOSE 8080
EXPOSE 443
COPY ./apps/transaction/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=builder app/dist/apps/transaction /usr/nginx/html
