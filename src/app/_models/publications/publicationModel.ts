import { ImageModel } from "../imageModel";
import { MemberModel } from "../user/memberModel";
import { PublicUserBriefModel } from "../user/publicUserBriefModel";
import { PublicationCardModel } from "./publicationCardModel";

export interface PublicationModel{
    id: string;
    content?: string;
    postedAt: Date;
    updatedAt?: Date;
    remindAt?: Date;
    images?: string[];
    author: PublicUserBriefModel;
    likesAmount: number;
    isLikedByCurrentUser: boolean;
    commentAmount: number;
    conditionType?: 'SubscriberCount';
    publicationType: string;
    conditionTarget?: number;
    comparisonOperator?: 'GreaterThanOrEqual'
    viewCount: number;
    isDeleted: boolean;
}

export const mapPublicationToCard = (pub: PublicationModel): PublicationCardModel => ({
  id: pub.id,
  content: pub.content,
  postedAt: pub.postedAt,
  updatedAt: pub.updatedAt,
  remindAt: pub.remindAt,
  images: pub.images,
  author: pub.author,
  likesAmount: pub.likesAmount,
  isLikedByCurrentUser: pub.isLikedByCurrentUser,
  commentAmount: pub.commentAmount,
  publicationType: pub.publicationType,  // ✅ Union type is assignable to string
  viewCount: pub.viewCount,
  isDeleted: pub.isDeleted
});