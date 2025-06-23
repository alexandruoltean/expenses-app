import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/expenses', pathMatch: 'full' },
  { 
    path: 'login', 
    loadComponent: () => import('./components/login/login').then(m => m.LoginComponent)
  },
  { 
    path: 'register', 
    loadComponent: () => import('./components/register/register').then(m => m.RegisterComponent)
  },
  { 
    path: 'expenses', 
    loadComponent: () => import('./components/expense-list/expense-list.component').then(m => m.ExpenseListComponent),
    canActivate: [AuthGuard]
  },
  { path: '**', redirectTo: '/login' }
];