import { Component, ViewEncapsulation } from '@angular/core';
import {MatTabsModule} from '@angular/material/tabs'
import { CreatePublicationComponent } from "../../publication/create-publication/create-publication.component";

@Component({
  selector: 'app-profile-tabs',
  standalone: true,
  imports: [MatTabsModule, CreatePublicationComponent],
  templateUrl: './profile-tabs.component.html',
  styleUrl: './profile-tabs.component.css',
  encapsulation: ViewEncapsulation.None
})
export class ProfileTabsComponent {

}
