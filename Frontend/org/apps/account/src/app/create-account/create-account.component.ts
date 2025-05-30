import { Component, inject, OnDestroy, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subject, Subscription, takeUntil } from 'rxjs';
import { AccountService } from '../services/account/account.service';
import { Account } from '../models/account';

@Component({
  selector: 'app-create-account',
  imports: [
    CommonModule,
    MatInputModule,
    MatButtonModule,
    FormsModule,
    MatSnackBarModule,
  ],
  templateUrl: './create-account.component.html',
  styleUrl: './create-account.component.css',
})
export class CreateAccountComponent implements OnDestroy {
  private destroy$ = new Subject<boolean>();
  private readonly accountService = inject(AccountService);
  private accountService$!: Subscription;
  isLoading = output<boolean>();
  private _snackBar = inject(MatSnackBar);
  accountId = '';
  ngOnDestroy(): void {
    this.destroy$.next(true);
    this.destroy$.complete();
  }
  customerId = '';
  createAccount(): void {
    if (!this.customerId || this.customerId.trim() === '') {
      return;
    }
    this.isLoading.emit(true);
    this.accountService$?.unsubscribe();
    this.accountService$ = this.accountService
      .createCustomerAccount(this.customerId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (account: Account) => {
          this.isLoading.emit(false);
          this._snackBar.open('Account created', 'Undo', {
            duration: 2000,
          });
          this.accountId = account.accountId;
          this.customerId = '';
        },
        error: (error) => {
          this.isLoading.emit(false);
          this._snackBar.open('Error occurred', 'Undo', {
            duration: 2000,
          });
        },
      });
  }
}
