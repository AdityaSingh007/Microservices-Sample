FROM node:latest AS builder
# stage 1
WORKDIR /app
COPY . .

RUN rm -rf node_modules
RUN npm install --verbose --force
#RUN npm install -g @nrwl/cli
RUN npx nx build customer --configuration=production
#RUN pwd && ls

#stage 2
FROM nginx:latest
EXPOSE 80
EXPOSE 443
COPY ./apps/customer/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=builder app/dist/apps/customer /usr/nginx/html
