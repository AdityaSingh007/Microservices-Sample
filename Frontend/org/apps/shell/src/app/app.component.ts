import {
  Component,
  effect,
  inject,
  OnDestroy,
  OnInit,
  Signal,
} from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import {
  AuthenticationService,
  NotificationService,
  Session,
} from '@org/common';
import { MatSnackBar } from '@angular/material/snack-bar';
import { filter, Subject, Subscription, takeUntil } from 'rxjs';

@Component({
  imports: [
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatMenuModule,
  ],
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'shell';
  routerService = inject(Router);
  private readonly auth = inject(AuthenticationService);
  public session: Signal<Session> = this.auth.session;
  public isAuthenticated = this.auth.isAuthenticated;
  public isAnonymous = this.auth.isAnonymous;
  public username = this.auth.username;
  public logoutUrl = this.auth.logoutUrl;
  private _snackBar = inject(MatSnackBar);
  private destroy$ = new Subject<boolean>();
  private notificationSub!: Subscription;
  private readonly notificationService = inject(NotificationService);

  constructor() {
    effect(() => {
      if (this.isAuthenticated()) {
        console.log('User is authenticated, starting notification service');
        this.notificationService.startConnection();
        this.notificationService.listenForTransactionNotification();
      }
    });
  }

  logout(): void {
    this.routerService.navigate(['/']);
  }

  ngOnInit(): void {
    this.notificationSub?.unsubscribe();
    this.notificationSub = this.notificationService.notifiy
      .pipe(
        takeUntil(this.destroy$),
        filter((message: string) => message !== '')
      )
      .subscribe((message: string) => {
        this._snackBar.open(message, 'Undo', {
          duration: 5000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next(true);
    this.destroy$.complete();
  }
}
