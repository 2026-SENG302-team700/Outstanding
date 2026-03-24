import { writable } from "svelte/store"

export type PasswordForm = {
    error: string;
    passwordErrors?: string;
    passwordErrorIndicator?: string;
    bind: string;
}

export const passwordForms = writable<PasswordForm[]>([]);


export function addPasswordForm(error: string, bind: string, passwordErrorIndicator: string = "", passwordErrors: string = "") {

}