import { Routes } from '@angular/router';
import { adminGuard, clienteGuard } from './core/guards/auth.guards';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'catalogo' },
  {
    path: 'registro',
    loadComponent: () =>
      import('./features/registro-cliente/registro-cliente').then((m) => m.RegistroClienteComponent)
  },
  {
    path: 'catalogo',
    loadComponent: () => import('./features/catalogo/catalogo').then((m) => m.CatalogoComponent)
  },
  {
    path: 'checkout',
    canActivate: [clienteGuard],
    loadComponent: () => import('./features/checkout/checkout').then((m) => m.CheckoutComponent)
  },
  {
    path: 'ordenes',
    loadComponent: () =>
      import('./features/estado-orden/estado-orden').then((m) => m.EstadoOrdenComponent)
  },
  {
    path: 'ordenes/:id',
    loadComponent: () =>
      import('./features/estado-orden/estado-orden').then((m) => m.EstadoOrdenComponent)
  },
  {
    path: 'pendientes',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./features/ordenes-pendientes/ordenes-pendientes').then(
        (m) => m.OrdenesPendientesComponent
      )
  },
  {
    path: 'bitacora',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/bitacora/bitacora').then((m) => m.BitacoraComponent)
  },
  { path: '**', redirectTo: 'catalogo' }
];
