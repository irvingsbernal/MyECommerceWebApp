import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Pago } from '../core/models/ecommerce.models';

@Injectable({ providedIn: 'root' })
export class PagoService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/ordenes`;

  procesar(ordenId: number, metodoPago: string, referencia?: string): Observable<Pago> {
    return this.http.post<Pago>(`${this.base}/${ordenId}/pagos`, { metodoPago, referencia });
  }

  reintentar(ordenId: number, metodoPago: string, referencia?: string): Observable<Pago> {
    return this.http.post<Pago>(`${this.base}/${ordenId}/pagos/reintentar`, { metodoPago, referencia });
  }
}
