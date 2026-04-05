import { MemberModel } from "./user/memberModel";

export interface CommentModel {
    id: string;
    content: string;
    author: MemberModel;
    publicationId: string;
    creationDate: Date;
    isDeleted: boolean;
    repliesAmount: number;
}

export interface CreateCommentModel {
    content: string;
    publicationId: string;
    parentId?: string;
}