import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('application shell', () => {
  it('renders the product purpose in an accessible main landmark', async () => {
    await TestBed.configureTestingModule({ imports: [App] }).compileComponents();
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('main h1')?.textContent).toBe('Secure Incident Hub');
    expect(element.querySelector('main p')?.textContent).toContain('your organization');
  });
});
