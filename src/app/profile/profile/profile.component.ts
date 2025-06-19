import { Component, inject, OnInit } from '@angular/core';
import { MemberService } from '../../_services/member.service';
import { MemberModel } from '../../_models/memberModel';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../../_services/account.service';
import { ProfileTabsComponent } from "../profile-tabs/profile-tabs.component";
import { ActivatedRoute, Router } from '@angular/router';
import { SubscriptionService } from '../../_services/subscription.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [ProfileTabsComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  private memberService = inject(MemberService);
  private toastr = inject(ToastrService);
  private router = inject(Router);
  accountService = inject(AccountService);
  private subscriptionService = inject(SubscriptionService);
  private route = inject(ActivatedRoute);

  memberModel!: MemberModel;
  isCurrentUserProfile = false;
  isFollowing = false;
  followingCount = 0;
  followersCount = 0;
  isLoading = true;

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const uniqueNameIdentifier = params['uniqueNameIdentifier'];
      if (uniqueNameIdentifier) {
        this.loadOtherMember(uniqueNameIdentifier);
      } else {
        this.loadMember();
      }
    });
  }
  
  loadMember(){
    this.memberService.getMyProfile().subscribe({
      next: (member) => {
        this.memberModel = member;
        this.checkIfCurrentUser();
        this.loadSubscriptionCounts();
        this.checkIfFollowing();
        this.isLoading = false;
      },
      error: (err) => {
        this.toastr.error(err.error);
        this.isLoading = false;
      }
    })
  }

    loadOtherMember(uniqueNameIdentifier: string): void {
    this.memberService.getOtherProfile(uniqueNameIdentifier).subscribe({
      next: (member) => {
        this.handleMemberData(member);
      },
      error: (err) => {
        this.toastr.error(err.error);
        this.isLoading = false;
        this.router.navigate(['/']); // Redirect if user not found
      }
    });
  }
  
  private handleMemberData(member: MemberModel): void {
    this.memberModel = member;
    this.checkIfCurrentUser();
    this.loadSubscriptionCounts();
    this.checkIfFollowing();
    this.isLoading = false;
  }

formatDate(dateString: string): string {
  const [day, month, year] = dateString.split('.');
  const date = new Date(parseInt(year), parseInt(month) - 1, parseInt(day)); // month is 0-based
  return date.toLocaleDateString('en-US', { 
    month: 'long', 
    year: 'numeric' 
  });
}

  goToUpdate() {
    this.router.navigate(['/edit-profile']);
  }

  checkIfCurrentUser() {
    const currentUser = this.accountService.currentUser();
    this.isCurrentUserProfile = currentUser?.uniqueNameIdentifier === this.memberModel.uniqueNameIdentifier;
  }

  loadSubscriptionCounts() {
    this.subscriptionService.getSubscriptionCount(this.memberModel.uniqueNameIdentifier).subscribe({
      next: (count) => {this.followingCount = count; console.log("Following count: ", count)},
      error: () => this.followingCount = 0
    });

    this.subscriptionService.getFollowerCount(this.memberModel.uniqueNameIdentifier).subscribe({
      next: (count) => this.followersCount = count,
      error: () => this.followersCount = 0
    })
  } 

  checkIfFollowing() {
    if (this.isCurrentUserProfile)  return;

    this.subscriptionService.isFollowing(this.memberModel.uniqueNameIdentifier).subscribe({
      next: (isFollowing) => {
        this.isFollowing = isFollowing;
      },
      error: (err) => {
        this.toastr.error("Failed to check following status: " + err.error);
      }
    });
  }

toggleFollow() {
  if (this.isCurrentUserProfile || this.accountService.currentUser()?.blocked) {
    return;
  }
  
  if (this.memberModel.blocked) {
    this.toastr.warning('You cannot follow a blocked account');
    return;
  }

  if (this.isFollowing) {
    this.unfollowUser();
  } else {
    this.followUser();
  }
}


  followUser() {
    this.subscriptionService.subscribe(this.memberModel.uniqueNameIdentifier).subscribe({
      next: () => {
        this.isFollowing = true;
        this.followersCount++;
      },
      error: (err) => {
        this.toastr.error("Failed to follow user:" + err.error);
      }
    })
  }

  unfollowUser(){
    this.subscriptionService.unsubscribe(this.memberModel.uniqueNameIdentifier).subscribe({
      next: () => {
        this.isFollowing = false;
        this.followersCount--;
      },
      error: (err) => {
        this.toastr.error("Failed to unfollow user: " + err.error);
      }
    })
  }
}
