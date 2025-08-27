import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Transaction } from '../_models/finance.models';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private baseUrl = '/api/transaction';

  constructor(private http: HttpClient) {}

  getTransactions(sortBy: string = 'date', sortOrder: string = 'desc'): Observable<Transaction[]> {
    let params = new HttpParams()
      .set('sortBy', sortBy)
      .set('sortOrder', sortOrder);

    return this.http.get<Transaction[]>(this.baseUrl, { params });
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
