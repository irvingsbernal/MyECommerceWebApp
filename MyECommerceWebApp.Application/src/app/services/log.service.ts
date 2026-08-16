import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { LogEvento } from '../core/models/ecommerce.models';

@Injectable({ providedIn: 'root' })
export class LogService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/logs`;

  listar(operacion?: string): Observable<LogEvento[]> {
    const params = operacion ? `?operacion=${encodeURIComponent(operacion)}` : '';
    return this.http.get<LogEvento[]>(`${this.base}${params}`);
  }

  registrar(payload: {
    tablaAfectada: string;
    operacion: string;
    registroId?: string;
    mensajeLog?: string;
  }): Observable<LogEvento> {
    return this.http.post<LogEvento>(this.base, payload);
  }
}
