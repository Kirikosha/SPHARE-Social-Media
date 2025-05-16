import { Component, inject, OnInit } from '@angular/core';
import { MemberService } from '../../_services/member.service';
import { MemberModel } from '../../_models/memberModel';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../../_services/account.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [],
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
}
