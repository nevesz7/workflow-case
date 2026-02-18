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
    canActivate: [AuthGuard], // Apply guard once to the parent
    children: [
      { path: '', component: RequestListComponent }, // Matches /requests
      { path: 'new', component: NewRequestComponent }, // Matches /requests/new
      { path: ':id', component: RequestDetailComponent } // Matches /requests/some-id
    ]
  },
  { path: '', redirectTo: '/login', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }