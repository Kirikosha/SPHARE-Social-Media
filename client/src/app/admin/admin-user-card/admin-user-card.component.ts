import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { AdminUserModel } from '../../_models/adminUserModel';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-user-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-user-card.component.html',
  styleUrl: './admin-user-card.component.css'
})
export class AdminUserCardComponent {
  @Input() user!: AdminUserModel;
  @Output() blockUser = new EventEmitter<number>();
  @Output() unblockUser = new EventEmitter<number>();

  private router = inject(Router);

  onBlockUser(){
    this.blockUser.emit(this.user.id);
  }

  onUnblockUser(){
    this.unblockUser.emit(this.user.id);
  }

  viewViolations() {
    this.router.navigate(['/admin/violation-list', this.user.id]);
  }

getViolationPercentage(): number {
  const max = 20;
  return Math.min((this.user.violationScore / max) * 100, 100);
}


  getViolationClass(): string {
    if (this.user.violationScore >= 15) return 'high-violation';
    if (this.user.violationScore >= 10) return 'medium-violation';
    return 'low-violation';
  }

}
