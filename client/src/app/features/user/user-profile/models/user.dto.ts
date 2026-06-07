export interface UserDto {
  id: string;
  username: string;
  uniqueNameIdentifier: string;
  joinedAt: string;
  imageUrl?: string | null;
  blocked: boolean;
  isOwner: boolean;
  userProfileDetails?: UserProfileDetailsDto | null;
  address?: AddressDto | null;
}

export interface UserProfileDetailsDto {
  id: string;
  pronouns?: string | null;
  mainProfileDescription?: string | null;
  interests: string[];
  dateOfBirth?: string | null; // Note: Use `Date | null` if you parse this into a Date object on the frontend
}

export interface AddressDto {
  id: string;
  city?: string | null;
  country?: string | null;
}
