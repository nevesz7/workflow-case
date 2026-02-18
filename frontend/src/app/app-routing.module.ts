import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { RequestListComponent } from './components/request-list/request-list.component';
import { NewRequestComponent } from './components/new-request/new-request.component';
import { RequestDetailComponent } from './components/request-detail/request-detail.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'requests', component: RequestListComponent },
  { path: 'requests/new', component: NewRequestComponent },
  { path: 'requests/:id', component: RequestDetailComponent },
  { path: '', redirectTo: '/login', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }