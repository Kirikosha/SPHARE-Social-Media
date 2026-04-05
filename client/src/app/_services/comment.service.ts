import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { CommentModel, CreateCommentModel } from '../_models/commentModel';

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  createComment(commentModel: CreateCommentModel){
    return this.http.post<CommentModel>(this.baseUrl + `/comment`, commentModel);
  }

  getCommentsByPublicationId(publicationId: string){
    return this.http.get<CommentModel[]>(this.baseUrl + `/comment/${publicationId}`);
  }

  getReplies(parentId: string) {
    return this.http.get<CommentModel[]>(this.baseUrl + `/comment/${parentId}/replies`);
  }

  getComment(id: string) {
    return this.http.get<CommentModel>(this.baseUrl + `/comment/${id}/comment`)
  }

  deleteComment(commentId: string){
    return this.http.delete(this.baseUrl + `/comment/${commentId}`);
  }

  constructor() { }
}
