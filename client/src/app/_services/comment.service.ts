import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { CommentModel, CreateCommentModel } from '../_models/commentModel';
import { PaginationParams, PagedList } from '../_models/shared/pagination/pagination';

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  createComment(commentModel: CreateCommentModel){
    return this.http.post<CommentModel>(this.baseUrl + `/comment`, commentModel);
  }

  getCommentsByPublicationId(publicationId: string, params?: PaginationParams){
    let httpParams = new HttpParams();
    if (params?.page) {
      httpParams = httpParams.set('page', params.page.toString());
    }
    if (params?.pageSize) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }
    return this.http.get<PagedList<CommentModel>>(
      this.baseUrl + `/comment/${publicationId}`, 
      { params: httpParams }
    );
  }

  getReplies(parentId: string, params?: PaginationParams) {
    let httpParams = new HttpParams();
    if (params?.page) {
      httpParams = httpParams.set('page', params.page.toString());
    }
    if (params?.pageSize) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }
    return this.http.get<PagedList<CommentModel>>(
      this.baseUrl + `/comment/${parentId}/replies`, 
      { params: httpParams }
    );
  }

  getComment(id: string) {
    return this.http.get<CommentModel>(this.baseUrl + `/comment/${id}/comment`)
  }

  deleteComment(commentId: string){
    return this.http.delete(this.baseUrl + `/comment/${commentId}`);
  }

  constructor() { }
}
