import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private darkThemeClass = 'dark-theme';
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
    const body = document.body;
    
    if (theme === 'dark') {
      body.classList.add(this.darkThemeClass);
    } else {
      body.classList.remove(this.darkThemeClass);
    }
    
    localStorage.setItem(this.storageKey, theme);
  }

  isDarkTheme(): boolean {
    return document.body.classList.contains(this.darkThemeClass);
  }

  toggleTheme(): void {
    const currentTheme = this.isDarkTheme() ? 'light' : 'dark';
    this.setTheme(currentTheme);
  }
}