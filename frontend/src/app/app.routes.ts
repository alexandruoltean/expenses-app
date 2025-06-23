import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/expenses', pathMatch: 'full' },
  { 
    path: 'expenses', 
    loadComponent: () => import('./components/expense-list/expense-list.component').then(m => m.ExpenseListComponent)
  },
  { path: '**', redirectTo: '/expenses' }
];