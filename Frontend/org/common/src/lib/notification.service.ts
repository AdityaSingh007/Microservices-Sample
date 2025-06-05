import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private hubConnection: signalR.HubConnection;
  public notifiy = new BehaviorSubject<string>('');
  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('api/transactionHub', {
        headers: {
          'X-CSRF': '1', // Custom header for CSRF protection
        },
      }) // SignalR hub URL
      .build();
  }

  startConnection(): void {
    this.hubConnection
      .start()
      .then(() => console.log('SignalR connection established'))
      .catch((err) =>
        console.error('Error establishing SignalR connection:', err)
      );
  }

  listenForTransactionNotification(): void {
    this.hubConnection.on('NotifyTransactionStatus', (notification: string) => {
      console.log('Transaction notification received:', notification);
      this.notifiy.next(notification);
    });
  }
}
