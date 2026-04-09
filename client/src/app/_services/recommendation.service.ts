import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { PublicationModel } from '../_models/publications/publicationModel';

export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class RecommendationService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;

  getRecommendations(params: PaginationParams) {
    let httpParams = new HttpParams()
      .set('PageNumber', params.pageNumber.toString())
      .set('PageSize', params.pageSize.toString());

    return this.http.get<PaginatedResponse<PublicationModel>>(
      `${this.baseUrl}/recommendation`, 
      { params: httpParams }
    );
  }
}