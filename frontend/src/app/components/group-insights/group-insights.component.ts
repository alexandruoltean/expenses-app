import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { BaseChartDirective } from 'ng2-charts';
import { 
  Chart,
  ChartConfiguration, 
  ChartOptions, 
  ChartType,
  DoughnutController,
  ArcElement,
  BarController,
  BarElement,
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Title,
  Tooltip,
  Legend
} from 'chart.js';
import { GroupInsightsService } from '../../services/group-insights.service';
import { Group } from '../../models/group.model';
import { 
  GroupInsights, 
  MemberSpendingOverview, 
  CategoryInsight, 
  MemberRanking 
} from '../../models/group-insights.model';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-group-insights',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    BaseChartDirective
  ],
  templateUrl: './group-insights.component.html',
  styleUrls: ['./group-insights.component.scss']
})
export class GroupInsightsComponent implements OnInit, OnDestroy {
  @Input() currentGroup: Group | null = null;
  
  loading = false;
  memberSpending: MemberSpendingOverview | null = null;
  categoryInsights: CategoryInsight[] = [];
  rankings: MemberRanking[] = [];
  
  private destroy$ = new Subject<void>();

  // Chart configurations
  public memberSpendingChartType = 'doughnut' as const;
  public memberSpendingChartData: ChartConfiguration<'doughnut'>['data'] = {
    labels: [],
    datasets: [{
      data: [],
      backgroundColor: [
        '#667eea',
        '#764ba2', 
        '#f093fb',
        '#f5576c',
        '#4facfe',
        '#00f2fe',
        '#43e97b',
        '#38f9d7'
      ],
      borderWidth: 2,
      borderColor: '#ffffff',
      hoverBorderWidth: 3
    }]
  };

  public memberSpendingChartOptions: ChartOptions<'doughnut'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          padding: 20,
          usePointStyle: true,
          font: {
            size: 12
          }
        }
      },
      tooltip: {
        callbacks: {
          label: (context) => {
            const label = context.label || '';
            const value = context.parsed || 0;
            const total = (context.dataset.data as number[]).reduce((a, b) => a + b, 0);
            const percentage = ((value / total) * 100).toFixed(1);
            return `${label}: $${value.toFixed(2)} (${percentage}%)`;
          }
        }
      }
    }
  };

  // Category chart
  public categoryChartType = 'bar' as const;
  public categoryChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: []
  };

  public categoryChartOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top'
      },
      tooltip: {
        mode: 'index',
        intersect: false
      }
    },
    scales: {
      x: {
        display: true,
        title: {
          display: true,
          text: 'Categories'
        }
      },
      y: {
        display: true,
        title: {
          display: true,
          text: 'Amount ($)'
        },
        beginAtZero: true
      }
    }
  };

  constructor(private groupInsightsService: GroupInsightsService) {
    // Register Chart.js components
    Chart.register(
      DoughnutController,
      ArcElement,
      BarController,
      BarElement,
      LineController,
      LineElement,
      PointElement,
      LinearScale,
      CategoryScale,
      Title,
      Tooltip,
      Legend
    );
  }

  ngOnInit() {
    if (this.currentGroup) {
      this.loadInsights();
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadInsights() {
    if (!this.currentGroup) return;

    this.loading = true;
    
    // Load member spending overview
    this.groupInsightsService.getMemberSpendingOverview(this.currentGroup.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.memberSpending = data;
          this.updateMemberSpendingChart();
        },
        error: (error) => {
          console.error('Error loading member spending:', error);
        }
      });

    // Load category insights
    this.groupInsightsService.getCategoryInsights(this.currentGroup.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.categoryInsights = data;
          this.updateCategoryChart();
        },
        error: (error) => {
          console.error('Error loading category insights:', error);
        }
      });

    // Load rankings
    this.groupInsightsService.getMemberRankings(this.currentGroup.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.rankings = data;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading rankings:', error);
          this.loading = false;
        }
      });
  }

  private updateMemberSpendingChart() {
    if (!this.memberSpending) return;

    this.memberSpendingChartData = {
      labels: this.memberSpending.members.map(m => m.username),
      datasets: [{
        data: this.memberSpending.members.map(m => m.totalAmount),
        backgroundColor: [
          '#667eea',
          '#764ba2', 
          '#f093fb',
          '#f5576c',
          '#4facfe',
          '#00f2fe',
          '#43e97b',
          '#38f9d7'
        ],
        borderWidth: 2,
        borderColor: '#ffffff',
        hoverBorderWidth: 3
      }]
    };
  }

  private updateCategoryChart() {
    if (!this.categoryInsights.length) return;

    const categories = this.categoryInsights.map(c => c.category);
    const users = Array.from(new Set(
      this.categoryInsights.flatMap(c => c.memberBreakdown.map(m => m.username))
    ));

    const datasets = users.map((username, index) => ({
      label: username,
      data: categories.map(category => {
        const categoryData = this.categoryInsights.find(c => c.category === category);
        const memberData = categoryData?.memberBreakdown.find(m => m.username === username);
        return memberData?.amount || 0;
      }),
      backgroundColor: [
        '#667eea',
        '#764ba2', 
        '#f093fb',
        '#f5576c',
        '#4facfe',
        '#00f2fe',
        '#43e97b',
        '#38f9d7'
      ][index % 8],
      borderWidth: 1
    }));

    this.categoryChartData = {
      labels: categories,
      datasets: datasets
    };
  }

  getRankingIcon(category: string): string {
    switch (category) {
      case 'Total Spending': return 'account_balance_wallet';
      case 'Average Expense': return 'trending_up';
      case 'Most Active': return 'flash_on';
      default: return 'star';
    }
  }

  getRankingColor(rank: number): string {
    switch (rank) {
      case 1: return 'gold';
      case 2: return 'silver';
      case 3: return 'bronze';
      default: return 'default';
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  refreshInsights() {
    this.loadInsights();
  }
}