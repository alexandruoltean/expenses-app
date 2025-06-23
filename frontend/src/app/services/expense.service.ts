import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Expense, CreateExpenseRequest, UpdateExpenseRequest } from '../models/expense.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ExpenseService {
  private apiUrl = `${environment.apiUrl}/expenses`;
  private expensesSubject = new BehaviorSubject<Expense[]>([]);
  public expenses$ = this.expensesSubject.asObservable();

  constructor(private http: HttpClient) {}

  getExpenses(): Observable<Expense[]> {
    return this.http.get<Expense[]>(this.apiUrl).pipe(
      tap(expenses => this.expensesSubject.next(expenses))
    );
  }

  getExpense(id: number): Observable<Expense> {
    return this.http.get<Expense>(`${this.apiUrl}/${id}`);
  }

  createExpense(expense: CreateExpenseRequest): Observable<Expense> {
    return this.http.post<Expense>(this.apiUrl, expense).pipe(
      tap(() => this.refreshExpenses())
    );
  }

  updateExpense(id: number, expense: UpdateExpenseRequest): Observable<Expense> {
    return this.http.put<Expense>(`${this.apiUrl}/${id}`, expense).pipe(
      tap(() => this.refreshExpenses())
    );
  }

  deleteExpense(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.refreshExpenses())
    );
  }

  private refreshExpenses(): void {
    this.http.get<Expense[]>(this.apiUrl).subscribe(
      expenses => this.expensesSubject.next(expenses)
    );
  }

  getExpensesByMonth(year: number, month: number): Observable<Expense[]> {
    return this.http.get<Expense[]>(`${this.apiUrl}/month/${year}/${month}`);
  }

  getExpensesByCategory(): Observable<{ category: string; total: number }[]> {
    return this.http.get<{ category: string; total: number }[]>(`${this.apiUrl}/by-category`);
  }
}