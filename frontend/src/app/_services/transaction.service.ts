import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Transaction } from '../_models/finance.models';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private baseUrl = '/api/transaction';

  constructor(private http: HttpClient) {}

  getTransactions(): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(this.baseUrl);
  }

  createTransaction(tx: Omit<Transaction, 'id'>): Observable<Transaction> {
    return this.http.post<Transaction>(this.baseUrl, tx);
  }

  updateTransaction(tx: Transaction): Observable<Transaction> {
    return this.http.put<Transaction>(`${this.baseUrl}/${tx.id}`, tx);
  }

  deleteTransaction(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
