export interface UpdateMainInfoDto {
    username: string;
    uniqueNameIdentifier: string;
}

export interface UpdateAdditionalInfoDto {
    pronouns?: string;
    profileDescription?: string;
    interests?: string[];
    dateOfBirth?: Date;
}

export interface UpdateAddressDto {
    city?: string;
    country?: string;
}

export interface UpdateProfileImageDto {
    profileImage: File;
}


export interface UserUpdateDataDto {
  username: string;
  uniqueNameIdentifier: string;
  pronouns: string | null;
  profileDescription: string | null;
  dateOfBirth: string | null; // Format dd.MM.yyyy
  city: string | null;
  country: string | null;
  profileImageUrl: string | null;
  interests: string[];
}
