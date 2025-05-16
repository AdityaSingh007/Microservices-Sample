import { Route } from '@angular/router';
import { HomePageComponent } from './home/home-page.component';
import { transactionmanagerGuard } from '@org/common';

export const appRoutes: Route[] = [
  {
    path: 'account',
    canActivate: [transactionmanagerGuard],
    loadChildren: () => import('account/Routes').then((m) => m!.remoteRoutes),
  },
  {
    path: 'transaction',
    canActivate: [transactionmanagerGuard],
    loadChildren: () =>
      import('transaction/Routes').then((m) => m!.remoteRoutes),
  },
  {
    path: 'customer',
    canActivate: [transactionmanagerGuard],
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
