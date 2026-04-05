import { Component, inject, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import {MatTabsModule} from '@angular/material/tabs'
import { CreatePublicationComponent } from "../../publication/create-publication/create-publication.component";
import { PublicationListComponent } from "../../publication/publication-list/publication-list.component";
import { AccountService } from '../../_services/account.service';
import { ActivatedRoute } from '@angular/router';


@Component({
  selector: 'app-profile-tabs',
  standalone: true,
  imports: [MatTabsModule, CreatePublicationComponent, PublicationListComponent],
  templateUrl: './profile-tabs.component.html',
  styleUrl: './profile-tabs.component.css',
  encapsulation: ViewEncapsulation.None
})
export class ProfileTabsComponent implements OnInit {
  accountService = inject(AccountService);
  private route = inject(ActivatedRoute);
  
  // Icons

  // Tab management
  activeTab: 'posts' | 'comments' = 'posts';
  uniqueNameIdentifier: string = '';
  isOwner: boolean = false;

  @ViewChild('publicationList') publicationList!: PublicationListComponent;

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.uniqueNameIdentifier = params['uniqueNameIdentifier'];
      this.isOwner = this.accountService.currentUser()?.uniqueNameIdentifier === this.uniqueNameIdentifier;
    });
  }

  refreshPublications(): void {
    if (this.publicationList) {
      this.publicationList.loadPublications(this.uniqueNameIdentifier);
    }
  }
}