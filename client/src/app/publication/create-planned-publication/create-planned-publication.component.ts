import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CreatePublicationComponent } from "../create-publication/create-publication.component";

@Component({
  selector: 'app-create-planned-publication',
  standalone: true,
  imports: [CreatePublicationComponent],
  templateUrl: './create-planned-publication.component.html',
  styleUrl: './create-planned-publication.component.css'
})
export class CreatePlannedPublicationComponent implements OnInit{
  selectedDateString?: string;
  private route = inject(ActivatedRoute);

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['date']) {
        this.selectedDateString = params['date'];
      }
    })
  }

}
