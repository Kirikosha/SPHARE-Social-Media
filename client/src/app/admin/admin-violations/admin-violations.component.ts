import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AdminService } from '../../_services/admin.service';
import { ViolationModel } from '../../_models/violationModel';
import { DatePipe, NgFor, NgIf } from '@angular/common';

@Component({
  selector: 'app-admin-violations',
  standalone: true,
  imports: [DatePipe, NgIf, NgFor],
  templateUrl: './admin-violations.component.html',
  styleUrl: './admin-violations.component.css'
})
export class AdminViolationsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private adminService = inject(AdminService);

  violations: ViolationModel[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.loadViolations();
  }

  loadViolations() {
    const userId = this.route.snapshot.params['userId'];
    this.adminService.getViolations(userId).subscribe({
      next: (violations) => {
        this.violations = violations;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load violations', err);
        this.isLoading = false;
      }
    });
  }
}
