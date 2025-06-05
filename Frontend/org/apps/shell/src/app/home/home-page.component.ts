import { Component, effect, inject, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import {
  AuthenticationService,
  NotificationService,
  Session,
} from '@org/common';

@Component({
  selector: 'app-home-page',
  imports: [CommonModule, MatCardModule],
  standalone: true,
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.css',
})
export class HomePageComponent {
  private readonly auth = inject(AuthenticationService);
  private readonly notificationService = inject(NotificationService);
  public session: Signal<Session> = this.auth.session;
  public isAuthenticated = this.auth.isAuthenticated;
  public isAnonymous = this.auth.isAnonymous;
}
