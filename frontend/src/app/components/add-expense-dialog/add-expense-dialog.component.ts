import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepicker, MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ExpenseService } from '../../services/expense.service';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
  selector: 'app-add-expense-dialog',
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
    MatIconModule,
    MatSnackBarModule
  ],
  templateUrl: './add-expense-dialog.component.html',
  styleUrls: ['./add-expense-dialog.component.css'],
  animations: [
    trigger('rotateIcon', [
      state('loading', style({ transform: 'rotate(360deg)' })),
      state('default', style({ transform: 'rotate(0deg)' })),
      transition('default => loading', animate('1s linear')),
      transition('loading => default', animate('0.3s ease-out'))
    ])
  ]
})
export class AddExpenseDialogComponent implements OnInit {
  expenseForm: FormGroup;
  loading = false;
  
  @ViewChild('datePicker') datePicker!: MatDatepicker<Date>;
  
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
    private dialogRef: MatDialogRef<AddExpenseDialogComponent>
  ) {
    this.expenseForm = this.formBuilder.group({
      title: ['', [Validators.required, Validators.maxLength(100)]],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      category: ['', Validators.required],
      date: [new Date(), Validators.required],
      description: ['', Validators.maxLength(500)]
    });
  }

  ngOnInit() {}

  openDatePicker() {
    if (this.datePicker) {
      this.datePicker.open();
    }
  }

  getCategoryIcon(category: string): string {
    const iconMap: { [key: string]: string } = {
      'Food & Dining': 'restaurant',
      'Transportation': 'directions_car',
      'Shopping': 'shopping_cart',
      'Entertainment': 'movie',
      'Bills & Utilities': 'receipt',
      'Healthcare': 'medical_services',
      'Travel': 'flight',
      'Education': 'school',
      'Personal Care': 'spa',
      'Home & Garden': 'home',
      'Other': 'category'
    };
    return iconMap[category] || 'category';
  }

  onSubmit() {
    if (this.expenseForm.invalid) return;
    
    this.loading = true;
    const formValue = this.expenseForm.value;
    
    this.expenseService.createExpense(formValue).subscribe({
      next: () => {
        this.snackBar.open('Expense added successfully!', 'Close', { 
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.dialogRef.close(true);
      },
      error: (error) => {
        this.snackBar.open('Error adding expense. Please try again.', 'Close', { 
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        this.loading = false;
      }
    });
  }
}