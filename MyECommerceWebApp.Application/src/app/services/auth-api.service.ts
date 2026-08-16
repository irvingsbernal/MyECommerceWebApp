import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { AuthResponse } from '../core/models/ecommerce.models';
import { AuthService } from '../core/services/auth.service';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly base = `${environment.apiUrl}/api/auth`;

  identificar(email: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.base}/identificar`, { email })
      .pipe(tap((auth) => this.auth.setSession(auth)));
  }

  admin(demoKey: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.base}/admin`, { demoKey })
      .pipe(tap((auth) => this.auth.setSession(auth)));
  }
}
