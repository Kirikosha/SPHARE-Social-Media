export interface MessageDto {
    id: string
    content: string
    sentAt: Date
    editedAt?: Date | null
    wasEdited: boolean
    senderId: string
    sendersUsername: string
    isRead: boolean
}