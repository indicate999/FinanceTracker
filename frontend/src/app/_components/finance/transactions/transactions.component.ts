import {CommonModule} from '@angular/common';
import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {ActivatedRoute, Router, RouterModule} from '@angular/router';
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

  private pendingEditId: number | null = null;

  transactionSort = { column: 'date', direction: 'desc' };

  constructor(
    private fb: FormBuilder,
    private transactionService: TransactionService,
    private categoryService: CategoryService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

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
        this.transactionForm.patchValue({ categoryId: null });
      }
    });

    this.route.queryParams.subscribe(p => {
      const id = p['edit'] ? +p['edit'] : null;
      if (id && id > 0) {
        this.pendingEditId = id;
        this.tryActivatePending();
      }
    });

    this.loadCategories();
    this.loadTransactions();
  }

  loadTransactions(): void {
    this.transactionService.getTransactions(this.transactionSort.column, this.transactionSort.direction).subscribe({
      next: (data) => {
        this.transactions = data;
        this.tryActivatePending();
      },
      error: () => this.errorMessage = 'Failed to load transactions'
    });
  }

  loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (data) => this.categories = data,
      error: () => this.errorMessage = 'Failed to load categories'
    });
  }

  sortTransactions(column: string): void {
    if (this.transactionSort.column === column) {
      this.transactionSort.direction = this.transactionSort.direction === 'asc' ? 'desc' : 'asc';
    } else {
      this.transactionSort.column = column;
      this.transactionSort.direction = 'asc';
    }
    this.loadTransactions();
  }

  addTransaction(): void {
    if (this.transactionForm.invalid) return;
    const newTransaction = this.transactionForm.value;

    this.transactionService.createTransaction(newTransaction).subscribe({
      next: () => {
        this.loadTransactions()
        this.transactionForm.reset({ type: 'Expense', date: new Date().toISOString().substring(0, 10) });
      },
      error: () => this.errorMessage = 'Failed to add transaction'
    });
  }

  startEdit(t: Transaction | number): void {
    const tx = typeof t === 'number' ? this.transactions.find(x => x.id === t) : t;
    if (!tx) return;

    this.isEditing = true;
    this.editingId = tx.id;

    const formattedDate = tx.date.substring(0, 10);
    this.transactionForm.setValue({
      amount: tx.amount,
      categoryId: tx.categoryId,
      type: tx.type,
      date: formattedDate
    });

    this.scrollToRow(tx.id);
  }

  saveEdit(): void {
    if (this.transactionForm.invalid || this.editingId === null) return;
    const updated = { id: this.editingId, ...this.transactionForm.value };

    this.transactionService.updateTransaction(updated).subscribe({
      next: (updatedTransaction) => {
        const i = this.transactions.findIndex(t => t.id === this.editingId);
        if (i !== -1) this.transactions[i] = updatedTransaction;
        this.cancelEdit();
      },
      error: () => this.errorMessage = 'Failed to update transaction'
    });
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.editingId = null;
    this.transactionForm.reset({ type: 'Expense', date: new Date().toISOString().substring(0, 10) });
  }

  deleteTransaction(id: number): void {
    this.transactionService.deleteTransaction(id).subscribe({
      next: () => this.transactions = this.transactions.filter(t => t.id !== id),
      error: () => this.errorMessage = 'Failed to delete transaction'
    });
  }

  get filteredCategories(): Category[] {
    const type = this.transactionForm.get('type')?.value;
    return this.categories.filter(c => c.type === type || c.type === 'Neutral');
  }

  private tryActivatePending(): void {
    if (!this.pendingEditId) return;
    const id = this.pendingEditId;
    const exists = this.transactions.some(t => t.id === id);
    if (exists) {
      this.startEdit(id);
      this.clearEditParam();
      this.pendingEditId = null;
    }
  }

  private clearEditParam(): void {
    this.router.navigate([], {
      queryParams: { edit: null },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  private scrollToRow(id: number): void {
    setTimeout(() => {
      const el = document.querySelector<HTMLElement>(`[data-tx-id="${id}"]`);
      el?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }, 50);
  }

  trackById = (index: number, t: Transaction) => t.id;
}
