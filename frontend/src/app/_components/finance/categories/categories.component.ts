import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Category } from '../../../_models/finance.models';
import { CategoryService } from '../../../_services/category.service';

@Component({
  selector: 'app-categories',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss'
})
export class CategoriesComponent implements OnInit{
  categoryForm!: FormGroup;
  categories: Category[] = [];
  errorMessage: string | null = null;

  editingCategoryId: number | null = null;
  isEditing: boolean = false;

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService
  ) {}

  ngOnInit(): void {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(50)]],
      type: ['Neutral', Validators.required]
    });

    this.loadCategories();
  }

  loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (data) => this.categories = data,
      error: (err) => this.errorMessage = 'Failed to load categories'
    });
  }

  addCategory(): void {
    if (this.categoryForm.invalid) return;

    const newCategory: Category = this.categoryForm.value;

    this.categoryService.createCategory(newCategory).subscribe({
      next: (category) => {
        this.categories.push(category);
        this.categoryForm.reset({ type: 'Neutral' });
      },
      error: (err) => this.errorMessage = 'Failed to add category'
    });
  }

  startEdit(category: Category): void {
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
        if (index !== -1) this.categories[index] = updatedCategory;

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
}
