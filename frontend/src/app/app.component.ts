import { Component } from '@angular/core';
import {Router, RouterModule, RouterOutlet } from '@angular/router';
import { MessageComponent } from './_components/message/message.component';
import { AuthService } from './_services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MessageComponent, RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'frontend';

  constructor(private authService: AuthService, private router: Router) {}

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
