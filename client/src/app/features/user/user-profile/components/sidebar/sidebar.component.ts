import {Component, Input} from '@angular/core';
import {UserDto} from "../../models/user.dto";
import {AvatarComponent} from "../avatar/avatar.component";

@Component({
  selector: 'app-sidebar',
  imports: [
    AvatarComponent
  ],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent {
  @Input() userProfile!: UserDto | null;
}
