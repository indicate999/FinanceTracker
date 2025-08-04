import { Routes } from '@angular/router';
import { MessageComponent } from './_components/message/message.component';
import { AuthGuard } from './_guards/auth.guard';
import { RegisterComponent } from './_components/auth/register/register.component';
import { LoginComponent } from './_components/auth/login/login.component';
import { TransactionsComponent } from './_components/finance/transactions/transactions.component';
import { CategoriesComponent } from './_components/finance/categories/categories.component';

export const routes: Routes = [
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
  {
    path: 'message',
    component: MessageComponent,
    canActivate: [AuthGuard]
  }
];
