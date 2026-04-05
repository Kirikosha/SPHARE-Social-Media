import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { CreateComplaintModel } from '../_models/complaints/createComplaintModel';
import { PublicationComplaintModel } from '../_models/complaints/publicationComplaintModel';
import { CommentComplaintModel } from '../_models/complaints/commentComplaintModel';
import { GroupPublicationComplaintModel } from '../_models/complaints/groupPublicationComplaintModel';

@Injectable({
  providedIn: 'root'
})
export class ComplaintService {
  private http = inject(HttpClient)
  private baseUrl = environment.apiUrl

  makePublicationComplaint(complaint: CreateComplaintModel) {
    return this.http.post<boolean>(`${this.baseUrl}/complaint/publication`, complaint);
  }

  makeCommentComplaint(complaint: CreateComplaintModel) {
    return this.http.post<boolean>(`${this.baseUrl}/complaint/comment`, complaint);
  }

  getPublicationComplaints() {
    return this.http.get<PublicationComplaintModel[]>(`${this.baseUrl}/complaint/publication`)
  }

  getCommentComplaints() {
    return this.http.get<CommentComplaintModel[]>(`${this.baseUrl}/complaint/comment`)
  }

  getPublicationGroupedComplaints() {
    return this.http.get<GroupPublicationComplaintModel[]>(`${this.baseUrl}/complaint/publication-grouped`)
  }

  constructor() { }
}
