export interface Group {
  id: number;
  name: string;
  description?: string;
  inviteCode: string;
  createdBy: string;
  createdByUsername: string;
  createdAt: Date;
  memberCount: number;
  userRole: string;
}

export interface CreateGroupRequest {
  name: string;
  description?: string;
}

export interface JoinGroupRequest {
  inviteCode: string;
}

export interface GroupMember {
  userId: string;
  username: string;
  email: string;
  role: string;
  joinedAt: Date;
}