import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthenticationService } from '../lib/authentication.service';

export const transactionmanagerGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthenticationService);
  const roleClaims = authService
    .session()
    ?.filter(
      (c) =>
        c.type.toLocaleLowerCase() ===
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
    );

  const isTransactionManager = roleClaims?.some(
    (c) => c.value.toLocaleLowerCase() === 'transaction_manager'
  );

  return isTransactionManager || false;
};
