import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ThemeService } from './services/theme.service';
import { ExpenseService } from './services/expense.service';
import { AddExpenseDialogComponent } from './components/add-expense-dialog/add-expense-dialog.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatDialogModule
  ],
  template: `
    <mat-toolbar color="primary" class="sticky-toolbar">
      <span>Expense Tracker</span>
      <span class="spacer"></span>
      <button mat-raised-button color="accent" (click)="openAddExpenseDialog()" class="add-expense-btn">
        <mat-icon>add</mat-icon>
        Add Expense
      </button>
      <div class="theme-toggle">
        <mat-slide-toggle 
          [checked]="isDarkTheme" 
          (change)="toggleTheme()"
          aria-label="Toggle dark theme">
          Dark Mode
        </mat-slide-toggle>
      </div>
    </mat-toolbar>
    
    <div class="content-container">
      <router-outlet></router-outlet>
    </div>
  `,
  styles: [`
    .sticky-toolbar {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      z-index: 1000;
      box-shadow: 0 2px 5px rgba(0,0,0,0.2);
    }
    
    .spacer {
      flex: 1 1 auto;
    }
    
    .add-expense-btn {
      margin-right: 16px;
    }
    
    .theme-toggle {
      margin-right: 16px;
    }
    
    .content-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
      margin-top: 64px; /* Height of toolbar */
    }
  `]
})
export class AppComponent implements OnInit {
  title = 'Expense Tracker';
  isDarkTheme = false;

  constructor(
    private themeService: ThemeService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.isDarkTheme = this.themeService.isDarkTheme();
  }

  toggleTheme() {
    this.isDarkTheme = !this.isDarkTheme;
    this.themeService.setTheme(this.isDarkTheme ? 'dark' : 'light');
  }

  openAddExpenseDialog() {
    const dialogRef = this.dialog.open(AddExpenseDialogComponent, {
      width: '500px',
      disableClose: false
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Expense was added successfully, no need to do anything
        // The expense list will be updated via service events
      }
    });
  }
}