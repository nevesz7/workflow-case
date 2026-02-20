import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { RequestListComponent } from './components/request-list/request-list.component';
import { NewRequestComponent } from './components/new-request/new-request.component';
import { RequestDetailComponent } from './components/request-detail/request-detail.component';
import { AuthGuard } from './guards/auth.guard';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: 'requests',
    canActivate: [AuthGuard], 
    children: [
      { path: '', component: RequestListComponent }, 
      { path: 'new', component: NewRequestComponent }, 
      { path: ':id', component: RequestDetailComponent } 
    ]
  },
  { path: '', redirectTo: '/login', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }