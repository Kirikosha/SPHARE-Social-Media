import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { mapPublicationToCard, PublicationModel } from '../_models/publications/publicationModel';
import { UpdatePublicationModel } from '../_models/publications/updatePublicationModel';
import { LikeModel } from '../_models/likeModel';
import { CreatePublicationModel } from '../_models/publications/createPublicationModel';
import { PublicationCalendarModel } from '../_models/publications/publicationCalendarModel';
import { map, Observable } from 'rxjs';
import { PublicationCardModel, mapPagedPublications } from '../_models/publications/publicationCardModel';
import { ApiResponse } from '../_models/shared/apiResponse';
import { PaginationParams, PagedList } from '../_models/shared/pagination/pagination';

@Injectable({
  providedIn: 'root'
})
export class PublicationService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;

createPublication(publicationModel: CreatePublicationModel) {
  const formData = new FormData();
  
  // Basic fields
  formData.append('Content', publicationModel.content);
  formData.append('PublicationType', publicationModel.publicationType);
  
  if (publicationModel.remindAt) {
    formData.append('RemindAt', publicationModel.remindAt.toISOString());
  }
  
  // Conditional fields
  if (publicationModel.conditionType) {
    formData.append('ConditionType', publicationModel.conditionType);
  }
  if (publicationModel.conditionTarget != null) {
    formData.append('ConditionTarget', publicationModel.conditionTarget.toString());
  }
  if (publicationModel.comparisonOperator) {
    formData.append('ConditionOperator', publicationModel.comparisonOperator);
  }
  
  // Images
  if (publicationModel.images) {
    publicationModel.images.forEach((image) => {
      formData.append('Images', image, image.name);
    });
  }
  
  return this.http.post(this.baseUrl + '/publication/create-publication', formData);
}

getPublications(uniqueNameIdentifier: string, params?: PaginationParams) {
  let httpParams = new HttpParams()
    .set('page', params?.page?.toString() ?? '1')
    .set('pageSize', params?.pageSize?.toString() ?? '10');

  return this.http
    .get<PagedList<any>>(
      `${this.baseUrl}/publication/publication-of-${uniqueNameIdentifier}`,
      { params: httpParams }
    )
    .pipe(
      map(apiPagedList => mapPagedPublications(apiPagedList))
    );
}

getPublicationById(publicationId: string){
  return this.http.get<PublicationModel>(this.baseUrl + `/publication/${publicationId}`);
}

updatePublication(publicationModel: UpdatePublicationModel): Observable<PublicationCardModel> {
  return this.http.put<PublicationModel>(
    `${this.baseUrl}/publication/update-publication`, 
    publicationModel
  ).pipe(
    map(fullPub => mapPublicationToCard(fullPub))
  );
}

deletePublication(publicationId: string){
  return this.http.delete(this.baseUrl + `/publication/${publicationId}`); 
}

likePublication(publicationId: string){
  return this.http.get<LikeModel>(this.baseUrl + `/publication/like/${publicationId}`);
}

getRecommendations(){
  return this.http.get<PublicationModel[]>(this.baseUrl + '/publication/recommendations');
}

getPublicationCalendar() {
  return this.http.get<PublicationCalendarModel[]>(this.baseUrl + '/publication/publication-calendar');
}

updatePublicationView(publicationId: string): Observable<number> {
  return this.http.get<number>(`${this.baseUrl}/publication/publication/view-update/${publicationId}`);
}

  constructor() { }
}
