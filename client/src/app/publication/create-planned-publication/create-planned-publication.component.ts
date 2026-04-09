import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CreatePublicationComponent } from "../create-publication/create-publication.component";
import { AccountService } from '../../_services/account.service';

@Component({
  selector: 'app-create-planned-publication',
  standalone: true,
  imports: [CreatePublicationComponent],
  templateUrl: './create-planned-publication.component.html',
  styleUrl: './create-planned-publication.component.css'
})
export class CreatePlannedPublicationComponent implements OnInit {
  selectedDateString?: string;
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private accountService = inject(AccountService);

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['date']) {
        this.selectedDateString = params['date'];
      }
    })
  }

  onPublicationCreated() {
    const currentUser = this.accountService.currentUser();
    if (currentUser?.uniqueNameIdentifier) {
      this.router.navigate(['/profile', currentUser.uniqueNameIdentifier]);
    } else {
      // Fallback to home or login if no user found
      this.router.navigateByUrl('/');
    }
  }
}