import { Component, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { CustomerService } from '../services/customer-service';
import { Subject, Subscription, takeUntil } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { MatListModule } from '@angular/material/list';
import { Customer } from '../models/customer';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-main',
  imports: [
    CommonModule,
    MatTabsModule,
    MatInputModule,
    MatButtonModule,
    FormsModule,
    MatListModule,
    MatCardModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './main.component.html',
  styleUrl: './main.component.css',
})
export class MainComponent implements OnDestroy {
  private destroy$ = new Subject<boolean>();
  private readonly customerService = inject(CustomerService);
  public customerId = '';
  public customerDetails: Customer | undefined = undefined;
  public firstName = '';
  public lastName = '';
  public email = '';
  private getcustomerDetailsSub!: Subscription;
  private createCustomerSub!: Subscription;
  private _snackBar = inject(MatSnackBar);
  public loading = false;

  public getCustomerDetails() {
    this.loading = true;
    this.getcustomerDetailsSub?.unsubscribe();
    this.getcustomerDetailsSub = this.customerService
      .getCustomerDetails(this.customerId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data: Customer) => {
          this.customerDetails = data;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error fetching customer details:', error);
          this._snackBar.open('Error occurred', 'Undo');
          this.loading = false;
        },
      });
  }

  public canCreateCustomer(): boolean {
    return this.firstName !== '' && this.lastName !== '' && this.email !== '';
  }

  public createCustomer() {
    if (this.canCreateCustomer()) {
      this.loading = true;
      this.customerDetails = {
        firstName: this.firstName,
        lastName: this.lastName,
        email: this.email,
        created: null,
      };

      this.createCustomerSub?.unsubscribe();
      this.createCustomerSub = this.customerService
        .createCustomer(this.customerDetails)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (data: Customer) => {
            this._snackBar.open('Customer Created ', 'Undo');
            this.loading = false;
          },
          error: (error) => {
            console.error('Error creating customer:', error);
            this._snackBar.open('Error occurred', 'Undo');
            this.loading = false;
          },
        });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next(true);
    this.destroy$.complete();
  }
}
