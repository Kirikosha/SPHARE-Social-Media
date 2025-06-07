export interface CreatePublicationModel{
    content: string;
    publicationType: 'ordinary' | 'planned';
    remindAt?: Date;
    images?: File[];
}