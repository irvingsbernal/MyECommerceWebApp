import { Injectable, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { AuthResponse } from '../models/ecommerce.models';

const TOKEN_KEY = 'ecommerce.token';
const ROLE_KEY = 'ecommerce.role';
const CLIENTE_KEY = 'ecommerce.clienteId';
const NAME_KEY = 'ecommerce.nombre';
const EMAIL_KEY = 'ecommerce.email';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly router = inject(Router);

  readonly token = signal<string | null>(null);
  readonly role = signal<string | null>(null);
  readonly clienteId = signal<number | null>(null);
  readonly nombre = signal<string | null>(null);
  readonly email = signal<string | null>(null);

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      this.token.set(localStorage.getItem(TOKEN_KEY));
      this.role.set(localStorage.getItem(ROLE_KEY));
      const cliente = localStorage.getItem(CLIENTE_KEY);
      this.clienteId.set(cliente ? Number(cliente) : null);
      this.nombre.set(localStorage.getItem(NAME_KEY));
      this.email.set(localStorage.getItem(EMAIL_KEY));
    }
  }

  get isCliente(): boolean {
    return this.role() === 'cliente' && !!this.token();
  }

  get isAdmin(): boolean {
    return this.role() === 'admin' && !!this.token();
  }

  setSession(auth: AuthResponse): void {
    this.token.set(auth.token);
    this.role.set(auth.role);
    this.clienteId.set(auth.clienteId ?? null);
    this.nombre.set(auth.nombreCompleto);
    this.email.set(auth.email);

    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(TOKEN_KEY, auth.token);
      localStorage.setItem(ROLE_KEY, auth.role);
      localStorage.setItem(NAME_KEY, auth.nombreCompleto);
      localStorage.setItem(EMAIL_KEY, auth.email);
      if (auth.clienteId) {
        localStorage.setItem(CLIENTE_KEY, String(auth.clienteId));
      } else {
        localStorage.removeItem(CLIENTE_KEY);
      }
    }
  }

  logout(): void {
    this.token.set(null);
    this.role.set(null);
    this.clienteId.set(null);
    this.nombre.set(null);
    this.email.set(null);

    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(ROLE_KEY);
      localStorage.removeItem(CLIENTE_KEY);
      localStorage.removeItem(NAME_KEY);
      localStorage.removeItem(EMAIL_KEY);
    }

    void this.router.navigate(['/registro']);
  }
}
