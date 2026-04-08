import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';
import { ProfileComponent } from './profile/profile/profile.component';
import { EditComponent } from './profile/edit/edit.component';
import { PublicationPageComponent } from './publication/publication-page/publication-page.component';
import { SearchPageComponent } from './search/search-page/search-page.component';
import { AdminPanelComponent } from './admin/admin-panel/admin-panel.component';
import { AdminUserListComponent } from './admin/admin-user-list/admin-user-list.component';
import { AdminViolationsComponent } from './admin/admin-violations/admin-violations.component';
import { PasswordResetComponent } from './auth/password-reset/password-reset.component';
import { RecommendationListComponent } from './recommendation/recommendation-list/recommendation-list.component';
import {HomeComponent} from "./home/home/home.component";
import { ComplaintsListComponent } from './complaint/complaints-list-component/complaints-list-component.component';
import { CommentPageComponent } from './publication/comment-page/comment-page.component';
import { PrivateChatComponent } from '../chat/private-chat/private-chat.component';
import { ChatListComponent } from '../chat/chat-list/chat-list.component';
import { CalendarPageComponent } from './publication/calendar-page/calendar-page.component';
import { CreatePlannedPublicationComponent } from './publication/create-planned-publication/create-planned-publication.component';
import { authGuard } from './_guards/auth.guard';

export const routes: Routes = [
    // Public routes — no guard
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'password-reset', component: PasswordResetComponent },

    // Protected routes
    { path: '', component: HomeComponent, canActivate: [authGuard] },
    { path: 'profile/:uniqueNameIdentifier', component: ProfileComponent, canActivate: [authGuard] },
    { path: 'edit-profile', component: EditComponent, canActivate: [authGuard] },
    { path: 'publication/:id', component: PublicationPageComponent, canActivate: [authGuard] },
    { path: 'publications/calendar', component: CalendarPageComponent, canActivate: [authGuard] },
    { path: 'publications/planned-create', component: CreatePlannedPublicationComponent, canActivate: [authGuard] },
    { path: 'search', component: SearchPageComponent, canActivate: [authGuard] },
    { path: 'admin-panel', component: AdminPanelComponent, canActivate: [authGuard] },
    { path: 'admin/user-list', component: AdminUserListComponent, canActivate: [authGuard] },
    { path: 'admin/violation-list/:userId', component: AdminViolationsComponent, canActivate: [authGuard] },
    { path: 'recommended-publications', component: RecommendationListComponent, canActivate: [authGuard] },
    { path: 'complaints', component: ComplaintsListComponent, canActivate: [authGuard] },
    { path: 'comments/:id', component: CommentPageComponent, canActivate: [authGuard] },
    { path: 'chats', component: ChatListComponent, canActivate: [authGuard] },
    { path: 'chat/:id', component: PrivateChatComponent, canActivate: [authGuard] },
];
