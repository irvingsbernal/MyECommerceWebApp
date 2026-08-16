import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ClienteService } from '../../services/cliente.service';
import { AuthApiService } from '../../services/auth-api.service';

@Component({
  selector: 'app-registro-cliente',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './registro-cliente.html',
  styleUrl: './registro-cliente.css'
})
export class RegistroClienteComponent {
  private readonly fb = inject(FormBuilder);
  private readonly clienteService = inject(ClienteService);
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);

  readonly modo = signal<'registro' | 'identificar' | 'admin'>('registro');
  readonly error = signal<string | null>(null);
  readonly loading = signal(false);

  readonly registroForm = this.fb.nonNullable.group({
    nombre: ['', [Validators.required, Validators.maxLength(100)]],
    apellido: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    telefono: [''],
    direccion: ['', [Validators.required, Validators.maxLength(300)]]
  });

  readonly identificarForm = this.fb.nonNullable.group({
    email: ['juan.perez@email.com', [Validators.required, Validators.email]]
  });

  readonly adminForm = this.fb.nonNullable.group({
    demoKey: ['demo-admin', Validators.required]
  });

  registrar(): void {
    if (this.registroForm.invalid) {
      this.registroForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.clienteService.registrar(this.registroForm.getRawValue()).subscribe({
      next: () => void this.router.navigate(['/catalogo']),
      error: (err: { message?: string }) => {
        this.error.set(err.message ?? 'No se pudo registrar el cliente.');
        this.loading.set(false);
      }
    });
  }

  identificar(): void {
    if (this.identificarForm.invalid) {
      this.identificarForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.authApi.identificar(this.identificarForm.controls.email.value).subscribe({
      next: () => void this.router.navigate(['/catalogo']),
      error: (err: { message?: string }) => {
        this.error.set(err.message ?? 'Cliente no encontrado.');
        this.loading.set(false);
      }
    });
  }

  admin(): void {
    if (this.adminForm.invalid) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.authApi.admin(this.adminForm.controls.demoKey.value).subscribe({
      next: () => void this.router.navigate(['/bitacora']),
      error: (err: { message?: string }) => {
        this.error.set(err.message ?? 'Clave invalida.');
        this.loading.set(false);
      }
    });
  }
}
