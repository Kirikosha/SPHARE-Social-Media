import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { AdminUserModel } from '../_models/adminUserModel';
import { CreateViolationModel } from '../_models/createViolationModel';
import { ViolationModel } from '../_models/violationModel';
import { map, Observable } from 'rxjs';
import { PaginationParams, PagedList } from '../_models/shared/pagination/pagination';
import { ApiResponse } from '../_models/shared/apiResponse';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  constructor() { }

getUsers(params?: PaginationParams): Observable<PagedList<AdminUserModel>> {
  const httpParams = new HttpParams()
    .set('page', params?.page?.toString() ?? '1')
    .set('pageSize', params?.pageSize?.toString() ?? '10');

  return this.http.get<PagedList<AdminUserModel>>(
    `${this.baseUrl}/admin/get-users`,
    { params: httpParams }
  );
}

  getViolations(userId: string){
    return this.http.get<ViolationModel[]>(`${this.baseUrl}/admin/violations/${userId}`);
  }

  blockUser(userId: string){
    return this.http.post(`${this.baseUrl}/admin/block-user`, JSON.stringify(userId), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  unblockUser(userId: string){
      return this.http.post(`${this.baseUrl}/admin/unblock-user`, JSON.stringify(userId), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  deleteComment(violationData: CreateViolationModel){
    return this.http.post(`${this.baseUrl}/admin/delete-comment`, violationData);
  }

  deletePublication(violationData: CreateViolationModel){
    return this.http.post(`${this.baseUrl}/admin/delete-publication`, violationData);
  }
}
