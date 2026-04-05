import { ImageModel } from "./imageModel";

export interface AdminUserModel {
  id: string;
  username: string;
  uniqueNameIdentifier: string;
  email: string;
  profileImageUrl?: string;
  violationScore: number;
  amountOfViolations: number;
  blocked: boolean;
  blockedAt?: string;
}

export const parseAdminUserDates = (user: AdminUserModel): AdminUserModel & { blockedAtDate?: Date } => ({
  ...user,
  blockedAtDate: user.blockedAt ? new Date(user.blockedAt) : undefined
});