import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Account } from '../../models/account';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = 'api/v1/CreateAccount';
  createCustomerAccount(customerId: string): Observable<Account> {
    return this.httpClient.post<Account>(this.apiUrl, { customerId });
  }
}
