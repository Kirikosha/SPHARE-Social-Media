import { jwtDecode } from "jwt-decode";

export function isTokenExpired(token:string): boolean{
    try {
        const decoded: any = jwtDecode(token);
        const expiry = decoded.exp;
        const now = Math.floor(new Date().getTime() / 1000);
        return expiry < now;
    } catch{
        return true;
    }
}