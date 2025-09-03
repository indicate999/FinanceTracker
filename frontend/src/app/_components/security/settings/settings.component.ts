import { Component } from '@angular/core';
import { AuthService } from "../../../_services/auth.service";
import { Router } from "@angular/router";

@Component({
  selector: 'app-settings',
  imports: [],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent {
  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  deleteAccount(): void {
    if (confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
      this.authService.deleteAccount().subscribe({
        next: (response) => {
          console.log('Account deleted successfully:', response);
          this.authService.logout();
          alert('Your account has been successfully deleted.');
          this.router.navigate(['/login']);
        },
        error: (error) => {
          console.error('Error deleting account:', error);
          alert('Failed to delete account. Please try again.');
        }
      });
    }
  }

}
