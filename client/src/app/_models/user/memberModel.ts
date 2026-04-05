import { ImageModel } from "../imageModel"
import { AddressModel } from "./addressModel"
import { UserProfileDetailsModel } from "./userProfileDetailsModel"

export interface MemberModel {
    id: string
    username: string,
    uniqueNameIdentifier: string,
    joinedAt: string
    profileImage: ImageModel
    blocked: boolean
    userProfileDetails?: UserProfileDetailsModel
    address?: AddressModel
}