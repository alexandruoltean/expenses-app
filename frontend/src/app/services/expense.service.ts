import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, combineLatest } from 'rxjs';
import { tap, switchMap } from 'rxjs/operators';
import { Expense, CreateExpenseRequest, UpdateExpenseRequest } from '../models/expense.model';
import { environment } from '../../environments/environment';
import { GroupService } from './group.service';

@Injectable({
  providedIn: 'root'
})
export class ExpenseService {
  private apiUrl = `${environment.apiUrl}/expenses`;
  private expensesSubject = new BehaviorSubject<Expense[]>([]);
  public expenses$ = this.expensesSubject.asObservable();

  constructor(
    private http: HttpClient,
    private groupService: GroupService
  ) {
    // Auto-load expenses when current group changes
    this.groupService.currentGroup$.subscribe(group => {
      this.loadExpenses(group?.id);
    });
  }

  getExpenses(groupId?: number): Observable<Expense[]> {
    let params = new HttpParams();
    if (groupId !== undefined) {
      params = params.set('groupId', groupId.toString());
    }
    
    return this.http.get<Expense[]>(this.apiUrl, { params }).pipe(
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

  // Load expenses for specific context (group or personal)
  loadExpenses(groupId?: number): void {
    this.getExpenses(groupId).subscribe();
  }

  private refreshExpenses(): void {
    const currentGroup = this.groupService.getCurrentGroup();
    this.loadExpenses(currentGroup?.id);
  }

  getExpensesByMonth(year: number, month: number): Observable<Expense[]> {
    return this.http.get<Expense[]>(`${this.apiUrl}/month/${year}/${month}`);
  }

  getExpensesByCategory(): Observable<{ category: string; total: number }[]> {
    return this.http.get<{ category: string; total: number }[]>(`${this.apiUrl}/by-category`);
  }
}