import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Producto } from '../../core/models/ecommerce.models';
import { AuthService } from '../../core/services/auth.service';
import { InventarioService } from '../../services/inventario.service';
import { OrdenService } from '../../services/orden.service';

@Component({
  selector: 'app-checkout',
  imports: [ReactiveFormsModule, CurrencyPipe],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css'
})
export class CheckoutComponent {
  private readonly fb = inject(FormBuilder);
  private readonly inventario = inject(InventarioService);
  private readonly ordenService = inject(OrdenService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly productos = signal<Producto[]>([]);
  readonly cantidades = signal<Record<number, number>>({});
  readonly error = signal<string | null>(null);
  readonly loading = signal(false);

  readonly lineas = computed(() =>
    this.productos()
      .map((producto) => ({
        producto,
        cantidad: this.cantidades()[producto.productoId] ?? 0
      }))
      .filter((linea) => linea.cantidad > 0)
  );

  readonly total = computed(() =>
    this.lineas().reduce((sum, linea) => sum + linea.producto.precio * linea.cantidad, 0)
  );

  readonly pagoForm = this.fb.nonNullable.group({
    metodoPago: ['Tarjeta de credito', Validators.required],
    referencia: ['VISA-4532']
  });

  constructor() {
    this.inventario.listarProductos().subscribe({
      next: (productos) => this.productos.set(productos),
      error: (err: { message?: string }) => this.error.set(err.message ?? 'No se pudo cargar el catálogo.')
    });
  }

  setCantidad(productoId: number, value: string): void {
    const cantidad = Math.max(0, Number(value) || 0);
    this.cantidades.update((actual) => ({ ...actual, [productoId]: cantidad }));
  }

  comprar(): void {
    const clienteId = this.auth.clienteId();
    if (!clienteId) {
      this.error.set('Debes identificarte como cliente.');
      return;
    }

    if (this.lineas().length === 0) {
      this.error.set('Selecciona al menos un producto.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.ordenService
      .comprar({
        clienteId,
        lineas: this.lineas().map((linea) => ({
          productoId: linea.producto.productoId,
          cantidad: linea.cantidad
        })),
        metodoPago: this.pagoForm.controls.metodoPago.value,
        referencia: this.pagoForm.controls.referencia.value
      })
      .subscribe({
        next: (result) => void this.router.navigate(['/ordenes', result.ordenId], {
          state: { resultado: result.resultado }
        }),
        error: (err: { message?: string; status?: number }) => {
          this.loading.set(false);
          if (err.status === 409) {
            this.error.set(
              'Stock insuficiente (posible compra simultánea). Se actualizó el catálogo.'
            );
            this.inventario.listarProductos().subscribe((productos) => this.productos.set(productos));
            return;
          }

          this.error.set(err.message ?? 'No se pudo procesar la compra.');
        }
      });
  }
}
