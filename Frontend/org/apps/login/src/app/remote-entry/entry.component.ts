import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserLoginComponent } from '../user-login/user-login.component';

@Component({
  imports: [CommonModule, UserLoginComponent],
  selector: 'app-login-entry',
  template: `<app-user-login></app-user-login>`,
})
export class RemoteEntryComponent {}
