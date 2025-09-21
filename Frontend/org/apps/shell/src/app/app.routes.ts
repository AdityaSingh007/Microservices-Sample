import { Route } from '@angular/router';
import { HomePageComponent } from './home/home-page.component';
import { transactionmanagerGuard } from '@org/common';
import { loadRemoteModule } from '@nx/angular/mf';

export const appRoutes: Route[] = [
  {
    path: 'account',
    canActivate: [transactionmanagerGuard],
    loadChildren: () =>
      loadRemoteModule('account', './Routes').then((m) => m.remoteRoutes),
  },
  {
    path: 'transaction',
    canActivate: [transactionmanagerGuard],
    loadChildren: () =>
      loadRemoteModule('transaction', './Routes').then((m) => m.remoteRoutes),
  },
  {
    path: 'customer',
    canActivate: [transactionmanagerGuard],
    loadChildren: () =>
      loadRemoteModule('customer', './Routes').then((m) => m.remoteRoutes),
  },
  {
    path: 'login',
    loadChildren: () =>
      loadRemoteModule('login', './Routes').then((m) => m.remoteRoutes),
  },
  {
    path: '',
    component: HomePageComponent,
  },
];
