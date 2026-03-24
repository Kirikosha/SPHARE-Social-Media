export interface AccountModel {
    username: string,
    uniqueNameIdentifier: string,
    userId: number,
    token: string,
    refreshToken: string,
    role: string,
    blocked: boolean,
}