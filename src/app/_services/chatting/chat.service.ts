import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { ChatDto, ChatWithMessageDto } from '../../_models/chatting/chatDto';
import { StartChatDto } from '../../_models/chatting/startChatDto';
import { MessageDto } from '../../_models/chatting/messageDto';
import { PageDto } from '../../_models/shared/pageDto';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private baseUrl = environment.apiUrl; //     apiUrl : 'https://localhost:5600/api'
  private http = inject(HttpClient); 
  
  startChat(otherUserId: number): Observable<ChatDto> {
    const dto: StartChatDto = {otherUserId};
    return this.http.post<ChatDto>(`${this.baseUrl}/chatrq/start`, dto);
  }

    getMessages(chatId: string, page: number = 1, pageSize: number = 50): Observable<MessageDto[]> {
    const params: any = {
      page: page.toString(),
      pageSize: pageSize.toString()
    };
    
    return this.http.get<MessageDto[]>(`${this.baseUrl}/chatrq/${chatId}/messages`, { params });
  }

    getChats(): Observable<ChatDto[]> {
    return this.http.get<ChatDto[]>(`${this.baseUrl}/chatrq/get-chats`);
  }

    getMessagesWithPageDto(chatId: string, pageDto: PageDto): Observable<MessageDto[]> {
    const params: any = {
      page: pageDto.page.toString(),
      pageSize: pageDto.pageSize.toString()
    };
    
    return this.http.get<MessageDto[]>(`${this.baseUrl}/chatrq/${chatId}/messages`, { params });
  }

    getFirstPageMessages(chatId: string, pageSize: number = 50): Observable<MessageDto[]> {
    return this.getMessages(chatId, 1, pageSize);
  }

    getNextPageMessages(chatId: string, currentPage: number, pageSize: number = 50): Observable<MessageDto[]> {
    return this.getMessages(chatId, currentPage + 1, pageSize);
  }

  getChat(chatId: string) {
    return this.http.get<ChatWithMessageDto>(`${this.baseUrl}/chatrq/${chatId}/open-chat`)
  }
  constructor() { }
}
