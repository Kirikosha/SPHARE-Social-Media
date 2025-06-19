import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { AdminUserModel } from '../_models/adminUserModel';
import { CreateViolationModel } from '../_models/createViolationModel';
import { ViolationModel } from '../_models/violationModel';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  constructor() { }

  getUsers() {
    return this.http.get<AdminUserModel[]>(`${this.baseUrl}/admin/get-users`);
  }

  getViolations(userId: number){
    return this.http.get<ViolationModel[]>(`${this.baseUrl}/admin/violations/${userId}`);
  }

  blockUser(userId: number){
    return this.http.post(`${this.baseUrl}/admin/block-user`, userId);
  }

  unblockUser(userId: number){
    return this.http.post(`${this.baseUrl}/admin/unblock-user`, userId);
  }

  deleteComment(violationData: CreateViolationModel){
    return this.http.post(`${this.baseUrl}/admin/delete-comment`, violationData);
  }

  deletePublication(violationData: CreateViolationModel){
    return this.http.post(`${this.baseUrl}/admin/delete-publication`, violationData);
  }
}
