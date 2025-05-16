import { computed, inject, Injectable, Signal } from '@angular/core';
import { catchError, defer, Observable, of, shareReplay } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import { Session } from '../model/session';

const ANONYMOUS: Session = null;
const CACHE_SIZE = 1;

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  private readonly http = inject(HttpClient);
  private session$: Observable<Session> | null = null;
  // Create a signal from the getSession() observable.
  // Maybe in the future this can be a Resource once the api stabilizes.
  public session: Signal<Session> = toSignal(
    defer(() => this.getSession()), // Defer the getSession call
    { initialValue: ANONYMOUS }
  );
  // Derived signals using computed that automatically update.
  public isAuthenticated = computed(() => this.session() !== null);
  public isAnonymous = computed(() => this.session() === null);
  public username = computed(() => {
    const session = this.session();
    return session
      ? session.find((c) => c.type === 'name')?.value || null
      : null;
  });

  public logoutUrl = computed(() => {
    const session = this.session();
    return session
      ? session.find((c) => c.type === 'bff:logout_url')?.value || null
      : null;
  });

  public getSession(ignoreCache = false): Observable<Session> {
    if (!this.session$ || ignoreCache) {
      this.session$ = this.http.get<Session>('bff/user').pipe(
        catchError((err) => of(ANONYMOUS)),
        shareReplay(CACHE_SIZE)
      );
    }
    return this.session$;
  }
}
