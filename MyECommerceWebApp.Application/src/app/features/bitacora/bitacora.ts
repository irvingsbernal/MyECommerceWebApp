import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { LogEvento } from '../../core/models/ecommerce.models';
import { LogService } from '../../services/log.service';

@Component({
  selector: 'app-bitacora',
  imports: [DatePipe],
  templateUrl: './bitacora.html',
  styleUrl: './bitacora.css'
})
export class BitacoraComponent {
  private readonly logService = inject(LogService);
  readonly logs = signal<LogEvento[]>([]);
  readonly error = signal<string | null>(null);

  constructor() {
    this.cargar();
  }

  cargar(operacion?: string): void {
    this.logService.listar(operacion || undefined).subscribe({
      next: (logs) => this.logs.set(logs),
      error: (err: { message?: string }) => this.error.set(err.message ?? 'No se pudo cargar la bitácora.')
    });
  }
}
