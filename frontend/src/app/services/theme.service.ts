import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private storageKey = 'theme-preference';

  constructor() {
    this.initializeTheme();
  }

  private initializeTheme(): void {
    const savedTheme = localStorage.getItem(this.storageKey);
    const isDarkTheme = savedTheme === 'dark' || 
      (!savedTheme && window.matchMedia('(prefers-color-scheme: dark)').matches);
    
    this.setTheme(isDarkTheme ? 'dark' : 'light');
  }

  setTheme(theme: 'light' | 'dark'): void {
    const htmlElement = document.documentElement;
    
    if (theme === 'dark') {
      htmlElement.setAttribute('data-theme', 'dark');
    } else {
      htmlElement.removeAttribute('data-theme');
    }
    
    localStorage.setItem(this.storageKey, theme);
  }

  isDarkTheme(): boolean {
    return document.documentElement.getAttribute('data-theme') === 'dark';
  }

  toggleTheme(): void {
    const currentTheme = this.isDarkTheme() ? 'light' : 'dark';
    this.setTheme(currentTheme);
  }
}