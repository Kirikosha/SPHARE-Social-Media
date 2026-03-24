import { Component, Input } from '@angular/core';
import { MemberModel } from '../../app/_models/user/memberModel';
import { PublicUserBriefModel } from '../../app/_models/user/publicUserBriefModel';

@Component({
  selector: 'app-icon',
  standalone: true,
  imports: [],
  templateUrl: './icon.component.html',
  styleUrl: './icon.component.css'
})
export class IconComponent {
  @Input() user!: PublicUserBriefModel
}
