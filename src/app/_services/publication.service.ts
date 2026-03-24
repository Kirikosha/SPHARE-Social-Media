import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PublicationModel } from '../_models/publications/publicationModel';
import { UpdatePublicationModel } from '../_models/publications/updatePublicationModel';
import { LikeModel } from '../_models/likeModel';
import { CreatePublicationModel } from '../_models/publications/createPublicationModel';
import { PublicationCalendarModel } from '../_models/publications/publicationCalendarModel';
import { Observable } from 'rxjs';

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

getPublications(uniqueNameIdentifier: string) {
  return this.http.get<PublicationModel[]>(this.baseUrl + '/publication/publication-of-' + uniqueNameIdentifier);
}

getPublicationById(publicationId: number){
  return this.http.get<PublicationModel>(this.baseUrl + `/publication/${publicationId}`);
}

updatePublication(publicationModel: UpdatePublicationModel){
  return this.http.put<PublicationModel>(this.baseUrl + '/publication/update-publication', publicationModel);
}

deletePublication(publicationId: number){
  return this.http.delete(this.baseUrl + `/publication/${publicationId}`); 
}

likePublication(publicationId: number){
  return this.http.get<LikeModel>(this.baseUrl + `/publication/like/${publicationId}`);
}

getRecommendations(){
  return this.http.get<PublicationModel[]>(this.baseUrl + '/publication/recommendations');
}

getPublicationCalendar() {
  return this.http.get<PublicationCalendarModel[]>(this.baseUrl + '/publication/publication-calendar');
}

updatePublicationView(publicationId: number): Observable<number> {
  return this.http.get<number>(`${this.baseUrl}/publication/publication/view-update/${publicationId}`);
}

  constructor() { }
}
