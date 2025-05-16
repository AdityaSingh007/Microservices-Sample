import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Customer } from '../models/customer';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = 'api/v1/customer';
  public getCustomerDetails(id: string): Observable<Customer> {
    return this.httpClient.get<Customer>(`${this.apiUrl}/${id}`);
  }

  public createCustomer(customer: Customer): Observable<Customer> {
    return this.httpClient.post<Customer>(this.apiUrl, customer);
  }
}
