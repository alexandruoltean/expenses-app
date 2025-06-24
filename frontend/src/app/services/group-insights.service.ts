import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  GroupInsights, 
  MemberSpendingOverview, 
  CategoryInsight, 
  MemberRanking, 
  SpendingTrend, 
  TimeRangeInsights,
  TimeRange
} from '../models/group-insights.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GroupInsightsService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getGroupInsights(groupId: number): Observable<GroupInsights> {
    return this.http.get<GroupInsights>(`${this.apiUrl}/groups/${groupId}/insights`);
  }

  getMemberSpendingOverview(groupId: number): Observable<MemberSpendingOverview> {
    return this.http.get<MemberSpendingOverview>(`${this.apiUrl}/groups/${groupId}/insights/members`);
  }

  getCategoryInsights(groupId: number): Observable<CategoryInsight[]> {
    return this.http.get<CategoryInsight[]>(`${this.apiUrl}/groups/${groupId}/insights/categories`);
  }

  getMemberRankings(groupId: number): Observable<MemberRanking[]> {
    return this.http.get<MemberRanking[]>(`${this.apiUrl}/groups/${groupId}/insights/rankings`);
  }

  getSpendingTrends(groupId: number, startDate?: Date, endDate?: Date): Observable<SpendingTrend[]> {
    let url = `${this.apiUrl}/groups/${groupId}/insights/trends`;
    const params = new URLSearchParams();
    
    if (startDate) {
      params.append('startDate', startDate.toISOString());
    }
    if (endDate) {
      params.append('endDate', endDate.toISOString());
    }
    
    if (params.toString()) {
      url += `?${params.toString()}`;
    }
    
    return this.http.get<SpendingTrend[]>(url);
  }

  getTimeRangeInsights(groupId: number, timeRange: TimeRange): Observable<TimeRangeInsights> {
    return this.http.get<TimeRangeInsights>(`${this.apiUrl}/groups/${groupId}/insights/timerange/${timeRange}`);
  }
}