/**
 * A simple object for toast notifications (small notifications that pop up in top right of browser for 3 seconds)
 */
import { writable } from "svelte/store";

export type ToastType = "success" | "error" | "info";

export type Toast = {
  message: string;
  type?: ToastType;
};

export const toasts = writable<Toast[]>([]);

export function addToast(message: string, type: ToastType = "success") {
  toasts.update((currentToasts) => [...currentToasts, { message, type }]);
  setTimeout(() => {
    toasts.update((currentToasts) =>
      currentToasts.filter((toast) => toast.message !== message),
    );
  }, 3000);
}
