import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GroupManagementDialog } from './group-management-dialog';

describe('GroupManagementDialog', () => {
  let component: GroupManagementDialog;
  let fixture: ComponentFixture<GroupManagementDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GroupManagementDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GroupManagementDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
