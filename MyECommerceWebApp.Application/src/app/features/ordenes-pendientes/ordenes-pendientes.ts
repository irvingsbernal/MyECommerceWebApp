import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { showToast } from '../../core/alerts/toast';
import { OrdenEstado } from '../../core/models/ecommerce.models';
import { OrdenService } from '../../services/orden.service';

@Component({
  selector: 'app-ordenes-pendientes',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './ordenes-pendientes.html',
  styleUrl: './ordenes-pendientes.css'
})
export class OrdenesPendientesComponent {
  private readonly ordenService = inject(OrdenService);

  readonly ordenes = signal<OrdenEstado[]>([]);
  readonly error = signal<string | null>(null);
  readonly autorizandoId = signal<number | null>(null);

  constructor() {
    this.cargar();
  }

  cargar(): void {
    this.error.set(null);
    this.ordenService.listarPendientes().subscribe({
      next: (ordenes) => this.ordenes.set(ordenes),
      error: (err: { message?: string }) =>
        this.error.set(err.message ?? 'No se pudieron cargar las órdenes pendientes.')
    });
  }

  autorizar(orden: OrdenEstado): void {
    this.autorizandoId.set(orden.ordenId);
    this.error.set(null);

    this.ordenService.autorizar(orden.ordenId).subscribe({
      next: () => {
        this.autorizandoId.set(null);
        showToast('success', `Orden #${orden.ordenId} autorizada.`);
        this.cargar();
      },
      error: (err: { message?: string; status?: number }) => {
        this.autorizandoId.set(null);
        if (err.status === 409) {
          showToast('error', 'No hay inventario suficiente para autorizar la orden.');
          this.error.set(err.message ?? 'Stock insuficiente. La orden sigue pendiente.');
          return;
        }

        showToast('error', err.message ?? 'No se pudo autorizar la orden.');
        this.error.set(err.message ?? 'No se pudo autorizar la orden.');
      }
    });
  }
}
