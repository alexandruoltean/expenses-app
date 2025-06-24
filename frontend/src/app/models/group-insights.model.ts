export interface GroupInsights {
  memberSpending: MemberSpendingOverview;
  categoryInsights: CategoryInsight[];
  rankings: MemberRanking[];
  statistics: GroupStatistics;
}

export interface MemberSpendingOverview {
  totalGroupSpending: number;
  members: MemberSpending[];
}

export interface MemberSpending {
  userId: string;
  username: string;
  email: string;
  totalAmount: number;
  percentage: number;
  expenseCount: number;
  averageExpense: number;
  categoryBreakdown: CategorySpending[];
}

export interface CategorySpending {
  category: string;
  amount: number;
  count: number;
  percentage: number;
}

export interface CategoryInsight {
  category: string;
  totalAmount: number;
  memberBreakdown: MemberCategorySpending[];
  topSpender: string;
  topSpenderAmount: number;
}

export interface MemberCategorySpending {
  userId: string;
  username: string;
  amount: number;
  percentage: number;
  count: number;
}

export interface MemberRanking {
  category: string;
  rankings: RankingEntry[];
}

export interface RankingEntry {
  rank: number;
  userId: string;
  username: string;
  value: number;
  additionalInfo?: string;
}

export interface GroupStatistics {
  averageExpensePerMember: number;
  medianExpenseAmount: number;
  totalExpenses: number;
  activeMembers: number;
  firstExpenseDate?: Date;
  lastExpenseDate?: Date;
  mostActiveCategory: string;
  mostExpensiveCategory: string;
}

export interface SpendingTrend {
  date: Date;
  memberSpending: MemberDailySpending[];
  totalDayAmount: number;
}

export interface MemberDailySpending {
  userId: string;
  username: string;
  amount: number;
  expenseCount: number;
}

export interface TimeRangeInsights {
  timeRange: string;
  startDate: Date;
  endDate: Date;
  memberSpending: MemberSpendingOverview;
  trends: SpendingTrend[];
  statistics: GroupStatistics;
}

export type TimeRange = 'week' | 'month' | 'all';