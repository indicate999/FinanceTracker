import {CommonModule} from '@angular/common';
import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {RouterModule} from '@angular/router';
import {Category, Transaction} from '../../../_models/finance.models';
import {TransactionService} from '../../../_services/transaction.service';
import {CategoryService} from '../../../_services/category.service';

@Component({
  selector: 'app-transactions',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.scss'
})
export class TransactionsComponent implements OnInit {
  transactionForm!: FormGroup;
  transactions: Transaction[] = [];
  categories: Category[] = [];
  isEditing = false;
  editingId: number | null = null;
  errorMessage = '';

  constructor(private fb: FormBuilder, private transactionService: TransactionService, private categoryService: CategoryService) {
  }

  ngOnInit(): void {
    this.transactionForm = this.fb.group({
      amount: [0, [Validators.required, Validators.min(0.01)]],
      categoryId: [null, Validators.required],
      type: ['Expense', Validators.required],
      date: [new Date().toISOString().substring(0, 10), Validators.required]
    });

    this.transactionForm.get('type')?.valueChanges.subscribe((newType: string) => {
      const selectedCategoryId = this.transactionForm.get('categoryId')?.value;
      const selectedCategory = this.categories.find(c => c.id === selectedCategoryId);

      if (selectedCategory && selectedCategory.type !== newType && selectedCategory.type !== 'Neutral') {
        this.transactionForm.patchValue({categoryId: null});
      }
    });

    this.loadTransactions();
    this.loadCategories();
  }

  loadTransactions(): void {
    this.transactionService.getTransactions().subscribe({
      next: (data) => this.transactions = data,
      error: () => this.errorMessage = 'Failed to load transactions'
    });
  }

  loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (data) => this.categories = data,
      error: () => this.errorMessage = 'Failed to load categories'
    });
  }

  addTransaction(): void {
    if (this.transactionForm.invalid) return;
    const newTransaction = this.transactionForm.value;
    this.transactionService.createTransaction(newTransaction).subscribe({
      next: (created) => {
        this.transactions.push(created);
        this.transactionForm.reset({type: 'Expense', date: new Date().toISOString().substring(0, 10)});
      },
      error: () => this.errorMessage = 'Failed to add transaction'
    });
  }

  startEdit(t: Transaction): void {
    this.isEditing = true;
    this.editingId = t.id;

    const formattedDate = t.date.substring(0, 10);

    this.transactionForm.setValue({
      amount: t.amount,
      categoryId: t.categoryId,
      type: t.type,
      date: formattedDate
    });
  }

  saveEdit(): void {
    if (this.transactionForm.invalid || this.editingId === null) return;
    const updated = {id: this.editingId, ...this.transactionForm.value};

    const category = this.categories.find(c => c.id === updated.categoryId);
    const categoryName = category ? category.name : '';

    this.transactionService.updateTransaction(updated).subscribe({
      next: (updatedTransaction) => {
        const i = this.transactions.findIndex(t => t.id === this.editingId);
        if (i !== -1) {
          this.transactions[i] = updatedTransaction;
        }
        this.cancelEdit();
      },
      error: () => this.errorMessage = 'Failed to update transaction'
    });
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.editingId = null;
    this.transactionForm.reset({type: 'Expense', date: new Date().toISOString().substring(0, 10)});
  }

  deleteTransaction(id: number): void {
    this.transactionService.deleteTransaction(id).subscribe({
      next: () => this.transactions = this.transactions.filter(t => t.id !== id),
      error: () => this.errorMessage = 'Failed to delete transaction'
    });
  }

  get filteredCategories(): Category[] {
    const type = this.transactionForm.get('type')?.value;
    return this.categories.filter(c =>
      c.type === type || c.type === 'Neutral'
    );
  }
}
