import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { GroupService } from '../../services/group.service';
import { Group, GroupMember, CreateGroupRequest, JoinGroupRequest } from '../../models/group.model';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-group-management-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatTabsModule,
    MatListModule,
    MatIconModule,
    MatCardModule,
    MatDividerModule
  ],
  templateUrl: './group-management-dialog.html',
  styleUrl: './group-management-dialog.scss'
})
export class GroupManagementDialogComponent implements OnInit {
  createGroupForm: FormGroup;
  joinGroupForm: FormGroup;
  groups$: Observable<Group[]>;
  selectedGroupMembers: GroupMember[] = [];
  loading = false;

  private groupService = inject(GroupService);
  private formBuilder = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);
  private dialogRef = inject(MatDialogRef<GroupManagementDialogComponent>);

  constructor() {
    this.groups$ = this.groupService.groups$;
    
    this.createGroupForm = this.formBuilder.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)]
    });

    this.joinGroupForm = this.formBuilder.group({
      inviteCode: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(8)]]
    });
  }

  ngOnInit() {
    this.groupService.loadUserGroups();
  }

  onCreateGroup() {
    if (this.createGroupForm.invalid) return;

    this.loading = true;
    const request: CreateGroupRequest = this.createGroupForm.value;

    this.groupService.createGroup(request).subscribe({
      next: (group) => {
        this.snackBar.open(`Group "${group.name}" created successfully!`, 'Close', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.createGroupForm.reset();
        this.loading = false;
      },
      error: (error) => {
        this.snackBar.open('Error creating group. Please try again.', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        this.loading = false;
      }
    });
  }

  onJoinGroup() {
    if (this.joinGroupForm.invalid) return;

    this.loading = true;
    const request: JoinGroupRequest = this.joinGroupForm.value;

    this.groupService.joinGroup(request).subscribe({
      next: (group) => {
        this.snackBar.open(`Successfully joined "${group.name}"!`, 'Close', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.joinGroupForm.reset();
        this.loading = false;
      },
      error: (error) => {
        this.snackBar.open('Error joining group. Check the invite code and try again.', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        this.loading = false;
      }
    });
  }

  loadGroupMembers(group: Group) {
    this.groupService.getGroupMembers(group.id).subscribe({
      next: (members) => {
        this.selectedGroupMembers = members;
      },
      error: (error) => {
        this.snackBar.open('Error loading group members.', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  leaveGroup(group: Group) {
    if (confirm(`Are you sure you want to leave "${group.name}"?`)) {
      this.groupService.leaveGroup(group.id).subscribe({
        next: () => {
          this.snackBar.open(`Left "${group.name}" successfully.`, 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar']
          });
        },
        error: (error) => {
          this.snackBar.open('Error leaving group. Please try again.', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar']
          });
        }
      });
    }
  }

  switchToGroup(group: Group) {
    this.groupService.setCurrentGroup(group);
    this.snackBar.open(`Switched to "${group.name}"`, 'Close', {
      duration: 2000
    });
    this.dialogRef.close();
  }

  copyInviteCode(inviteCode: string) {
    navigator.clipboard.writeText(inviteCode).then(() => {
      this.snackBar.open('Invite code copied to clipboard!', 'Close', {
        duration: 2000,
        panelClass: ['success-snackbar']
      });
    });
  }

  close() {
    this.dialogRef.close();
  }
}