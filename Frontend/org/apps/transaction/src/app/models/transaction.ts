import { TransactionType } from './transactionType';

export interface Transaction {
  id: number;
  accountId: string;
  amount: number;
  customerId: string;
  createdDate: string;
  type: TransactionType;
}
