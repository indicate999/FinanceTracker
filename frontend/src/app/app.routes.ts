import { Routes } from '@angular/router';
import { AuthGuard } from './_guards/auth.guard';
import { RegisterComponent } from './_components/auth/register/register.component';
import { LoginComponent } from './_components/auth/login/login.component';
import { TransactionsComponent } from './_components/finance/transactions/transactions.component';
import { CategoriesComponent } from './_components/finance/categories/categories.component';
import { ProjectOverviewComponent } from "./_components/information/project-overview/project-overview.component";
import { PrivacyPolicyComponent } from "./_components/information/privacy-policy/privacy-policy.component";

export const routes: Routes = [
  {path: '', redirectTo: '/project-overview', pathMatch: 'full' },
  {path: 'project-overview', component: ProjectOverviewComponent },
  {path: 'login', component: LoginComponent },
  {path: 'register', component: RegisterComponent },
  {
    path: 'categories',
    component: CategoriesComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'transactions',
    component: TransactionsComponent,
    canActivate: [AuthGuard]
  },
  {path: 'privacy-policy', component: PrivacyPolicyComponent }
];
