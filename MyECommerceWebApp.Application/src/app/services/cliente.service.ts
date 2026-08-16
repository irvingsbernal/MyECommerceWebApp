import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { AuthResponse, Cliente } from '../core/models/ecommerce.models';
import { AuthService } from '../core/services/auth.service';

@Injectable({ providedIn: 'root' })
export class ClienteService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly base = `${environment.apiUrl}/api/clientes`;

  registrar(payload: {
    nombre: string;
    apellido: string;
    email: string;
    telefono?: string;
    direccion: string;
  }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(this.base, payload).pipe(tap((auth) => this.auth.setSession(auth)));
  }

  getById(id: number): Observable<Cliente> {
    return this.http.get<Cliente>(`${this.base}/${id}`);
  }
}
