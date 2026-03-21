import { writable } from "svelte/store";

export const user = writable<{ 
    displayName: string | null; 
    profilePicture: number;
    pfpUrl: string | null;
}>({
    displayName: null,
    profilePicture: null,
    pfpUrl: null,
});