<script lang="ts">
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";

    let loading = $state(false);
    let error = $state("");
</script>

<div class="container">
    <h1 class="text-center mb-4">Task List Creation Screen (WIP)</h1>
    <button
        type="button"
        class="btn btn-secondary"
        on:click={() => goto(resolve("/home"))}
        >Cancel
    </button>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
