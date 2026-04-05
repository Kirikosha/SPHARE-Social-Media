import { ReasonModel } from "./reasonModel";

export interface GroupPublicationComplaintModel {
    publicationId: number,
    totalComplaints: number,
    reasons: ReasonModel[]
}