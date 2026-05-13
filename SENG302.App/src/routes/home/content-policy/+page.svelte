<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import ContentPolicy from "\$lib/policies/content-policy.svelte";
</script>

<div class="container py-4">
    <ContentPolicy/>
    <button
            class="btn btn-primary mt-3"
            on:click={() => goto(resolve("/home"))}
    >
        Back to Home
    </button>
</div>

