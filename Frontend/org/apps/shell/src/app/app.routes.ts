import { Route } from '@angular/router';
import { AppComponent } from './app.component';
import { HomePageComponent } from './home/home-page.component';

export const appRoutes: Route[] = [
  {
    path: 'account',
    loadChildren: () => import('account/Routes').then((m) => m!.remoteRoutes),
  },
  {
    path: 'transaction',
    loadChildren: () =>
      import('transaction/Routes').then((m) => m!.remoteRoutes),
  },
  {
    path: 'customer',
    loadChildren: () => import('customer/Routes').then((m) => m!.remoteRoutes),
  },
  {
    path: 'login',
    loadChildren: () => import('login/Routes').then((m) => m!.remoteRoutes),
  },
  {
    path: '',
    component: HomePageComponent,
  },
];
