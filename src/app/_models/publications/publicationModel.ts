import { ImageModel } from "../imageModel";
import { MemberModel } from "../user/memberModel";

export interface PublicationModel{
    id: number;
    content?: string;
    postedAt: Date;
    updatedAt?: Date;
    remindAt?: Date;
    images?: ImageModel[];
    author: MemberModel;
    likesAmount: number;
    isLikedByCurrentUser: boolean;
    commentAmount: number;
    conditionType?: 'SubscriberCount';
    conditionTarget?: number;
    comparisonOperator?: 'GreaterThanOrEqual'
    viewCount: number;
}