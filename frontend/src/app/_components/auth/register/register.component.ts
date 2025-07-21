import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import {Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../_services/auth.service';

@Component({
  selector: 'app-register',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent implements OnInit {
  public registerForm!: FormGroup;
  public validationErrors: string[] = [];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      username: ['', [Validators.required, Validators.maxLength(20)]],
      password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(20)]],
      displayName: ['', [Validators.required, Validators.maxLength(20)]]
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) return;

    console.log('📤 Form data:', this.registerForm.value);
    this.authService.register(this.registerForm.value).subscribe({
      next: () => this.router.navigate(['/login']),
      error: (err) => {
        console.error('Registration error:', err);
        this.validationErrors = [];

        if (Array.isArray(err.error)) {
          err.error.forEach((e: any) => {
            if (e.description) {
              this.validationErrors.push(e.description);
            }
          });
        } else if (typeof err.error === 'string') {
          this.validationErrors.push(err.error);
        } else {
          this.validationErrors.push('Unexpected error occurred.');
        }
      }
    });
  }
}
