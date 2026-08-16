export interface AuthResponse {
  token: string;
  role: string;
  clienteId?: number | null;
  email: string;
  nombreCompleto: string;
}

export interface Cliente {
  clienteId: number;
  nombre: string;
  apellido: string;
  email: string;
  telefono?: string | null;
  direccion: string;
  fechaRegistro: string;
  activo: boolean;
}

export interface Producto {
  productoId: number;
  nombre: string;
  descripcion?: string | null;
  precio: number;
  stock: number;
  activo: boolean;
}

export interface LineaOrden {
  productoId: number;
  cantidad: number;
}

export interface OrdenDetalle {
  ordenDetalleId: number;
  productoId: number;
  producto: string;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
  stockActual: number;
}

export interface Pago {
  pagoId: number;
  monto: number;
  estado: string;
  metodoPago: string;
  referencia?: string | null;
  intentos: number;
  fechaPago?: string | null;
  fechaRegistro: string;
  mensajeError?: string | null;
}

export interface OrdenEstado {
  ordenId: number;
  fechaOrden: string;
  estadoOrden: string;
  total: number;
  clienteId: number;
  clienteNombre: string;
  email: string;
  pagoId?: number | null;
  estadoPago?: string | null;
  metodoPago?: string | null;
  intentosPago?: number | null;
  fechaPago?: string | null;
  mensajeError?: string | null;
  totalProductos: number;
  detalles: OrdenDetalle[];
  pagos: Pago[];
}

export interface CompraResult {
  ordenId: number;
  estadoOrden: string;
  estadoPago?: string | null;
  resultado: string;
}

export interface LogEvento {
  logId: number;
  tablaAfectada: string;
  operacion: string;
  registroId?: string | null;
  mensajeLog?: string | null;
  usuario: string;
  fechaEvento: string;
}

export interface ApiProblem {
  title?: string;
  detail?: string;
  status?: number;
}
