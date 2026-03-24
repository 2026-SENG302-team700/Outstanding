import { writable } from "svelte/store";

export const user = writable<{
    displayName: string | null;
    profilePicture: number | null;
    pfpData: PfpData | null;
}>({
    displayName: null,
    profilePicture: null,
    pfpData: null,
});