import { ImageModel } from "../imageModel"
import { AddressModel } from "./addressModel"
import { UserProfileDetailsModel } from "./userProfileDetailsModel"

export interface MemberModel {
    id: string
    username: string,
    uniqueNameIdentifier: string,
    joinedAt: string
    imageUrl: string
    blocked: boolean
    userProfileDetails?: UserProfileDetailsModel
    address?: AddressModel
}