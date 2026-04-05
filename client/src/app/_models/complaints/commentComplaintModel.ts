import { CommentModel } from "../commentModel";

export interface CommentComplaintModel {
    id: number,
    reason: string,
    explanation?: string,
    complainerId: number,
    complainedAt: string,
    comment: CommentModel
}