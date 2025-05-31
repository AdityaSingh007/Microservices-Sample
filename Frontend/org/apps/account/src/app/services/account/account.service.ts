import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Account } from '../../models/account';
import { TransactionType } from '../../models/transactiontype';
import { TransactionRequest } from '../../models/transactionrequest';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = 'api/v1/CreateAccount';
  createCustomerAccount(customerId: string): Observable<Account> {
    return this.httpClient.post<Account>(this.apiUrl, { customerId });
  }

  performTransaction(
    transactionType: TransactionType,
    transactionRequest: TransactionRequest
  ): Observable<Account> {
    const transactionUrl =
      transactionType === TransactionType.Deposit
        ? `api/v1/AddToAccount`
        : `api/v1/Withdrawing`;

    return this.httpClient.post<Account>(transactionUrl, transactionRequest);
  }
}
