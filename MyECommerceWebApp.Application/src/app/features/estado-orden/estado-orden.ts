import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { OrdenEstado } from '../../core/models/ecommerce.models';
import { AuthService } from '../../core/services/auth.service';
import { OrdenService } from '../../services/orden.service';
import { PagoService } from '../../services/pago.service';
import { InventarioService } from '../../services/inventario.service';

@Component({
  selector: 'app-estado-orden',
  imports: [ReactiveFormsModule, CurrencyPipe, DatePipe],
  templateUrl: './estado-orden.html',
  styleUrl: './estado-orden.css'
})
export class EstadoOrdenComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly ordenService = inject(OrdenService);
  private readonly pagoService = inject(PagoService);
  private readonly inventario = inject(InventarioService);
  readonly auth = inject(AuthService);

  readonly orden = signal<OrdenEstado | null>(null);
  readonly mensaje = signal<string | null>(history.state?.['resultado'] ?? null);
  readonly error = signal<string | null>(null);
  readonly loading = signal(false);

  readonly consultaForm = this.fb.nonNullable.group({
    ordenId: [Number(this.route.snapshot.paramMap.get('id') ?? 1), Validators.required]
  });

  readonly pagoForm = this.fb.nonNullable.group({
    metodoPago: ['Tarjeta de credito', Validators.required],
    referencia: ['VISA-4532']
  });

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.cargar(Number(id));
    }
  }

  consultar(): void {
    this.cargar(this.consultaForm.controls.ordenId.value);
  }

  reintentarPago(): void {
    const orden = this.orden();
    if (!orden) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.pagoService
      .reintentar(orden.ordenId, this.pagoForm.controls.metodoPago.value, this.pagoForm.controls.referencia.value)
      .subscribe({
        next: () => this.cargar(orden.ordenId),
        error: (err: { message?: string }) => {
          this.error.set(err.message ?? 'No se pudo reintentar el pago.');
          this.loading.set(false);
        }
      });
  }

  actualizarInventario(): void {
    const orden = this.orden();
    if (!orden) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.inventario.actualizarPorOrden(orden.ordenId).subscribe({
      next: () => {
        this.mensaje.set('Inventario actualizado.');
        this.cargar(orden.ordenId);
      },
      error: (err: { message?: string; status?: number }) => {
        this.loading.set(false);
        this.error.set(
          err.status === 409
            ? 'Stock insuficiente (posible compra simultánea).'
            : err.message ?? 'No se pudo actualizar el inventario.'
        );
        this.cargar(orden.ordenId);
      }
    });
  }

  private cargar(id: number): void {
    this.loading.set(true);
    this.ordenService.getEstado(id).subscribe({
      next: (orden) => {
        this.orden.set(orden);
        this.loading.set(false);
      },
      error: (err: { message?: string }) => {
        this.error.set(err.message ?? 'Orden no encontrada.');
        this.loading.set(false);
      }
    });
  }
}
