import { Inject, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import {
  COMMON_LIB_ENVIRONMENT,
  LibEnvironment,
} from '../model/common-lib.config';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private hubConnection: signalR.HubConnection;
  public notifiy = new BehaviorSubject<string>('');
  constructor(@Inject(COMMON_LIB_ENVIRONMENT) env: LibEnvironment) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`https://${env.frontendHost}/notificationHub`, {
        headers: {
          'X-CSRF': '1', // Custom header for CSRF protection
        },
      }) // SignalR hub URL
      .withAutomaticReconnect()
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
    this.hubConnection.on('SendNotification', (notification: any) => {
      console.log('Transaction notification received:', notification);
      this.notifiy.next(notification);
    });
  }
}
