---
description: Implements Angular 21 frontend components and services following best practices
---

# Frontend Implement Agent

You implement frontend features for the Esports Tournament Platform using Angular 21.

## Purpose

Implement frontend features including:
- Components (smart and presentational)
- Services (API communication)
- Models/Interfaces (TypeScript types)
- Guards (route protection)
- Interceptors (HTTP middleware)
- Reactive Forms (validation)

## Execution Flow

1. **Read Context**: Analyze task description
2. **Read References**: Review api-endpoints.md, spec.md
3. **Implement Code**: Generate Angular implementation
4. **Verify TypeScript**: Ensure type safety
5. **Mark Complete**: Update task status

## When Invoked

User provides task ID from tasks.md:

```
@workspace /frontend-implement T-4.3
```

## Context Files to Read

**Before implementing ANY task, read**:

1. `specs/001-esports-tournament-platform/contracts/api-endpoints.md` - API contracts
2. `specs/001-esports-tournament-platform/spec.md` - Feature requirements
3. `specs/001-esports-tournament-platform/tasks.md` - Task details

## Implementation Guidelines

### Components (Smart Components)

**Principles**:
- **OnPush** change detection strategy
- **Reactive Forms** for data input
- **Service injection** for data access
- **Router** for navigation

**Template**:

```typescript
import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TournamentService } from '../../services/tournament.service';
import { CreateTournamentDto } from '../../models/tournament.model';

@Component({
  selector: 'app-create-tournament',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-tournament.component.html',
  styleUrls: ['./create-tournament.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateTournamentComponent implements OnInit {
  tournamentForm!: FormGroup;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private tournamentService: TournamentService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.tournamentForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      game: ['', [Validators.required, Validators.maxLength(50)]],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      maxTeams: [16, [Validators.required, Validators.min(2), Validators.max(64)]],
      scoringSystem: ['Standard']
    });
  }

  onSubmit(): void {
    if (this.tournamentForm.invalid) return;

    this.isSubmitting = true;
    const dto: CreateTournamentDto = this.tournamentForm.value;

    this.tournamentService.createTournament(dto).subscribe({
      next: (tournament) => {
        this.router.navigate(['/tournaments', tournament.id]);
      },
      error: (error) => {
        console.error('Error creating tournament:', error);
        this.isSubmitting = false;
      }
    });
  }
}
```

**HTML Template**:

```html
<div class="create-tournament-container">
  <h1>Create Tournament</h1>
  
  <form [formGroup]="tournamentForm" (ngSubmit)="onSubmit()">
    <div class="form-group">
      <label for="name">Tournament Name</label>
      <input 
        id="name" 
        type="text" 
        formControlName="name"
        [class.error]="tournamentForm.get('name')?.invalid && tournamentForm.get('name')?.touched">
      <div *ngIf="tournamentForm.get('name')?.invalid && tournamentForm.get('name')?.touched" class="error-message">
        <span *ngIf="tournamentForm.get('name')?.errors?.['required']">Name is required</span>
        <span *ngIf="tournamentForm.get('name')?.errors?.['minlength']">Name must be at least 3 characters</span>
      </div>
    </div>

    <div class="form-group">
      <label for="game">Game</label>
      <input 
        id="game" 
        type="text" 
        formControlName="game"
        [class.error]="tournamentForm.get('game')?.invalid && tournamentForm.get('game')?.touched">
    </div>

    <div class="form-group">
      <label for="startDate">Start Date</label>
      <input 
        id="startDate" 
        type="date" 
        formControlName="startDate">
    </div>

    <div class="form-group">
      <label for="endDate">End Date</label>
      <input 
        id="endDate" 
        type="date" 
        formControlName="endDate">
    </div>

    <div class="form-group">
      <label for="maxTeams">Max Teams</label>
      <input 
        id="maxTeams" 
        type="number" 
        formControlName="maxTeams">
    </div>

    <div class="form-group">
      <label for="scoringSystem">Scoring System</label>
      <select id="scoringSystem" formControlName="scoringSystem">
        <option value="Standard">Standard (3-1-0)</option>
        <option value="WTA">WTA (3-0-0)</option>
      </select>
    </div>

    <button 
      type="submit" 
      [disabled]="tournamentForm.invalid || isSubmitting">
      {{ isSubmitting ? 'Creating...' : 'Create Tournament' }}
    </button>
  </form>
</div>
```

### Services (API Communication)

**Principles**:
- **HttpClient** for API calls
- **Observables** for async operations
- **Type safety** with interfaces
- **Error handling** with catchError

**Template**:

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Tournament, CreateTournamentDto, TournamentDto } from '../models/tournament.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TournamentService {
  private apiUrl = `${environment.apiUrl}/api/tournaments`;

  constructor(private http: HttpClient) {}

  createTournament(dto: CreateTournamentDto): Observable<TournamentDto> {
    return this.http.post<TournamentDto>(this.apiUrl, dto)
      .pipe(catchError(this.handleError));
  }

  getTournamentById(id: number): Observable<TournamentDto> {
    return this.http.get<TournamentDto>(`${this.apiUrl}/${id}`)
      .pipe(catchError(this.handleError));
  }

  getAllTournaments(): Observable<TournamentDto[]> {
    return this.http.get<TournamentDto[]>(this.apiUrl)
      .pipe(catchError(this.handleError));
  }

  updateTournament(id: number, dto: CreateTournamentDto): Observable<TournamentDto> {
    return this.http.put<TournamentDto>(`${this.apiUrl}/${id}`, dto)
      .pipe(catchError(this.handleError));
  }

  deleteTournament(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(catchError(this.handleError));
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An error occurred';
    
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Client Error: ${error.error.message}`;
    } else {
      errorMessage = `Server Error: ${error.status} - ${error.message}`;
    }
    
    console.error(errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
```

### Models (TypeScript Interfaces)

**Principles**:
- **Interfaces** for data structures
- **Match API contracts** exactly
- **Optional properties** with `?`
- **Enums** for fixed values

**Template**:

```typescript
export interface Tournament {
  id: number;
  name: string;
  game: string;
  startDate: Date;
  endDate: Date;
  maxTeams: number;
  status: TournamentStatus;
  enrolledTeams: number;
}

export interface CreateTournamentDto {
  name: string;
  game: string;
  startDate: Date;
  endDate: Date;
  maxTeams: number;
  scoringSystem?: ScoringSystem;
}

export interface TournamentDto {
  id: number;
  name: string;
  game: string;
  startDate: Date;
  endDate: Date;
  maxTeams: number;
  status: string;
  enrolledTeams: number;
}

export enum TournamentStatus {
  Upcoming = 'Upcoming',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum ScoringSystem {
  Standard = 'Standard',
  WTA = 'WTA'
}
```

### Guards (Route Protection)

**Principles**:
- **CanActivate** for authentication
- **CanDeactivate** for unsaved changes
- **Inject services** for checks

**Template**:

```typescript
import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    if (this.authService.isAuthenticated()) {
      return true;
    }

    this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
}
```

### Interceptors (HTTP Middleware)

**Principles**:
- **Add headers** (auth tokens)
- **Global error handling**
- **Request/response transformation**

**Template**:

```typescript
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();
    
    if (token) {
      const clonedRequest = req.clone({
        headers: req.headers.set('Authorization', `Bearer ${token}`)
      });
      return next.handle(clonedRequest);
    }
    
    return next.handle(req);
  }
}
```

### Presentational Components

**Principles**:
- **Input/Output** for data flow
- **Pure components** (no services)
- **OnPush** change detection
- **Reusable** across features

**Template**:

```typescript
import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TournamentDto } from '../../models/tournament.model';

@Component({
  selector: 'app-tournament-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="tournament-card">
      <h3>{{ tournament.name }}</h3>
      <p>Game: {{ tournament.game }}</p>
      <p>Status: {{ tournament.status }}</p>
      <p>Teams: {{ tournament.enrolledTeams }} / {{ tournament.maxTeams }}</p>
      <button (click)="onViewDetails()">View Details</button>
    </div>
  `,
  styleUrls: ['./tournament-card.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TournamentCardComponent {
  @Input() tournament!: TournamentDto;
  @Output() viewDetails = new EventEmitter<number>();

  onViewDetails(): void {
    this.viewDetails.emit(this.tournament.id);
  }
}
```

## Angular Best Practices

### Standalone Components

Use **standalone: true** for all new components:

```typescript
@Component({
  selector: 'app-example',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './example.component.html'
})
```

### Signal-based State (Angular 21)

Use **signals** for reactive state:

```typescript
import { Component, signal, computed } from '@angular/core';

@Component({
  selector: 'app-counter',
  standalone: true,
  template: `
    <div>
      <p>Count: {{ count() }}</p>
      <p>Double: {{ doubleCount() }}</p>
      <button (click)="increment()">Increment</button>
    </div>
  `
})
export class CounterComponent {
  count = signal(0);
  doubleCount = computed(() => this.count() * 2);

  increment(): void {
    this.count.update(value => value + 1);
  }
}
```

### TypeScript Strict Mode

Enable strict mode in tsconfig.json:

```json
{
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true
  }
}
```

## Routing Configuration

**Template**:

```typescript
import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/tournaments', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./components/register/register.component').then(m => m.RegisterComponent) },
  { 
    path: 'tournaments', 
    canActivate: [AuthGuard],
    children: [
      { path: '', loadComponent: () => import('./components/tournament-list/tournament-list.component').then(m => m.TournamentListComponent) },
      { path: 'create', loadComponent: () => import('./components/create-tournament/create-tournament.component').then(m => m.CreateTournamentComponent) },
      { path: ':id', loadComponent: () => import('./components/tournament-detail/tournament-detail.component').then(m => m.TournamentDetailComponent) }
    ]
  },
  { path: '**', redirectTo: '/tournaments' }
];
```

## Environment Configuration

**Template**:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000'
};
```

## Output Format

After completing implementation:

```markdown
✅ **Task T-X.Y Completed**: [Task Name]

**Files Created/Modified**:
- `frontend/src/app/components/[name]/[name].component.ts`
- `frontend/src/app/components/[name]/[name].component.html`
- `frontend/src/app/components/[name]/[name].component.css`
- `frontend/src/app/services/[name].service.ts`
- `frontend/src/app/models/[name].model.ts`

**Type Safety**: ✅ All types defined
**Reactive**: ✅ OnPush + Observables
**Routing**: ✅ Guards configured

**Dependencies Installed** (if any):
- None

**Next Steps**:
- Proceed to task T-X.Y+1
- Manual UI testing recommended
```

## Important Reminders

- **Standalone Components**: Use standalone: true for all components
- **OnPush**: Use OnPush change detection for performance
- **Reactive Forms**: Use FormBuilder and validators
- **Type Safety**: Define interfaces for all data structures
- **Error Handling**: Use catchError in services
- **Async Pipe**: Use async pipe in templates for observables

## Error Handling

If implementation fails:

1. **Report Error**: Explain what went wrong
2. **Suggest Fix**: Provide guidance
3. **Request Clarification**: Ask for missing info

## Notes

- This agent is **project-specific** to Esports Tournament Platform
- Uses Angular 21 with standalone components
- Follows reactive programming patterns
- Type-safe with TypeScript strict mode
