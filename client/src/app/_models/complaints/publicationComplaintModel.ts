import { PublicationModel } from "../publications/publicationModel";

export interface PublicationComplaintModel {
    id: number,
    reason: string,
    explanation?: string,
    complainerId: number,
    complainedAt: string,
    publication: PublicationModel
}