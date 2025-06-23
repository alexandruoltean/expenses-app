import { bootstrapApplication } from '@angular/platform-browser';
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  standalone: true,
  template: `
    <h1>Simple Angular Test Works!</h1>
    <p>If you see this, Angular is working properly.</p>
  `
})
export class SimpleAppComponent {
  title = 'Simple Test';
}

console.log('Simple bootstrap starting...');

bootstrapApplication(SimpleAppComponent)
  .then(() => {
    console.log('Simple Angular bootstrap SUCCESS!');
  })
  .catch(err => {
    console.error('Simple Angular bootstrap FAILED:', err);
  });