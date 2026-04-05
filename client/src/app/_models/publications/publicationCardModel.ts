import { PagedList } from "../shared/pagination/pagination";
import { PublicUserBriefModel } from "../user/publicUserBriefModel";

export interface PublicationCardModel {
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
    publicationType: string;
    viewCount: number;
    isDeleted?: boolean;
}

export const mapPublicationCard = (apiPub: any): PublicationCardModel => ({
  id: apiPub.id,
  content: apiPub.content,
  postedAt: new Date(apiPub.postedAt),
  updatedAt: apiPub.updatedAt ? new Date(apiPub.updatedAt) : undefined,
  remindAt: apiPub.remindAt ? new Date(apiPub.remindAt) : undefined,
  images: apiPub.imageUrls,
  author: apiPub.author,
  likesAmount: apiPub.likesAmount,
  isLikedByCurrentUser: apiPub.isLikedByCurrentUser,
  commentAmount: apiPub.commentAmount,
  publicationType: apiPub.publicationType?.toString() ?? '0',
  viewCount: apiPub.viewCount,
  isDeleted: apiPub.isDeleted
});

export const mapPagedPublications = (pagedResult: any): PagedList<PublicationCardModel> => ({
  items: pagedResult.items?.map(mapPublicationCard) ?? [],
  totalCount: pagedResult.totalCount,
  pageNumber: pagedResult.pageNumber,
  pageSize: pagedResult.pageSize,
  totalPages: pagedResult.totalPages,
  hasNextPage: pagedResult.hasNextPage,
  hasPreviousPage: pagedResult.hasPreviousPage
});