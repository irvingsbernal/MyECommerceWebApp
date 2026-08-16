import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { InventarioService } from '../../services/inventario.service';
import { Producto } from '../../core/models/ecommerce.models';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-catalogo',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './catalogo.html',
  styleUrl: './catalogo.css'
})
export class CatalogoComponent {
  private readonly inventario = inject(InventarioService);
  readonly auth = inject(AuthService);
  readonly productos = signal<Producto[]>([]);
  readonly error = signal<string | null>(null);

  constructor() {
    this.inventario.listarProductos().subscribe({
      next: (productos) => this.productos.set(productos),
      error: (err: { message?: string }) => this.error.set(err.message ?? 'No se pudo cargar el catálogo.')
    });
  }
}
