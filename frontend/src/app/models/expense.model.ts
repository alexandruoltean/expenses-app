export interface Expense {
  id?: number;
  title: string;
  amount: number;
  category: string;
  description?: string;
  date: Date;
  createdAt?: Date;
  updatedAt?: Date;
  groupId?: number;
  groupName?: string;
  createdByUsername?: string;
}

export interface CreateExpenseRequest {
  title: string;
  amount: number;
  category: string;
  description?: string;
  date: Date;
  groupId?: number;
}

export interface UpdateExpenseRequest {
  title?: string;
  amount?: number;
  category?: string;
  description?: string;
  date?: Date;
}