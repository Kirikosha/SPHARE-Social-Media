import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminUserModel, parseAdminUserDates } from '../../_models/adminUserModel';
import { PaginationParams, PagedList } from '../../_models/shared/pagination/pagination';

@Component({
  selector: 'app-admin-user-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './admin-user-list.component.html',
  styleUrl: './admin-user-list.component.css'
})
export class AdminUserListComponent implements OnInit {
  private adminService = inject(AdminService);
  private toastr = inject(ToastrService);
  
  users: AdminUserModel[] = [];
  
  currentPage = 1;
  totalPages = 0;
  totalCount = 0;
  pageSize = 10;
  readonly pageSizes = [10, 25, 50];
  hasNextPage = false;
  hasPreviousPage = false;
  
  isLoading = false;
  searchTerm = '';
  currentSort = 'username';
  isAscending = true;

  ngOnInit(): void {
    this.loadUsers(); 
  }

  loadUsers(page: number = 1, pageSize: number = this.pageSize) {
    this.isLoading = true;
    
    const params: PaginationParams = {
      page,
      pageSize: Math.min(Math.max(pageSize, 1), 50)
    };

    this.adminService.getUsers(params)
      .subscribe({
        next: (pagedResult: PagedList<AdminUserModel>) => {
          console.log('Received paged users:', pagedResult);
          this.users = pagedResult.items.map(parseAdminUserDates);
          
          this.totalCount = pagedResult.totalCount;
          this.currentPage = pagedResult.pageNumber;
          this.totalPages = pagedResult.totalPages;
          this.pageSize = pagedResult.pageSize;
          this.hasNextPage = pagedResult.hasNextPage;
          this.hasPreviousPage = pagedResult.hasPreviousPage;
          
          this.isLoading = false;
        },
        error: (err) => {
          console.error('Failed to load users', err);
          this.toastr.error('Failed to load users', 'Error');
          this.isLoading = false;
        }
      });
  }


goToPage(page: number | string): void {
  if (typeof page !== 'number' || page === this.currentPage) return;
  
  if (page < 1 || page > this.totalPages) return;

  this.loadUsers(page, this.pageSize);
}

  nextPage(): void {
    if (this.hasNextPage) this.loadUsers(this.currentPage + 1, this.pageSize);
  }

  prevPage(): void {
    if (this.hasPreviousPage) this.loadUsers(this.currentPage - 1, this.pageSize);
  }

  onPageSizeChange(event: Event): void {
    const newPageSize = +(event.target as HTMLSelectElement).value;
    this.pageSize = newPageSize;
    this.loadUsers(1, newPageSize);
  }

  get pageNumbers(): (number | string)[] {
    const pages: (number | string)[] = [];
    const maxVisible = 5;
    
    if (this.totalPages <= maxVisible + 2) {
      for (let i = 1; i <= this.totalPages; i++) pages.push(i);
    } else {
      pages.push(1);
      
      let start = Math.max(2, this.currentPage - 2);
      let end = Math.min(this.totalPages - 1, this.currentPage + 2);
      
      if (start > 2) pages.push('...');
      
      for (let i = start; i <= end; i++) pages.push(i);
      
      if (end < this.totalPages - 1) pages.push('...');
      
      if (this.totalPages > 1) pages.push(this.totalPages);
    }
    
    return pages;
  }


  onBlockUser(userId: string): void {
    this.adminService.blockUser(userId).subscribe({
      next: () => {
        this.toastr.success('User blocked successfully');
        this.updateUserStatus(userId, true);
      },
      error: () => this.toastr.error('Failed to block user', 'Error')
    });
  }

  onUnblockUser(userId: string): void {
    this.adminService.unblockUser(userId).subscribe({
      next: () => {
        this.toastr.success('User unblocked successfully');
        this.updateUserStatus(userId, false);
      },
      error: () => this.toastr.error('Failed to unblock user', 'Error')
    });
  }

  private updateUserStatus(userId: string, isBlocked: boolean): void {
    const user = this.users.find(u => u.id === userId);
    if (user) {
      user.blocked = isBlocked;
      user.blockedAt = isBlocked ? new Date().toISOString() : undefined;
    }
  }

  sortUsers(column: keyof AdminUserModel): void {
    if (this.currentSort === column) {
      this.isAscending = !this.isAscending;
    } else {
      this.currentSort = column;
      this.isAscending = true;
    }

    this.users.sort((a, b) => {
      const valA = a[column];
      const valB = b[column];

      if (valA == null && valB == null) return 0;
      if (valA == null) return this.isAscending ? -1 : 1;
      if (valB == null) return this.isAscending ? 1 : -1;

      if (typeof valA === 'string' && typeof valB === 'string') {
        return this.isAscending 
          ? valA.localeCompare(valB)
          : valB.localeCompare(valA);
      } else if (typeof valA === 'number' && typeof valB === 'number') {
        return this.isAscending ? valA - valB : valB - valA;
      } else if (typeof valA === 'boolean' && typeof valB === 'boolean') {
        return this.isAscending 
          ? Number(valA) - Number(valB) 
          : Number(valB) - Number(valA);
      }
      return 0;
    });
  }

  get filteredUsers(): AdminUserModel[] {
    if (!this.searchTerm.trim()) return this.users;
    
    const term = this.searchTerm.toLowerCase();
    return this.users.filter(user => 
      user.username?.toLowerCase().includes(term) ||
      user.email?.toLowerCase().includes(term) ||
      user.uniqueNameIdentifier?.toLowerCase().includes(term)
    );
  }

  get showingRange(): string {
    if (this.totalCount === 0) return 'No users';
    const start = (this.currentPage - 1) * this.pageSize + 1;
    const end = Math.min(this.currentPage * this.pageSize, this.totalCount);
    return `Showing ${start}-${end} of ${this.totalCount}`;
  }
}