import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GroupInsights } from './group-insights';

describe('GroupInsights', () => {
  let component: GroupInsights;
  let fixture: ComponentFixture<GroupInsights>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GroupInsights]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GroupInsights);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
