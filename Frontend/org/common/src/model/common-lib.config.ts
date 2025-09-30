import { InjectionToken } from '@angular/core';

export interface LibEnvironment {
  production: string;
  frontendHost: string;
}

export const COMMON_LIB_ENVIRONMENT = new InjectionToken<LibEnvironment>(
  'COMMON_LIB_ENVIRONMENT'
);
