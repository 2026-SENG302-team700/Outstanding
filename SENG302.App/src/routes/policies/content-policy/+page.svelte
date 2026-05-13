<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { page } from "$app/stores";
    import ContentPolicy from "$lib/policies/content-policy.svelte";

    $: from = $page.url.searchParams.get("from") || "/";

</script>

<div class="container py-4">
    <ContentPolicy />
    <button
        class="btn btn-secondary mt-3"
        onclick={() => goto(resolve(from))}
    >
        Back
    </button>
</div>
