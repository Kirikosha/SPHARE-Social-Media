import { PublicUserBriefModel } from "../user/publicUserBriefModel";
import { PublicationCardModel } from "./publicationCardModel";

export enum PublicationTypes {
  ordinary = 0,
  planned = 1,
  plannedConditional = 2
}

export interface PublicationModel {
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
    conditionType?: 'SubscriberCount' | null;
    publicationType: PublicationTypes;  // Use the enum type
    conditionTarget?: number | null;
    comparisonOperator?: 'GreaterThanOrEqual' | null;
    viewCount: number;
    isDeleted: boolean;
}

// Helper to convert enum number to string for display
export const getPublicationTypeString = (type: PublicationTypes): string => {
  switch (type) {
    case PublicationTypes.ordinary: return 'ordinary';
    case PublicationTypes.planned: return 'planned';
    case PublicationTypes.plannedConditional: return 'plannedConditional';
    default: return 'unknown';
  }
};

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
  publicationType: getPublicationTypeString(pub.publicationType),  // Map number to string
  viewCount: pub.viewCount,
  isDeleted: pub.isDeleted
});