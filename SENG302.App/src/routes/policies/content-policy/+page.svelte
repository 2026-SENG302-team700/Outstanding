<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { page } from "$app/stores";
    import ContentPolicy from "$lib/policies/content-policy.svelte";

    $: from = $page.url.searchParams.get("from") ?? "landing";

    const urls: Record<string, string> = {
        home: "/home",
        register: "/register",
        landing: "/",
    };

    const buttons: Record<string, string> = {
        home: "Back to Home",
        register: "Back to Register",
        landing: "Back",
    };
</script>

<div class="container py-4">
    <ContentPolicy />
    <button
        class="btn btn-primary mt-3"
        onclick={() => goto(resolve((urls[from] ?? "/") as any))}
    >
        {buttons[from]}
    </button>
</div>
