import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import {Router, RouterModule } from '@angular/router';
import { Category, CategoryVm, Transaction } from '../../../_models/finance.models';
import { CategoryService } from '../../../_services/category.service';

@Component({
  selector: 'app-categories',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss'
})
export class CategoriesComponent implements OnInit {
  categoryForm!: FormGroup;

  categories: CategoryVm[] = [];

  errorMessage: string | null = null;

  editingCategoryId: number | null = null;
  isEditing: boolean = false;

  openPopoverId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(50)]],
      type: ['Neutral', Validators.required]
    });

    this.loadCategories();
  }

  loadCategories(): void {
    this.categoryService.getCategoriesWithCounts().subscribe({
      next: (data) => {
        this.categories = data.map(c => ({
          ...c,
          isLoading: false,
          error: null
        }));
      },
      error: () => this.errorMessage = 'Failed to load categories'
    });
  }

  addCategory(): void {
    if (this.categoryForm.invalid) return;

    const newCategory: Category = this.categoryForm.value;

    this.categoryService.createCategory(newCategory).subscribe({
      next: (category) => {
        const vm: CategoryVm = { ...category, transactionCount: 0, isLoading: false, error: null };
        this.categories.push(vm);
        this.categoryForm.reset({ type: 'Neutral' });
      },
      error: () => this.errorMessage = 'Failed to add category'
    });
  }

  startEdit(category: CategoryVm): void {
    if (category.name === 'WITHOUT CATEGORY') return;

    this.isEditing = true;
    this.editingCategoryId = category.id;
    this.categoryForm.setValue({
      name: category.name,
      type: category.type
    });
  }

  saveEdit(): void {
    if (this.categoryForm.invalid || this.editingCategoryId === null) return;

    const updatedCategory: Category = {
      id: this.editingCategoryId,
      ...this.categoryForm.value
    };

    this.categoryService.updateCategory(updatedCategory).subscribe({
      next: () => {
        const index = this.categories.findIndex(c => c.id === updatedCategory.id);
        if (index !== -1) {
          const prev = this.categories[index];
          this.categories[index] = {
            ...prev,
            ...updatedCategory
          };
        }
        this.cancelEdit();
      },
      error: () => this.errorMessage = 'Failed to update category'
    });
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.editingCategoryId = null;
    this.categoryForm.reset({ type: 'Neutral' });
  }

  deleteCategory(id: number): void {
    const category = this.categories.find(c => c.id === id);
    if (!category || category.name === 'WITHOUT CATEGORY') return;

    this.categoryService.deleteCategory(id).subscribe({
      next: () => {
        this.categories = this.categories.filter(c => c.id !== id);
      },
      error: () => {
        this.errorMessage = 'Failed to delete category';
      }
    });
  }

  togglePopover(c: CategoryVm): void {
    if (this.openPopoverId === c.id) {
      this.closePopover();
      return;
    }
    this.openPopoverId = c.id;

    if (c.transactions !== undefined) return;

    c.isLoading = true;
    c.error = null;

    this.categoryService.getTransactionsByCategory(c.id).subscribe({
      next: (tx: Transaction[]) => {
        c.transactions = tx;
        c.isLoading = false;
      },
      error: () => {
        c.error = 'Failed to load transactions';
        c.isLoading = false;
      }
    });
  }

  closePopover(): void {
    this.openPopoverId = null;
  }

  @HostListener('document:keydown.escape')
  onEsc(): void {
    if (this.openPopoverId !== null) this.closePopover();
  }

  goToTransactionEdit(transactionId: number): void {
    this.router.navigate(['/transactions'], { queryParams: { edit: transactionId } });
  }
}
