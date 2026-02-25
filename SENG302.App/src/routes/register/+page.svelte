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
    <h1 class="text-center mb-4">Register</h1>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
