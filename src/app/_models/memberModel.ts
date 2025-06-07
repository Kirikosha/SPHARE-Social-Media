import { ImageModel } from "./imageModel"

export interface MemberModel {
    username: string,
    uniqueNameIdentifier: string,
    joinedAt: string
    profileImage: ImageModel
}