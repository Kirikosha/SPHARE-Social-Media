import { Component, inject, OnDestroy, OnInit, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';

import { SignalrService, Message, TypingEvent, PresenceEvent } from '../../app/_services/chatting/signalr.service';
import { ChatService } from '../../app/_services/chatting/chat.service';
import { AccountService } from '../../app/_services/account.service';
import { MessageDto } from '../../app/_models/chatting/messageDto';
import { ChatWithMessageDto } from '../../app/_models/chatting/chatDto';
import { ChatUserDto } from '../../app/_models/chatting/chatUserDto';
import { MessageViewComponent } from "../message-view/message-view.component";

@Component({
  selector: 'app-private-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MessageViewComponent],
  templateUrl: './private-chat.component.html',
  styleUrl: './private-chat.component.css'
})
export class PrivateChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;
  
  // Services
  private signalRService = inject(SignalrService);
  private chatService = inject(ChatService);
  private accountService = inject(AccountService);
  private route = inject(ActivatedRoute);
  
  // State
  isConnected = false;
  isLoading = false;
  isSending = false;
  error: string | null = null;
  
  // Chat data
  chatId: string = '';
  chat: ChatWithMessageDto | null = null;
  messages: MessageDto[] = [];
  otherUser: ChatUserDto | null = null;
  currentUserId: string | null = null;
  
  // Message input
  newMessage = '';
  
  // Typing indicators
  isTyping = false;
  otherUserIsTyping = false;
  private typingTimeout: any;
  
  // Pagination
  currentPage = 1;
  pageSize = 50;
  hasMoreMessages = false; // Will be set after initial load
  isLoadingMore = false;
  
  private subscriptions: Subscription[] = [];

  ngOnInit(): void {
    this.currentUserId = this.accountService.currentUser()?.userId ?? null;
    
    // Get chat ID from route
    this.subscriptions.push(
      this.route.params.subscribe(params => {
        this.chatId = params['id'];
        if (this.chatId) {
          this.loadChatDetails();
          this.initializeSignalR();
        }
      })
    );
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    
    // Leave chat group using SignalR service method
    if (this.chatId && this.isConnected) {
      this.signalRService.leaveChat(this.chatId)
        .catch(err => console.error('Failed to leave chat:', err));
    }
    
    // Stop typing indicator
    this.stopTypingIndicator();
  }

  /**
   * Load chat details and messages using the new /open-chat endpoint
   */
private loadChatDetails(): void {
  this.isLoading = true;
  this.error = null;
  
  this.subscriptions.push(
    this.chatService.getChat(this.chatId).subscribe({
      next: (chat) => {
        this.chat = chat;
        this.messages = chat.messages ? [...chat.messages].reverse() : [];
        this.hasMoreMessages = chat.messages?.length === this.pageSize;
        
        // Store current online status before replacing the user object
        const currentOnlineStatus = this.otherUser?.isOnline;
        
        this.otherUser = this.getOtherParticipant(chat);
        
        // Preserve real-time presence status over stale API data
        if (this.otherUser && currentOnlineStatus !== undefined) {
          this.otherUser.isOnline = currentOnlineStatus;
        }
        
        this.isLoading = false;
        this.markAsRead();
      },
      error: (err) => {
        console.error('Failed to load chat:', err);
        this.error = 'Failed to load conversation';
        this.isLoading = false;
      }
    })
  );
}

  /**
   * Load more messages (older messages) via pagination endpoint
   */
  loadMoreMessages(): void {
    if (this.isLoadingMore || !this.hasMoreMessages) return;
    
    this.isLoadingMore = true;
    this.currentPage++;
    
    this.subscriptions.push(
      this.chatService.getMessages(this.chatId, this.currentPage, this.pageSize).subscribe({
        next: (olderMessages) => {
          // API returns descending order, we need to reverse to prepend correctly
          const reversedMessages = [...olderMessages].reverse();
          
          // Prepend older messages
          this.messages = [...reversedMessages, ...this.messages];
          
          // If we received fewer than pageSize, there are no more messages
          this.hasMoreMessages = olderMessages.length === this.pageSize;
          this.isLoadingMore = false;
        },
        error: (err) => {
          console.error('Failed to load more messages:', err);
          this.isLoadingMore = false;
          this.currentPage--; // Revert page increment
        }
      })
    );
  }

  /**
   * Initialize SignalR connection and join the chat room
   */
  private initializeSignalR(): void {
    const token = this.accountService.currentUser()?.token;
    if (!token) {
      console.error('No authentication token found');
      return;
    }

    // Start connection
    this.signalRService.startConnection(token)
      .then(() => {
        console.log('SignalR connection established');
        // Join the chat room using service method
        return this.signalRService.joinChat(this.chatId);
      })
      .then(() => {
        console.log(`Joined chat: ${this.chatId}`);
      })
      .catch(err => {
        console.error('Failed to establish SignalR connection:', err);
      });

    // Subscribe to connection state
    this.subscriptions.push(
      this.signalRService.connectionState$.subscribe(state => {
        this.isConnected = state;
      })
    );

    // Subscribe to incoming messages
    this.subscriptions.push(
      this.signalRService.messageReceived$.subscribe((message: Message) => {
        if (message.chatId === this.chatId) {
          // Convert to MessageDto format
          const newMessage: MessageDto = {
            id: crypto.randomUUID(), // Temporary ID (server will have real ID)
            content: message.content,
            sentAt: message.timestamp || new Date(),
            editedAt: null,
            wasEdited: false,
            senderId: message.senderId,
            sendersUsername: message.senderId === this.currentUserId 
              ? 'You' 
              : (this.otherUser?.username || 'User'),
            isRead: false
          };
          
          this.messages = [...this.messages, newMessage];
          this.scrollToBottom();
        }
      })
    );

    // Subscribe to typing indicators
    this.subscriptions.push(
      this.signalRService.userTyping$.subscribe((event: TypingEvent) => {
        if (event.chatId === this.chatId && event.userId !== this.currentUserId) {
          this.otherUserIsTyping = true;
        }
      })
    );

    this.subscriptions.push(
      this.signalRService.userStoppedTyping$.subscribe((event: TypingEvent) => {
        if (event.chatId === this.chatId && event.userId !== this.currentUserId) {
          this.otherUserIsTyping = false;
        }
      })
    );

    // Subscribe to errors
    this.subscriptions.push(
      this.signalRService.sendMessageError$.subscribe(error => {
        console.error('Send message error:', error);
        this.error = 'Failed to send message. Please try again.';
        this.isSending = false;
      })
    );

    this.subscriptions.push(
      this.signalRService.contactPresenceChanged$.subscribe((event: PresenceEvent) => {
        // Check if this is the other user in the chat
        // Note: You may need to convert types if userId is string vs number
        if (this.otherUser && String(event.userId) === this.otherUser.userId) {
          this.otherUser.isOnline = event.isOnline;
      }
    })
    );
  }

  /**
   * Send a message via SignalR
   */
  async sendMessage(): Promise<void> {
    if (!this.newMessage.trim() || !this.chatId || !this.currentUserId || !this.otherUser || !this.isConnected) {
      return;
    }

    this.isSending = true;
    
    try {
      // Send via SignalR using service method
      await this.signalRService.sendMessage(
        this.chatId,
        this.otherUser.userId,
        this.newMessage.trim()
      );
      
      // Clear input and stop typing indicator
      this.newMessage = '';
      this.stopTypingIndicator();
      
    } catch (err) {
      console.error('Failed to send message:', err);
      this.error = 'Failed to send message';
    } finally {
      this.isSending = false;
    }
  }

  /**
   * Handle typing indicator
   */
  onInputChange(): void {
    if (!this.isConnected) return;
    
    if (!this.isTyping) {
      this.isTyping = true;
      this.signalRService.sendTypingIndicator(this.chatId);
    }
    
    // Clear existing timeout
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
    }
    
    // Set timeout to stop typing after 2 seconds of inactivity
    this.typingTimeout = setTimeout(() => {
      this.stopTypingIndicator();
    }, 2000);
  }

  /**
   * Stop typing indicator
   */
  private stopTypingIndicator(): void {
    if (this.isTyping && this.isConnected) {
      this.isTyping = false;
      this.signalRService.stopTypingIndicator(this.chatId);
    }
    
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
    }
  }

  /**
   * Mark current chat as read (to be implemented)
   */
  private markAsRead(): void {
    // TODO: Implement read receipts
    // this.chatService.markAsRead(this.chatId).subscribe();
  }

  /**
   * Get other participant in 1-on-1 chat
   */
  private getOtherParticipant(chat: ChatWithMessageDto): ChatUserDto | null {
    if (!chat?.participants || !this.currentUserId) return null;
    return chat.participants.find(p => p.userId !== this.currentUserId) || null;
  }

  /**
   * Format message time for display
   */
  formatMessageTime(date: Date): string {
    if (!date) return '';
    
    const messageDate = new Date(date);
    const now = new Date();
    const diffMs = now.getTime() - messageDate.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);
    
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m`;
    if (diffHours < 24) return `${diffHours}h`;
    if (diffDays < 7) return `${diffDays}d`;
    
    return messageDate.toLocaleDateString();
  }

  /**
   * Scroll to bottom of messages
   */
  private scrollToBottom(): void {
    try {
      setTimeout(() => {
        if (this.messagesContainer) {
          this.messagesContainer.nativeElement.scrollTop = 
            this.messagesContainer.nativeElement.scrollHeight;
        }
      }, 100);
    } catch (err) {
      console.error('Error scrolling to bottom:', err);
    }
  }

  /**
   * Track by function for ngFor performance
   */
  trackByMessageId(index: number, message: MessageDto): string {
    return message.id;
  }
}