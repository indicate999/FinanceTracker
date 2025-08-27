import {CommonModule} from '@angular/common';
import {Component, HostListener, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {Router, RouterModule} from '@angular/router';
import {Category, CategoryVm, Transaction} from '../../../_models/finance.models';
import {CategoryService} from '../../../_services/category.service';
import {MatSnackBar, MatSnackBarModule} from '@angular/material/snack-bar';


@Component({
  selector: 'app-categories',
  imports: [CommonModule, ReactiveFormsModule, RouterModule, MatSnackBarModule],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss'
})
export class CategoriesComponent implements OnInit {
  categoryForm!: FormGroup;

  categories: CategoryVm[] = [];

  errorMessage: string | null = null;

  editingCategoryId: number | null = null;
  originalCategoryType: string | null = null;
  isEditing: boolean = false;

  openPopoverId: number | null = null;

  categorySort = { column: 'name', direction: 'asc' };

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
  }

  ngOnInit(): void {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(50)]],
      type: ['Neutral', Validators.required]
    });

    this.loadCategories();
  }

  loadCategories(): void {
    this.categoryService.getCategoriesWithCounts(this.categorySort.column, this.categorySort.direction).subscribe({
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

  sortCategories(column: string): void {
    if (this.categorySort.column === column) {
      this.categorySort.direction = this.categorySort.direction === 'asc' ? 'desc' : 'asc';
    } else {
      this.categorySort.column = column;
      this.categorySort.direction = 'asc';
    }
    this.loadCategories();
  }

  addCategory(): void {
    if (this.categoryForm.invalid) return;

    const newCategory: Category = this.categoryForm.value;

    this.categoryService.createCategory(newCategory).subscribe({
      next: (category) => {
        this.loadCategories();
        this.categoryForm.reset({type: 'Neutral'});
      },
      error: () => this.errorMessage = 'Failed to add category'
    });
  }

  startEdit(category: CategoryVm): void {
    if (category.name === 'WITHOUT CATEGORY') return;

    this.isEditing = true;
    this.editingCategoryId = category.id;
    this.originalCategoryType = category.type;
    this.categoryForm.setValue({
      name: category.name,
      type: category.type
    });
  }

  saveEdit(): void {
    if (this.categoryForm.invalid || this.editingCategoryId === null) return;

    const newType = this.categoryForm.value.type;

    const needsConfirmation =
      this.originalCategoryType !== newType &&
      (newType === 'Income' || newType === 'Expense');

    if (needsConfirmation) {
      this.showConfirmationAndSave();
    } else {
      this.performUpdate();
    }
  }

  showConfirmationAndSave(): void {
    const message = 'Incompatible transactions will be moved to the default category.';
    const snackBarRef = this.snackBar.open(message, 'Confirm', {
      duration: 7000,
      panelClass: ['custom-confirmation-snackbar']
    });

    snackBarRef.onAction().subscribe(() => {
      this.performUpdate();
    });
  }

  performUpdate(): void {
    if (this.categoryForm.invalid || this.editingCategoryId === null) return;

    const updatedCategory: Category = {
      id: this.editingCategoryId,
      ...this.categoryForm.value
    };

    this.categoryService.updateCategory(updatedCategory).subscribe({
      next: () => {
        this.loadCategories();
        this.cancelEdit();
      },
      error: () => this.errorMessage = 'Failed to update category'
    });
  }


  cancelEdit(): void {
    this.isEditing = false;
    this.editingCategoryId = null;
    this.originalCategoryType = null;
    this.categoryForm.reset({type: 'Neutral'});
  }

  deleteCategory(id: number): void {
    const category = this.categories.find(c => c.id === id);
    if (!category || category.name === 'WITHOUT CATEGORY') return;

    this.categoryService.deleteCategory(id).subscribe({
      next: () => {
        this.loadCategories();
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
    this.router.navigate(['/transactions'], {queryParams: {edit: transactionId}});
  }
}
