<script lang="ts">
    import { goto } from "$app/navigation";
    let { path = "/" } = $props();
</script>

<button type="button" class="btn btn-secondary" onclick={() => goto(path)}
    >Cancel</button
>
