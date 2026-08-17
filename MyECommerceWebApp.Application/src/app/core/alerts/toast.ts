import Swal, { SweetAlertIcon } from 'sweetalert2';

export function showToast(icon: SweetAlertIcon, title: string): void {
  void Swal.fire({
    toast: true,
    position: 'top-end',
    icon,
    title,
    timer: 3500,
    showConfirmButton: false
  });
}
