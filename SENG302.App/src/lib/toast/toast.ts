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

/**
 * Add a new toast notification to the store and automatically remove after 5 seconds
 * @param message the text to display within the toast
 * @param type the type of toast (success, error, info). This changes the background colour of the toast
 */
export function addToast(message: string, type: ToastType = "success") {
  toasts.update((currentToasts) => [...currentToasts, { message, type }]);
  // display the toast for 5 seconds
  setTimeout(() => {
    toasts.update((currentToasts) =>
      currentToasts.filter((toast) => toast.message !== message),
    );
  }, 5000);
}
