<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    let { path = "/" } = $props();
</script>

<button
    type="button"
    class="btn btn-secondary"
    onclick={() => goto(resolve(path))}>Cancel</button
>
