import { ChatUserDto } from "./chatUserDto"
import { MessageDto } from "./messageDto"

export interface ChatDto {
    id: string
    participants: ChatUserDto[]
    lastMessage: string
    unreadCount: number
}

export interface ChatWithMessageDto {
    id: string,
    participants: ChatUserDto[],
    lastMessage: string,
    unreadCount: number,
    messages: MessageDto[]
}