<script lang="ts">
    import { onMount } from "svelte";
    import type { Book } from "$lib/types";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";

    let user = $state(null);
    let email = $state("");
    let displayName = $state("");
    let password = $state("");
    let loading = $state(false);
    let error = $state("");

    async function registerUser() {
        //TODO: Implement registration logic
    }
</script>

<div class="container">
    <h1 class="text-center mb-4">Register</h1>

    <form onsubmit={registerUser}>
        <input
            type="email"
            class="form-control"
            placeholder="Email"
            bind:value={email}
            disabled={loading}
        />
        <input
            type="text"
            class="form-control"
            placeholder="Display Name"
            bind:value={displayName}
            disabled={loading}
        />
        <input
            type="password"
            class="form-control"
            placeholder="Password"
            bind:value={password}
            disabled={loading}
        />
        <input
            type="password"
            class="btn btn-primary mt-3"
            value="Register"
            disabled={loading}
    </form>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
