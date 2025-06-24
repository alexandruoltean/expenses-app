import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Group, CreateGroupRequest, JoinGroupRequest, GroupMember } from '../models/group.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GroupService {
  private apiUrl = `${environment.apiUrl}/groups`;
  
  private groupsSubject = new BehaviorSubject<Group[]>([]);
  public groups$ = this.groupsSubject.asObservable();
  
  private currentGroupSubject = new BehaviorSubject<Group | null>(null);
  public currentGroup$ = this.currentGroupSubject.asObservable();

  constructor(private http: HttpClient) {
    // Don't load groups on construction - wait for authentication
  }

  // Get all groups for the current user
  getUserGroups(): Observable<Group[]> {
    return this.http.get<Group[]>(this.apiUrl).pipe(
      tap(groups => this.groupsSubject.next(groups))
    );
  }

  // Load groups and update the subject
  loadUserGroups(): void {
    this.getUserGroups().subscribe({
      next: (groups) => {
        // If no current group selected and there are groups, select the first one
        if (!this.currentGroupSubject.value && groups.length > 0) {
          this.setCurrentGroup(groups[0]);
        }
      },
      error: (error) => {
        console.error('Error loading user groups:', error);
      }
    });
  }

  // Get a specific group
  getGroup(id: number): Observable<Group> {
    return this.http.get<Group>(`${this.apiUrl}/${id}`);
  }

  // Create a new group
  createGroup(request: CreateGroupRequest): Observable<Group> {
    return this.http.post<Group>(this.apiUrl, request).pipe(
      tap(newGroup => {
        const currentGroups = this.groupsSubject.value;
        this.groupsSubject.next([...currentGroups, newGroup]);
        // Set as current group if it's the first group
        if (currentGroups.length === 0) {
          this.setCurrentGroup(newGroup);
        }
      })
    );
  }

  // Join an existing group
  joinGroup(request: JoinGroupRequest): Observable<Group> {
    return this.http.post<Group>(`${this.apiUrl}/join`, request).pipe(
      tap(joinedGroup => {
        const currentGroups = this.groupsSubject.value;
        this.groupsSubject.next([...currentGroups, joinedGroup]);
      })
    );
  }

  // Leave a group
  leaveGroup(groupId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${groupId}/leave`).pipe(
      tap(() => {
        const currentGroups = this.groupsSubject.value;
        const updatedGroups = currentGroups.filter(g => g.id !== groupId);
        this.groupsSubject.next(updatedGroups);
        
        // If leaving current group, switch to another group or personal
        if (this.currentGroupSubject.value?.id === groupId) {
          const nextGroup = updatedGroups.length > 0 ? updatedGroups[0] : null;
          this.setCurrentGroup(nextGroup);
        }
      })
    );
  }

  // Get group members
  getGroupMembers(groupId: number): Observable<GroupMember[]> {
    return this.http.get<GroupMember[]>(`${this.apiUrl}/${groupId}/members`);
  }

  // Set current group (null for personal expenses)
  setCurrentGroup(group: Group | null): void {
    this.currentGroupSubject.next(group);
  }

  // Get current group
  getCurrentGroup(): Group | null {
    return this.currentGroupSubject.value;
  }

  // Get current groups array
  getCurrentGroups(): Group[] {
    return this.groupsSubject.value;
  }

  // Check if currently viewing personal expenses
  isPersonalView(): boolean {
    return this.currentGroupSubject.value === null;
  }

  // Get current context display name
  getCurrentContextName(): string {
    const currentGroup = this.getCurrentGroup();
    return currentGroup ? currentGroup.name : 'Personal';
  }
}