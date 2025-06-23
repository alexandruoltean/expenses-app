import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ThemeService } from './services/theme.service';

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
    MatSlideToggleModule
  ],
  template: `
    <mat-toolbar color="primary">
      <span>Expense Tracker</span>
      <span class="spacer"></span>
      <div class="theme-toggle">
        <mat-slide-toggle 
          [checked]="isDarkTheme" 
          (change)="toggleTheme()"
          aria-label="Toggle dark theme">
          Dark Mode
        </mat-slide-toggle>
      </div>
      <button mat-button routerLink="/expenses">
        <mat-icon>list</mat-icon>
        Expenses
      </button>
    </mat-toolbar>
    
    <div class="content-container">
      <router-outlet></router-outlet>
    </div>
  `,
  styles: [`
    .spacer {
      flex: 1 1 auto;
    }
    
    .theme-toggle {
      margin-right: 16px;
    }
    
    .content-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
    }
  `]
})
export class AppComponent implements OnInit {
  title = 'Expense Tracker';
  isDarkTheme = false;

  constructor(private themeService: ThemeService) {}

  ngOnInit() {
    this.isDarkTheme = this.themeService.isDarkTheme();
  }

  toggleTheme() {
    this.isDarkTheme = !this.isDarkTheme;
    this.themeService.setTheme(this.isDarkTheme ? 'dark' : 'light');
  }
}