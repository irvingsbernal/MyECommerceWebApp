import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Producto } from '../core/models/ecommerce.models';

@Injectable({ providedIn: 'root' })
export class InventarioService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api`;

  listarProductos(): Observable<Producto[]> {
    return this.http.get<Producto[]>(`${this.base}/productos`);
  }

  actualizarPorOrden(ordenId: number): Observable<void> {
    return this.http.post<void>(`${this.base}/ordenes/${ordenId}/inventario`, {});
  }
}
