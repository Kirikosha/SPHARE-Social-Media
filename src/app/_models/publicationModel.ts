import { ImageModel } from "./imageModel";
import { MemberModel } from "./memberModel";

export interface PublicationModel{
    id: number;
    content?: string;
    postedAt: Date;
    updatedAt?: Date;
    images?: ImageModel[];
    author: MemberModel;
}