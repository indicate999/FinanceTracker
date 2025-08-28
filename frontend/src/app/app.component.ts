import { Component } from '@angular/core';
import {Router, RouterModule, RouterOutlet } from '@angular/router';
import { AuthService } from './_services/auth.service';
import { CommonModule } from "@angular/common";

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'frontend';

  constructor(public authService: AuthService, private router: Router) {}

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
