import { PublicationModel } from "../publications/publicationModel.js";

export interface PublicationComplaintModel {
    id: number,
    reason: string,
    explanation?: string,
    complainerId: number,
    complainedAt: string,
    publication: PublicationModel
}