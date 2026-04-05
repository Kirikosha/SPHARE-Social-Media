export interface AccountModel {
    username: string,
    uniqueNameIdentifier: string,
    userId: string,
    token: string,
    refreshToken: string,
    role: string,
    blocked: boolean,
}