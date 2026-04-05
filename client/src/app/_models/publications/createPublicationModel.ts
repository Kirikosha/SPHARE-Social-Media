export interface CreatePublicationModel{
    content: string;
    publicationType: 'ordinary' | 'planned' | 'plannedConditional';
    remindAt?: Date;
    images?: File[];
    conditionType?: 'SubscriberCount';
    conditionTarget?: number | null;
    comparisonOperator?: 'GreaterThanOrEqual'
}