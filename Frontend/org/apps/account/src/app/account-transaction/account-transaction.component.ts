import { Component, inject, OnDestroy, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { TransactionType } from '../models/transactiontype';
import { Account } from '../models/account';
import { AccountService } from '../services/account/account.service';
import { TransactionRequest } from '../models/transactionrequest';
import { Subject, Subscription, takeUntil } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/module.d-CnjH8Dlt';

@Component({
  selector: 'app-account-transaction',
  imports: [
    CommonModule,
    MatInputModule,
    MatButtonModule,
    FormsModule,
    MatSnackBarModule,
    MatCardModule,
  ],
  templateUrl: './account-transaction.component.html',
  styleUrl: './account-transaction.component.css',
})
export class AccountTransactionComponent implements OnDestroy {
  private destroy$ = new Subject<boolean>();
  private readonly accountService = inject(AccountService);
  customerId = '';
  accountId = '';
  transactionAmount = 0;
  private _snackBar = inject(MatSnackBar);
  private accountService$!: Subscription;
  isLoading = output<boolean>();

  ngOnDestroy(): void {
    this.destroy$.next(true);
    this.destroy$.complete();
  }

  canPerformTransaction(): boolean {
    return (
      this.customerId !== '' &&
      this.accountId !== '' &&
      this.transactionAmount > 0
    );
  }

  performTransaction(transactionType: TransactionType | string): void {
    this.isLoading.emit(true);
    const transactionRequest: TransactionRequest = {
      customerId: this.customerId,
      accountId: this.accountId,
      amount: this.transactionAmount,
    };

    this.accountService$?.unsubscribe();
    this.accountService$ = this.accountService
      .performTransaction(
        transactionType as TransactionType,
        transactionRequest
      )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: Account) => {
          console.log(response);
          this._snackBar.open('Request accepted', 'Undo', {
            duration: 2000,
          });
          this.isLoading.emit(false);
          this.customerId = '';
          this.accountId = '';
          this.transactionAmount = 0;
        },
        error: (error: HttpErrorResponse) => {
          console.error(error);
          this._snackBar.open('Error occurred', 'Undo', {
            duration: 2000,
          });
          this.isLoading.emit(false);
        },
      });
  }
}
