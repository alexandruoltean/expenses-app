import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { FormControl, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { Expense, CreateExpenseRequest } from '../../models/expense.model';
import { ExpenseService } from '../../services/expense.service';
import { GroupService } from '../../services/group.service';
import { Group } from '../../models/group.model';
import { GroupInsightsComponent } from '../group-insights/group-insights.component';
import { BaseChartDirective } from 'ng2-charts';
import { 
  Chart,
  ChartConfiguration, 
  ChartOptions, 
  ChartType,
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Title,
  Tooltip,
  Legend,
  Filler
} from 'chart.js';

@Component({
  selector: 'app-expense-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatDialogModule,
    MatChipsModule,
    MatTooltipModule,
    MatTabsModule,
    GroupInsightsComponent,
    BaseChartDirective
  ],
  templateUrl: './expense-list.component.html',
  styleUrls: ['./expense-list.component.css']
})
export class ExpenseListComponent implements OnInit {
  expenses: Expense[] = [];
  filteredExpenses: Expense[] = [];
  categories: string[] = [];
  loading = false;
  
  // Group context properties
  currentGroup: Group | null = null;
  isPersonalView = true;
  contextName = 'Personal';
  
  categoryFilter = new FormControl([]);
  monthFilter = new FormControl();

  // Chart.js properties
  public lineChartType = 'line' as const;
  public lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Daily Expenses',
        fill: true,
        tension: 0.4,
        borderColor: '#667eea',
        backgroundColor: 'rgba(102, 126, 234, 0.1)',
        pointBackgroundColor: '#667eea',
        pointBorderColor: '#ffffff',
        pointBorderWidth: 2,
        pointRadius: 4,
        pointHoverRadius: 6,
        pointHoverBackgroundColor: '#5a6fd8',
        pointHoverBorderColor: '#ffffff',
        pointHoverBorderWidth: 3,
      }
    ]
  };

  public lineChartOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      },
      tooltip: {
        enabled: true,
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        titleColor: '#ffffff',
        bodyColor: '#ffffff',
        borderColor: '#667eea',
        borderWidth: 1,
        cornerRadius: 8,
        displayColors: false,
        callbacks: {
          title: (context) => {
            return context[0].label || '';
          },
          label: (context) => {
            return `$${Number(context.parsed.y).toFixed(2)}`;
          }
        }
      }
    },
    scales: {
      x: {
        display: true,
        grid: {
          color: 'rgba(0, 0, 0, 0.05)',
          lineWidth: 1
        },
        ticks: {
          color: '#666',
          font: {
            size: 11
          }
        }
      },
      y: {
        display: true,
        grid: {
          color: 'rgba(0, 0, 0, 0.05)',
          lineWidth: 1
        },
        ticks: {
          color: '#666',
          font: {
            size: 11
          },
          callback: function(value) {
            return '$' + Number(value).toFixed(0);
          }
        },
        beginAtZero: true
      }
    },
    interaction: {
      intersect: false,
      mode: 'index'
    },
    elements: {
      point: {
        hoverRadius: 8
      }
    }
  };

  constructor(
    private expenseService: ExpenseService,
    private groupService: GroupService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    // Register Chart.js components
    Chart.register(
      LineController,
      LineElement,
      PointElement,
      LinearScale,
      CategoryScale,
      Title,
      Tooltip,
      Legend,
      Filler
    );
  }

  ngOnInit() {
    this.loadExpenses();
    this.setupFilters();
    this.subscribeToExpenses();
    this.subscribeToGroupChanges();
  }

  private setupFilters() {
    // Filter setup can be implemented here if needed
  }

  private subscribeToExpenses() {
    this.expenseService.expenses$.subscribe({
      next: (expenses) => {
        this.expenses = expenses;
        this.filteredExpenses = expenses;
        this.categories = [...new Set(expenses.map(e => e.category))];
        this.updateChartData();
      },
      error: (error) => {
        console.error('Error receiving expenses:', error);
      }
    });
  }

  private subscribeToGroupChanges() {
    this.groupService.currentGroup$.subscribe(group => {
      this.currentGroup = group;
      this.isPersonalView = group === null;
      this.contextName = group ? group.name : 'Personal';
    });
  }

  private loadExpenses() {
    this.loading = true;
    this.expenseService.getExpenses().subscribe({
      next: (expenses) => {
        this.expenses = expenses;
        this.filteredExpenses = expenses;
        this.categories = [...new Set(expenses.map(e => e.category))];
        this.updateChartData();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading expenses:', error);
        this.snackBar.open('Error loading expenses', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  private updateChartData() {
    const dailyData = this.getDailyExpenses();
    
    if (dailyData.length > 0) {
      this.lineChartData = {
        labels: dailyData.map(day => day.date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })),
        datasets: [
          {
            data: dailyData.map(day => day.amount),
            label: 'Daily Expenses',
            fill: true,
            tension: 0.4,
            borderColor: '#667eea',
            backgroundColor: 'rgba(102, 126, 234, 0.1)',
            pointBackgroundColor: '#667eea',
            pointBorderColor: '#ffffff',
            pointBorderWidth: 2,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointHoverBackgroundColor: '#5a6fd8',
            pointHoverBorderColor: '#ffffff',
            pointHoverBorderWidth: 3,
          }
        ]
      };
    }
  }

  private applyFilters() {
    this.filteredExpenses = this.expenses;
  }

  deleteExpense(id: number) {
    if (confirm('Are you sure you want to delete this expense?')) {
      this.expenseService.deleteExpense(id).subscribe({
        next: () => {
          this.snackBar.open('Expense deleted successfully', 'Close', { duration: 3000 });
        },
        error: (error) => {
          this.snackBar.open('Error deleting expense', 'Close', { duration: 3000 });
        }
      });
    }
  }

  getTotalAmount(): number {
    return this.filteredExpenses.reduce((total, expense) => total + expense.amount, 0);
  }

  getApiUrl(): string {
    return this.expenseService['apiUrl'] || 'Unknown';
  }

  getAverageAmount(): number {
    if (this.filteredExpenses.length === 0) return 0;
    return this.getTotalAmount() / this.filteredExpenses.length;
  }

  getTopCategory(): string {
    if (this.filteredExpenses.length === 0) return 'None';
    
    const categoryTotals = this.filteredExpenses.reduce((acc, expense) => {
      acc[expense.category] = (acc[expense.category] || 0) + expense.amount;
      return acc;
    }, {} as {[key: string]: number});

    return Object.entries(categoryTotals)
      .sort(([,a], [,b]) => b - a)[0][0];
  }

  getCategoryBreakdown(): {name: string, amount: number, percentage: number}[] {
    if (this.filteredExpenses.length === 0) return [];

    const categoryTotals = this.filteredExpenses.reduce((acc, expense) => {
      acc[expense.category] = (acc[expense.category] || 0) + expense.amount;
      return acc;
    }, {} as {[key: string]: number});

    const total = this.getTotalAmount();
    
    return Object.entries(categoryTotals)
      .map(([name, amount]) => ({
        name,
        amount,
        percentage: (amount / total) * 100
      }))
      .sort((a, b) => b.amount - a.amount);
  }

  getRecentExpenses(): Expense[] {
    return [...this.filteredExpenses]
      .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
      .slice(0, 5);
  }

  getCategoryIcon(category: string): string {
    const iconMap: {[key: string]: string} = {
      'Food & Dining': 'restaurant',
      'Transportation': 'directions_car',
      'Shopping': 'shopping_cart',
      'Bills & Utilities': 'receipt_long',
      'Entertainment': 'movie',
      'Healthcare': 'local_hospital',
      'Travel': 'flight',
      'Education': 'school',
      'Personal Care': 'spa',
      'Other': 'category'
    };
    return iconMap[category] || 'category';
  }

  getDailyExpenses(): {date: Date, amount: number, percentage: number}[] {
    // Get last 7 days
    const days: {date: Date, amount: number, percentage: number}[] = [];
    const today = new Date();
    
    for (let i = 6; i >= 0; i--) {
      const date = new Date(today);
      date.setDate(today.getDate() - i);
      date.setHours(0, 0, 0, 0);
      
      const dayTotal = this.filteredExpenses
        .filter(expense => {
          const expenseDate = new Date(expense.date);
          expenseDate.setHours(0, 0, 0, 0);
          return expenseDate.getTime() === date.getTime();
        })
        .reduce((total, expense) => total + expense.amount, 0);
      
      days.push({
        date: date,
        amount: dayTotal,
        percentage: 0
      });
    }
    
    // Calculate percentages based on highest day
    const maxAmount = Math.max(...days.map(d => d.amount));
    if (maxAmount > 0) {
      days.forEach(day => {
        day.percentage = (day.amount / maxAmount) * 100;
      });
    }
    
    return days;
  }

  getYAxisLabels(): number[] {
    const days = this.getDailyExpenses();
    const maxAmount = Math.max(...days.map(d => d.amount));
    
    if (maxAmount === 0) return [0, 10, 20, 30, 40, 50];
    
    // Create nice round numbers for Y axis
    const roundedMax = Math.ceil(maxAmount / 10) * 10;
    const step = roundedMax / 5;
    
    return [0, step, step * 2, step * 3, step * 4, roundedMax].reverse();
  }

  getLinePoints(): string {
    const days = this.getDailyExpenses();
    const maxAmount = Math.max(...days.map(d => d.amount), 50); // minimum scale of 50
    
    return days.map((day, index) => {
      const x = (index / (days.length - 1)) * 400;
      const y = 200 - ((day.amount / maxAmount) * 180); // 180 to leave margin
      return `${x},${y}`;
    }).join(' ');
  }

  getPointX(index: number): number {
    const days = this.getDailyExpenses();
    return (index / (days.length - 1)) * 400;
  }

  getPointY(amount: number): number {
    const days = this.getDailyExpenses();
    const maxAmount = Math.max(...days.map(d => d.amount), 50);
    return 200 - ((amount / maxAmount) * 180);
  }

  getWeekTotal(): number {
    return this.getDailyExpenses().reduce((total, day) => total + day.amount, 0);
  }

  getTrendDirection(): number {
    const days = this.getDailyExpenses();
    if (days.length < 2) return 0;
    
    const firstHalf = days.slice(0, Math.ceil(days.length / 2));
    const secondHalf = days.slice(Math.floor(days.length / 2));
    
    const firstAvg = firstHalf.reduce((sum, day) => sum + day.amount, 0) / firstHalf.length;
    const secondAvg = secondHalf.reduce((sum, day) => sum + day.amount, 0) / secondHalf.length;
    
    if (firstAvg === 0) return secondAvg > 0 ? 100 : 0;
    return ((secondAvg - firstAvg) / firstAvg) * 100;
  }

  getMiniLinePoints(): string {
    const days = this.getDailyExpenses();
    const maxAmount = Math.max(...days.map(d => d.amount), 10);
    
    return days.map((day, index) => {
      const x = (index / (days.length - 1)) * 200;
      const y = 80 - ((day.amount / maxAmount) * 60);
      return `${x},${y}`;
    }).join(' ');
  }

  getMiniPointX(index: number): number {
    const days = this.getDailyExpenses();
    return (index / (days.length - 1)) * 200;
  }

  getMiniPointY(amount: number): number {
    const days = this.getDailyExpenses();
    const maxAmount = Math.max(...days.map(d => d.amount), 10);
    return 80 - ((amount / maxAmount) * 60);
  }

  getAreaPath(): string {
    const days = this.getDailyExpenses();
    const maxAmount = Math.max(...days.map(d => d.amount), 10);
    
    let path = `M 0,80`;
    
    days.forEach((day, index) => {
      const x = (index / (days.length - 1)) * 200;
      const y = 80 - ((day.amount / maxAmount) * 60);
      if (index === 0) {
        path += ` L ${x},${y}`;
      } else {
        path += ` L ${x},${y}`;
      }
    });
    
    path += ` L 200,80 Z`;
    return path;
  }

  // New Dashboard Methods
  getTodaySpending(): number {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    return this.filteredExpenses
      .filter(expense => {
        const expenseDate = new Date(expense.date);
        expenseDate.setHours(0, 0, 0, 0);
        return expenseDate.getTime() === today.getTime();
      })
      .reduce((total, expense) => total + expense.amount, 0);
  }

  getCurrentMonthTotal(): number {
    const now = new Date();
    const currentMonth = now.getMonth();
    const currentYear = now.getFullYear();
    
    return this.filteredExpenses
      .filter(expense => {
        const expenseDate = new Date(expense.date);
        return expenseDate.getMonth() === currentMonth && expenseDate.getFullYear() === currentYear;
      })
      .reduce((total, expense) => total + expense.amount, 0);
  }

  getLastMonthTotal(): number {
    const now = new Date();
    const lastMonth = now.getMonth() === 0 ? 11 : now.getMonth() - 1;
    const lastMonthYear = now.getMonth() === 0 ? now.getFullYear() - 1 : now.getFullYear();
    
    return this.filteredExpenses
      .filter(expense => {
        const expenseDate = new Date(expense.date);
        return expenseDate.getMonth() === lastMonth && expenseDate.getFullYear() === lastMonthYear;
      })
      .reduce((total, expense) => total + expense.amount, 0);
  }

  getMonthlyGrowth(): number {
    const current = this.getCurrentMonthTotal();
    const last = this.getLastMonthTotal();
    
    if (last === 0) return current > 0 ? 100 : 0;
    return ((current - last) / last) * 100;
  }

  getCurrentMonthPercentage(): number {
    const current = this.getCurrentMonthTotal();
    const last = this.getLastMonthTotal();
    const max = Math.max(current, last, 100);
    return (current / max) * 100;
  }

  getLastMonthPercentage(): number {
    const current = this.getCurrentMonthTotal();
    const last = this.getLastMonthTotal();
    const max = Math.max(current, last, 100);
    return (last / max) * 100;
  }

  getHighestDayAmount(): number {
    const days = this.getDailyExpenses();
    return Math.max(...days.map(d => d.amount));
  }

  getMostEfficientDay(): string {
    const days = this.getDailyExpenses();
    const minDay = days.reduce((min, day) => day.amount < min.amount ? day : min, days[0]);
    return minDay.date.toLocaleDateString('en-US', { weekday: 'long' });
  }

  getHighestSpendingDay(): {day: string, amount: number} {
    const days = this.getDailyExpenses();
    const maxDay = days.reduce((max, day) => day.amount > max.amount ? day : max, days[0]);
    return {
      day: maxDay.date.toLocaleDateString('en-US', { weekday: 'long' }),
      amount: maxDay.amount
    };
  }


  copyInviteCode() {
    if (this.currentGroup?.inviteCode) {
      navigator.clipboard.writeText(this.currentGroup.inviteCode).then(() => {
        this.snackBar.open('Invite code copied to clipboard!', 'Close', {
          duration: 2000,
          panelClass: ['success-snackbar']
        });
      });
    }
  }

  openEditExpenseDialog(expense: Expense): void {
    const dialogRef = this.dialog.open(EditExpenseDialogComponent, {
      width: '650px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      disableClose: false,
      panelClass: 'custom-dialog-container',
      hasBackdrop: true,
      backdropClass: 'custom-backdrop',
      data: expense
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // List will be automatically updated via reactive service
      }
    });
  }
}


// Edit Dialog Component
@Component({
  selector: 'app-edit-expense-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule
  ],
  template: `
    <div class="dialog-header">
      <h2 mat-dialog-title><mat-icon>edit</mat-icon> Edit Expense</h2>
      <button mat-icon-button mat-dialog-close>
        <mat-icon>close</mat-icon>
      </button>
    </div>

    <mat-dialog-content>
      <form [formGroup]="expenseForm" class="expense-form">
        <mat-form-field appearance="outline">
          <mat-label>Title</mat-label>
          <input matInput formControlName="title" placeholder="Enter expense title">
          <mat-error *ngIf="expenseForm.get('title')?.hasError('required')">
            Title is required
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Amount ($)</mat-label>
          <input matInput type="number" formControlName="amount" placeholder="0.00" step="0.01">
          <mat-error *ngIf="expenseForm.get('amount')?.hasError('required')">
            Amount is required
          </mat-error>
          <mat-error *ngIf="expenseForm.get('amount')?.hasError('min')">
            Amount must be greater than 0
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Category</mat-label>
          <mat-select formControlName="category">
            <mat-option *ngFor="let category of categories" [value]="category">
              {{category}}
            </mat-option>
          </mat-select>
          <mat-error *ngIf="expenseForm.get('category')?.hasError('required')">
            Category is required
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Date</mat-label>
          <input matInput [matDatepicker]="picker" formControlName="date" readonly>
          <mat-datepicker-toggle matSuffix [for]="picker"></mat-datepicker-toggle>
          <mat-datepicker #picker></mat-datepicker>
          <mat-error *ngIf="expenseForm.get('date')?.hasError('required')">
            Date is required
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Description (Optional)</mat-label>
          <textarea matInput formControlName="description" placeholder="Enter description" rows="3"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" 
              [disabled]="expenseForm.invalid || loading" 
              (click)="onSubmit()">
        <mat-icon *ngIf="loading">hourglass_empty</mat-icon>
        <mat-icon *ngIf="!loading">save</mat-icon>
        {{loading ? 'Updating...' : 'Update Expense'}}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    :host {
      display: block;
    }

    .dialog-header {
      position: relative;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      margin: -24px -24px 24px -24px;
      padding: 32px 32px 24px 32px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.15);
    }

    .dialog-header h2 {
      margin: 0;
      padding: 0 50px 0 0;
      font-size: 22px;
      font-weight: 600;
      color: white;
      display: flex;
      align-items: center;
      gap: 12px;
      line-height: 1;
    }

    .dialog-header h2 mat-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
    }

    .dialog-header button {
      position: absolute;
      top: 30px;
      right: 24px;
      width: 36px;
      height: 36px;
      padding: 0;
      border: none;
      border-radius: 50%;
      background: rgba(255,255,255,0.1);
      color: white;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: background-color 0.2s ease;
      transform: translateX(-10px);
    }

    .dialog-header button:hover {
      background: rgba(255,255,255,0.2);
    }

    .dialog-header button mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
    }

    mat-dialog-content {
      padding: 0 32px 24px 32px;
      max-height: 60vh;
      overflow-y: auto;
    }

    .expense-form {
      display: flex;
      flex-direction: column;
      gap: 20px;
      min-width: 500px;
    }

    .expense-form mat-form-field {
      width: 100%;
    }

    .expense-form .mat-mdc-form-field {
      margin-bottom: 4px;
    }

    .expense-form .mat-mdc-form-field-subscript-wrapper {
      padding: 0 16px;
    }

    mat-dialog-actions {
      padding: 20px 32px 24px 32px;
      margin: 0;
      border-top: 1px solid #f0f0f0;
      background: #fafafa;
      gap: 12px;
    }

    mat-dialog-actions button {
      min-width: 100px;
      height: 44px;
      font-weight: 600;
      border-radius: 8px;
    }

    mat-dialog-actions .mat-mdc-button {
      color: #666;
      border: 1px solid #e0e0e0;
    }

    mat-dialog-actions .mat-mdc-raised-button {
      box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
    }

    /* Custom styling for form fields */
    ::ng-deep .expense-form .mat-mdc-form-field-outline {
      border-radius: 8px;
    }

    ::ng-deep .expense-form .mat-mdc-form-field-focus-overlay {
      border-radius: 8px;
    }

    ::ng-deep .expense-form .mdc-text-field--outlined .mdc-notched-outline .mdc-notched-outline__leading {
      border-radius: 8px 0 0 8px;
    }

    ::ng-deep .expense-form .mdc-text-field--outlined .mdc-notched-outline .mdc-notched-outline__trailing {
      border-radius: 0 8px 8px 0;
    }

    /* Dialog container customization */
    ::ng-deep .custom-dialog-container .mat-mdc-dialog-container {
      border-radius: 16px;
      box-shadow: 0 24px 38px 3px rgba(0,0,0,0.14), 
                  0 9px 46px 8px rgba(0,0,0,0.12), 
                  0 11px 15px -7px rgba(0,0,0,0.2);
      overflow: hidden;
      padding: 0;
    }

    ::ng-deep .custom-backdrop {
      background: rgba(0,0,0,0.6);
      backdrop-filter: blur(4px);
    }

    /* Responsive design */
    @media (max-width: 600px) {
      .expense-form {
        min-width: 300px;
      }

      .dialog-header {
        margin: -24px -24px 20px -24px;
        padding: 20px 24px;
      }

      .dialog-header h2 {
        font-size: 20px;
      }

      mat-dialog-content {
        padding: 0 24px 20px 24px;
      }

      mat-dialog-actions {
        padding: 16px 24px 20px 24px;
      }
    }
  `]
})
export class EditExpenseDialogComponent implements OnInit {
  expenseForm: FormGroup;
  loading = false;
  
  categories = [
    'Food & Dining',
    'Transportation',
    'Shopping',
    'Entertainment',
    'Bills & Utilities',
    'Healthcare',
    'Travel',
    'Education',
    'Personal Care',
    'Home & Garden',
    'Other'
  ];

  constructor(
    private formBuilder: FormBuilder,
    private expenseService: ExpenseService,
    private snackBar: MatSnackBar,
    private dialogRef: MatDialogRef<EditExpenseDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Expense
  ) {
    this.expenseForm = this.formBuilder.group({
      title: [data.title, [Validators.required, Validators.maxLength(100)]],
      amount: [data.amount, [Validators.required, Validators.min(0.01)]],
      category: [data.category, Validators.required],
      date: [new Date(data.date), Validators.required],
      description: [data.description || '', Validators.maxLength(500)]
    });
  }

  ngOnInit() {
    console.log('EditExpenseDialogComponent initialized with data:', this.data);
  }

  onSubmit() {
    if (this.expenseForm.invalid || !this.data.id) return;
    
    this.loading = true;
    const formValue = this.expenseForm.value;
    
    this.expenseService.updateExpense(this.data.id, formValue).subscribe({
      next: () => {
        this.snackBar.open('Expense updated successfully!', 'Close', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: (error) => {
        console.error('Error updating expense:', error);
        this.snackBar.open('Error updating expense. Please try again.', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }
}