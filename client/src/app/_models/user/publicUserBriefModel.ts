export interface PublicUserBriefModel  {
    id: string;
    username: string;
    uniqueNameIdentifier : string;
    imageUrl?: string;
    blocked: boolean;
}