<script>
    let {description = $bindable(""), error = "", loading = false } = $props();
</script>

<div class="mb-3">
    <input
            type="text"
            class="form-control"
            class:is-invalid={error}
            placeholder="Description"
            bind:value={description}
            disabled={loading}
    />
    {#if errors.description}
        <div class="invalid-feedback">
            {error}
        </div>
    {/if}
</div>