import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CompraResult, LineaOrden, OrdenEstado } from '../core/models/ecommerce.models';

@Injectable({ providedIn: 'root' })
export class OrdenService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api`;

  getEstado(ordenId: number): Observable<OrdenEstado> {
    return this.http.get<OrdenEstado>(`${this.base}/ordenes/${ordenId}`);
  }

  listarPendientes(): Observable<OrdenEstado[]> {
    return this.http.get<OrdenEstado[]>(`${this.base}/ordenes`, { params: { estado: 'pendiente' } });
  }

  autorizar(id: number): Observable<OrdenEstado> {
    return this.http.post<OrdenEstado>(`${this.base}/ordenes/${id}/autorizar`, {});
  }

  crear(clienteId: number, lineas: LineaOrden[], observaciones?: string): Observable<OrdenEstado> {
    return this.http.post<OrdenEstado>(`${this.base}/ordenes`, { clienteId, lineas, observaciones });
  }

  comprar(payload: {
    clienteId: number;
    lineas: LineaOrden[];
    metodoPago: string;
    referencia?: string;
    observaciones?: string;
  }): Observable<CompraResult> {
    return this.http.post<CompraResult>(`${this.base}/compras`, payload);
  }
}
