import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr'
import { BehaviorSubject, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
export interface Message {
  chatId: string;
  content: string;
  senderId: string;
  timestamp?: Date;
}

export interface PresenceEvent {
  userId: number;
  isOnline: boolean;
}

export interface TypingEvent {
  userId: string;
  chatId: string;
}

export interface SendMessageError {
  message: string;
  code?: number;
}

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  hubConnection: signalR.HubConnection | undefined;
  
  // Observable for connection state
  public connectionState$ = new BehaviorSubject<boolean>(false);
  
  // Subjects for receiving data
  public messageReceived$ = new Subject<Message>();
  public connectionEstablished$ = new BehaviorSubject<boolean>(false);
  public connectionError$ = new Subject<any>();
  public sendMessageError$ = new Subject<SendMessageError>();
  
  // New observables for presence and typing
  public contactPresenceChanged$ = new Subject<PresenceEvent>();
  public userTyping$ = new Subject<TypingEvent>();
  public userStoppedTyping$ = new Subject<TypingEvent>();
  public userJoinedChat$ = new Subject<number>(); // userId
  public userLeftChat$ = new Subject<number>(); // userId
  
  private url = "https://localhost:5600"
  // Store token getter function
  private tokenGetter: (() => string | Promise<string>) | null = null;

  constructor() {}

  public startConnection(token: string | (() => string | Promise<string>)): Promise<void> {
    if (typeof token === 'function') {
      this.tokenGetter = token;
    }
    
    // Build connection URL
    const hubUrl = `${this.url}/chat`;
    
    // Create hub connection
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: async () => {
          if (typeof token === 'string') {
            return token;
          } else {
            return await token();
          }
        }
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryContext => {
          if (retryContext.previousRetryCount < 4) {
            return Math.pow(2, retryContext.previousRetryCount) * 1000;
          }
          return 30000;
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Register hub methods
    this.registerOnHubMethods();
    
    // Start connection
    return this.hubConnection.start()
      .then(() => {
        console.log('SignalR Connected');
        this.connectionEstablished$.next(true);
        this.connectionState$.next(true);
      })
      .catch(err => {
        console.error('Error while starting connection:', err);
        this.connectionError$.next(err);
        throw err;
      });
  }

  private registerOnHubMethods(): void {
    if (!this.hubConnection) return;

    // Listen for incoming messages
    this.hubConnection.on('ReceiveMessage', (messageData: any) => {
      // Assuming your backend sends a MessageResponseDto
      const message: Message = {
        chatId: messageData.chatId,
        content: messageData.content,
        senderId: messageData.senderId,
        timestamp: messageData.sentAt ? new Date(messageData.sentAt) : new Date()
      };
      this.messageReceived$.next(message);
    });

    // Listen for errors
    this.hubConnection.on('SendMessageError', (error: SendMessageError) => {
      this.sendMessageError$.next(error);
    });

    // Presence events
    this.hubConnection.on('ContactPresenceChanged', (userId: number, isOnline: boolean) => {
      this.contactPresenceChanged$.next({ userId, isOnline });
    });

    // Typing indicators
    this.hubConnection.on('UserTyping', (userId: string, chatId: string) => {
      this.userTyping$.next({ userId, chatId });
    });
    
    this.hubConnection.on('UserStoppedTyping', (userId: string, chatId: string) => {
      this.userStoppedTyping$.next({ userId, chatId });
    });

    // Chat participation events
    this.hubConnection.on('UserJoinedChat', (userId: number) => {
      this.userJoinedChat$.next(userId);
    });
    
    this.hubConnection.on('UserLeftChat', (userId: number) => {
      this.userLeftChat$.next(userId);
    });

    // Connection events
    this.hubConnection.onreconnected(() => {
      console.log('SignalR Reconnected');
      this.connectionState$.next(true);
    });

    this.hubConnection.onreconnecting(() => {
      console.log('SignalR Reconnecting...');
      this.connectionState$.next(false);
    });

    this.hubConnection.onclose(() => {
      console.log('SignalR Connection Closed');
      this.connectionState$.next(false);
    });
  }


  public async sendTypingIndicator(chatId: string): Promise<void> {
    if (!this.hubConnection) return;
    await this.hubConnection.invoke('SendTypingIndicator', chatId);
  }

  public async stopTypingIndicator(chatId: string): Promise<void> {
    if (!this.hubConnection) return;
    await this.hubConnection.invoke('StopTypingIndicator', chatId);
  }

  public async joinChat(chatId: string): Promise<void> {
  if (!this.hubConnection) {
    throw new Error('Hub connection not established');
  }
  await this.hubConnection.invoke('JoinChat', chatId);
}

/**
 * Leave a chat group
 */
public async leaveChat(chatId: string): Promise<void> {
  if (!this.hubConnection) {
    throw new Error('Hub connection not established');
  }
  await this.hubConnection.invoke('LeaveChat', chatId);
}

/**
 * Send a message
 */
public async sendMessage(chatId: string, receiverId: string, message: string): Promise<void> {
  if (!this.hubConnection) {
    throw new Error('Hub connection not established');
  }
  await this.hubConnection.invoke('SendMessage', chatId, receiverId, message);
}
}
