import { Routes } from '@angular/router';
import { MessageComponent } from './_components/message/message.component';
import { AuthGuard } from './_guards/auth.guard';
import { RegisterComponent } from './_components/auth/register/register.component';
import { LoginComponent } from './_components/auth/login/login.component';

export const routes: Routes = [
  {path: 'login', component: LoginComponent },
  {path: 'register', component: RegisterComponent },
  {
    path: 'message',
    component: MessageComponent,
    canActivate: [AuthGuard]
  }
];
