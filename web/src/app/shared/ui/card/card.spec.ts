import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Card } from './card';

@Component({
  imports: [Card],
  template: `<ui-card><p>Body</p></ui-card>`,
})
class Host {}

describe('Card', () => {
  it('projects its content', () => {
    TestBed.configureTestingModule({ imports: [Host] });
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('ui-card p').textContent).toBe('Body');
  });
});
