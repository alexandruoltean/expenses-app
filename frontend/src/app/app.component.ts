import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ThemeService } from './services/theme.service';
import { AuthService } from './services/auth.service';
import { AddExpenseDialogComponent } from './components/add-expense-dialog/add-expense-dialog.component';
import { Observable } from 'rxjs';

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
    MatDialogModule,
    MatMenuModule
  ],
  template: `
    <mat-toolbar color="primary" class="sticky-toolbar" *ngIf="isAuthenticated$ | async">
      <span>Expense Tracker</span>
      <button mat-raised-button color="accent" (click)="openAddExpenseDialog()" class="add-expense-btn">
        <mat-icon>add</mat-icon>
        Add Expense
      </button>
      <span class="spacer"></span>
      <button mat-icon-button (click)="toggleTheme()" class="theme-toggle" [attr.aria-label]="isDarkTheme ? 'Switch to light mode' : 'Switch to dark mode'">
        <mat-icon>{{isDarkTheme ? 'light_mode' : 'dark_mode'}}</mat-icon>
      </button>
      <span class="user-info" *ngIf="currentUser$ | async as user">
        {{user.username}}
      </span>
      <button mat-icon-button [matMenuTriggerFor]="userMenu">
        <mat-icon>account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <button mat-menu-item (click)="logout()">
          <mat-icon>logout</mat-icon>
          <span>Logout</span>
        </button>
      </mat-menu>
    </mat-toolbar>
    
    <div class="content-container" [class.no-toolbar]="!(isAuthenticated$ | async)">
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
    
    .user-info {
      margin-right: 8px;
      font-size: 0.875rem;
      color: rgba(255, 255, 255, 0.8);
    }
    
    .add-expense-btn {
      margin-left: 16px;
    }
    
    .theme-toggle {
      margin-right: 8px;
    }
    
    .content-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
      margin-top: 64px; /* Height of toolbar */
    }
    
    .content-container.no-toolbar {
      margin-top: 0;
    }
  `]
})
export class AppComponent implements OnInit {
  title = 'Expense Tracker';
  isDarkTheme = false;
  isAuthenticated$: Observable<boolean>;
  currentUser$: Observable<any>;

  constructor(
    private themeService: ThemeService,
    private authService: AuthService,
    private dialog: MatDialog,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.isAuthenticated$ = this.authService.isAuthenticated$;
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit() {
    this.isDarkTheme = this.themeService.isDarkTheme();
  }

  toggleTheme() {
    this.isDarkTheme = !this.isDarkTheme;
    this.themeService.setTheme(this.isDarkTheme ? 'dark' : 'light');
  }

  logout(): void {
    this.authService.logout();
    this.snackBar.open('Logged out successfully', 'Close', { 
      duration: 3000,
      panelClass: ['success-snackbar']
    });
    this.router.navigate(['/login']);
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