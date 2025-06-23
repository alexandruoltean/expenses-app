export interface Expense {
  id?: number;
  title: string;
  amount: number;
  category: string;
  description?: string;
  date: Date;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface CreateExpenseRequest {
  title: string;
  amount: number;
  category: string;
  description?: string;
  date: Date;
}

export interface UpdateExpenseRequest {
  title?: string;
  amount?: number;
  category?: string;
  description?: string;
  date?: Date;
}