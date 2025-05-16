import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Transaction } from '../models/transaction';
import { map, Observable } from 'rxjs';
import { TransactionType } from '../models/transactionType';

@Injectable({
  providedIn: 'root',
})
export class TransactionService {
  private httpClient = inject(HttpClient);
  private baseUrl = 'api/v1/Transaction';
  public getTransactions(accountId: string): Observable<Transaction[]> {
    return this.httpClient
      .get<Transaction[]>(`${this.baseUrl}/${accountId}`)
      .pipe(
        map((response: any) => {
          return response.map((transaction: any) => {
            return {
              id: transaction.id,
              accountId: transaction.accountId,
              customerId: transaction.customerId,
              amount: transaction.amount,
              createdDate: transaction.createdDate,
              type:
                transaction.type === 0
                  ? TransactionType.Adding
                  : TransactionType.Withdrawing,
            } as Transaction;
          });
        })
      );
  }
}
