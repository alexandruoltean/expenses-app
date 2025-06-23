import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ExpenseService } from '../../services/expense.service';

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
  styleUrls: ['./add-expense-dialog.component.css']
})
export class AddExpenseDialogComponent implements OnInit {
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

  onSubmit() {
    if (this.expenseForm.invalid) return;
    
    this.loading = true;
    const formValue = this.expenseForm.value;
    
    this.expenseService.createExpense(formValue).subscribe({
      next: () => {
        this.snackBar.open('Expense added successfully!', 'Close', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: (error) => {
        this.snackBar.open('Error adding expense. Please try again.', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }
}