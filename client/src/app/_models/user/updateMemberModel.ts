import { UpdateAddressModel } from "./updateAddressModel";
import { UpdateUserProfileDetailsModel } from "./updateUserProfileDetailsModel";

export interface UpdateMemberModel {
    id: string;
    username: string;
    uniqueNameIdentifier: string;
    joinedAt: string;
    profileImage?: File;
    removeProfileImage: boolean;
    blocked: boolean;
    userProfileDetails?: UpdateUserProfileDetailsModel
    address?: UpdateAddressModel
}