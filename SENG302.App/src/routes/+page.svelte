<script lang="ts">
    import { onMount } from "svelte";
    import type { Book } from "$lib/types";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";

    let loading = $state(false);
    let error = $state("");
</script>

<div class="container">
    <h1 class="text-center mb-4">Welcome to OUTSTANDING</h1>
    <button
        class="btn btn-primary w-100 mb-3"
        on:click={() => goto(resolve("/register"))}
    >
        Register
    </button>
    <button
        class="btn btn-primary w-100"
        on:click={() => goto(resolve("/login"))}
    >
        Login
    </button>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
