import { writable } from "svelte/store";

export type Toast = {
  message: string;
  type: "success" | "error" | "info";
};

export const toasts = writable<Toast[]>([]);

export function addToast(
  message: string,
  type: "success" | "error" | "info" = "info",
) {
  const id = Date.now();
  toasts.update((currentToasts) => [...currentToasts, { message, type }]);
  setTimeout(() => {
    toasts.update((currentToasts) =>
      currentToasts.filter((toast) => toast.message !== message),
    );
  }, 3000);
}
