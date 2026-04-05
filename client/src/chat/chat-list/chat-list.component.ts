import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { ChatService } from '../../app/_services/chatting/chat.service';
import { AccountService } from '../../app/_services/account.service';
import { ChatDto } from '../../app/_models/chatting/chatDto';
import { Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ChatUserDto } from '../../app/_models/chatting/chatUserDto';

@Component({
  selector: 'app-chat-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './chat-list.component.html',
  styleUrl: './chat-list.component.css'
})
export class ChatListComponent implements OnInit, OnDestroy {
  private chatService = inject(ChatService)
  private accountService = inject(AccountService)

  private subscriptions: Subscription[] = [];

  chats: ChatDto[] = [];
  filteredChats: ChatDto[] = [];
  currentUserId: string | null = null

  isLoading = false;
  error: string | null = null;
  searchTerm = '';

  currentPage = 1;
  pageSize = 20;
  totalChats = 0;

  ngOnInit(): void {
    this.currentUserId = this.accountService.currentUser()?.userId ?? null;
    this.loadChats();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadChats() {
    this.isLoading = true;
    this.error = null;

    this.subscriptions.push(
        this.chatService.getChats().subscribe({
          next: (chats) => {
            this.chats = chats;
            this.filteredChats = chats;
            this.totalChats = chats.length;
            this.isLoading = false;
          },
          error: (err) => {
            console.error('Error loading chats:', err);
            this.error = 'Failed to load chats. Please try again.';
            this.isLoading = false;
          }
        })
    );
  }

  refreshChats() {
    this.loadChats();
  }

  onSearch() {
    if(!this.searchTerm.trim()) {
      this.filteredChats = this.chats
      return;
    }

    const term = this.searchTerm.toLowerCase().trim();
    this.filteredChats = this.chats.filter(chat => {
      const otherParticipant = this.getOtherChatUser(chat.participants);
      return otherParticipant?.username?.toLowerCase().includes(term) ||
             otherParticipant?.uniqueNameIdentifier?.toLowerCase().includes(term) ||
             chat.lastMessage?.toLowerCase().includes(term);
    });
  }
  
  clearSearch(): void {
    this.searchTerm = '';
    this.filteredChats = this.chats;
  }

  hasUnreadChats(): boolean {
    return this.filteredChats.some(chat => chat.unreadCount > 0);
  }

  loadMore(): void {
    console.log('Load more chats');
  }

  getInitials(name: string): string {
    if (!name) return '?';
    return name.split(' ')
      .map(part => part[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }

  formatLastMessageTime(chat: ChatDto): string {
    return 'Just now';
  }

  trackByChatId(index: number, chat: ChatDto): string {
    return chat.id;
  }

  getOtherChatUser(participants: ChatUserDto[]): ChatUserDto | undefined {
    return participants?.find(s => s.userId !== this.currentUserId);
  }
}