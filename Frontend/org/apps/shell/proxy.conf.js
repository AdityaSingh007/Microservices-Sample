const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost`
  : env.ASPNETCORE_URLS
  ? env.ASPNETCORE_URLS.split(';')[0]
  : 'https://localhost';

const PROXY_CONFIG = [
  {
    context: [
      // BFF Management Routes
      '/bff',

      // OIDC Handler Routes
      '/signin-oidc',
      '/signout-callback-oidc',

      // API Routes
      '/api'
    ],
    target,
    secure: false,
  },
  {
    context: [
      // signalR hub endpoint
      '/notificationHub'
    ],
    target,
    secure: false,
    ws:true
  }
];

module.exports = PROXY_CONFIG;
