import { Component, Input } from '@angular/core';
import { MessageDto } from '../../app/_models/chatting/messageDto';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-message-view',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './message-view.component.html',
  styleUrl: './message-view.component.css'
})
export class MessageViewComponent {
  @Input() message!: MessageDto;
  @Input() currentUserId!: string | null;
  @Input() showAvatar = false;
  @Input() avatarUrl: string | undefined | null;
  @Input() username: string | undefined | null;

  get isOwnMessage(): boolean {
    return this.message.senderId === this.currentUserId;
  }

  get messageTime(): string {
    const date = new Date(this.message.sentAt);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m`;
    if (diffHours < 24) return `${diffHours}h`;
    if (diffDays < 7) return `${diffDays}d`;
    return date.toLocaleDateString();
  }
}