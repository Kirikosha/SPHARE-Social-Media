import {Component, inject, OnInit} from '@angular/core';
import {SidebarComponent} from "./sidebar/sidebar.component";
import {UserProfileService} from "../services/user-profile.service";
import {UserDto} from "../models/user.dto";
import {ActivatedRoute} from "@angular/router";

@Component({
  selector: 'app-profile-page',
  imports: [
    SidebarComponent
  ],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.css'
})
export class ProfilePageComponent implements OnInit {
  private userProfileService: UserProfileService = inject(UserProfileService);
  private route = inject(ActivatedRoute);
  userProfile: UserDto | null = null;

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const uniqueNameIdentifier = params['uNI'];
      if (uniqueNameIdentifier) {
        this.loadProfile(uniqueNameIdentifier);
      }
      else {
        console.error("No profile loaded since no UNI given")
      }
    })
  }

  loadProfile(uNI: string) {
    this.userProfileService.getProfile(uNI).subscribe({
      next: (profile) => {
        this.userProfile = profile;
      },
      error: (error) => {
        console.error(error);
      }
    })
  }
}
