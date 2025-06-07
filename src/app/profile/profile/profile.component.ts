import { Component, inject, OnInit } from '@angular/core';
import { MemberService } from '../../_services/member.service';
import { MemberModel } from '../../_models/memberModel';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../../_services/account.service';
import { ProfileTabsComponent } from "../profile-tabs/profile-tabs.component";

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
  memberModel!: MemberModel;

  ngOnInit(): void {
    this.loadMember();
  }
  
  loadMember(){
    this.memberService.getMyProfile().subscribe({
      next: (member) => {
        this.memberModel = member;
        console.log(this.memberModel);
      },
      error: (err) => {
        this.toastr.error(err.error);
      }
    })
  }

    formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      month: 'long', 
      year: 'numeric' 
    });
  }
}
