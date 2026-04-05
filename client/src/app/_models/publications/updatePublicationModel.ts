export interface UpdatePublicationModel {
    id: string;
    content?: string;
    remindAt?: Date;
    conditionTarget?: number | undefined | null;
}