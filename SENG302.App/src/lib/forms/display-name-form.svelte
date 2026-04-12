<script lang="ts">
    let { displayName = $bindable(""), error = "", loading = false } = $props();
</script>

<div class="mb-3">
    <input
        type="text"
        class="form-control"
        class:is-invalid={error}
        autocomplete="name"
        placeholder={"Display Name *"}
        bind:value={displayName}
        disabled={loading}
    />
    {#if error}
        <div class="invalid-feedback">{error}</div>
    {/if}
</div>
