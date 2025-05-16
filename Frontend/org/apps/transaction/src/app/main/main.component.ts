import { Component, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TransactionService } from '../services/transaction.service';
import { Transaction } from '../models/transaction';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, Subscription, takeUntil } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-main',
  imports: [
    CommonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    FormsModule,
    MatInputModule,
    MatButtonModule,
    MatTableModule,
  ],
  templateUrl: './main.component.html',
  styleUrl: './main.component.css',
})
export class MainComponent implements OnDestroy {
  private destroy$ = new Subject<boolean>();
  private transactionService = inject(TransactionService);
  public Transactions: Transaction[] = [];
  public accountId = '';
  TransactionSub$!: Subscription;
  public loading = false;
  public displayedColumns: string[] = [
    'id',
    'accountId',
    'customerId',
    'amount',
    'createdDate',
    'type',
  ];

  loadTransactions() {
    if (!this.accountId || this.accountId.trim() === '') {
      return;
    }
    this.loading = true;
    this.TransactionSub$?.unsubscribe();
    this.TransactionSub$ = this.transactionService
      .getTransactions(this.accountId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (transactions: Transaction[]) => {
          this.Transactions = transactions;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading transactions:', error);
          this.loading = false;
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next(true);
    this.destroy$.complete();
    if (this.TransactionSub$) {
      this.TransactionSub$.unsubscribe();
    }
  }
}
