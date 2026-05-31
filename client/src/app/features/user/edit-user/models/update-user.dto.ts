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
  Username: string;
  UniqueNameIdentifier: string;
  Pronouns: string | null;
  ProfileDescription: string | null;
  DateOfBirth: string | null; // Format dd.MM.yyyy
  City: string | null;
  Country: string | null;
  ProfileImageUrl: string | null;
  Interests: string[];
}
