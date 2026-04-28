<script>
    let {name = $bindable(""), error = "", loading = false } = $props();
</script>

<div class="mb-3">
    <input
            type="text"
            class="form-control"
            class:is-invalid={error}
            placeholder="Title *"
            bind:value={name}
            disabled={loading}
    />
    {#if error}
        <div class="invalid-feedback">
            {error}
        </div>
    {/if}
</div>