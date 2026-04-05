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

export const routes: Routes = [
    {path: 'login', component:LoginComponent},
    {path: 'register', component: RegisterComponent},
    {path: 'profile/:uniqueNameIdentifier', component: ProfileComponent},
    {path: 'edit-profile', component: EditComponent},
    {path: 'publication/:id', component: PublicationPageComponent},
    {path: 'publications/calendar', component: CalendarPageComponent},
    {path: 'publications/planned-create', component: CreatePlannedPublicationComponent},
    {path: 'search', component: SearchPageComponent},
    {path: 'admin-panel', component: AdminPanelComponent},
    {path: 'admin/user-list', component: AdminUserListComponent},
    {path: 'admin/violation-list/:userId', component: AdminViolationsComponent},
    {path: 'password-reset', component: PasswordResetComponent},
    {path: 'recommended-publications', component: RecommendationListComponent},
    {path: 'complaints', component: ComplaintsListComponent},
    {path: 'comments/:id', component: CommentPageComponent},
    {path: 'chats', component: ChatListComponent},
    {path: 'chat/:id', component: PrivateChatComponent},
    {path: '', component: HomeComponent}
       ];
