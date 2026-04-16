<script>
    let {description = $bindable(""), error = "", loading = false } = $props();
</script>


<input
        type="text"
        class="form-control"
        class:is-invalid={error}
        placeholder="Description"
        bind:value={description}
        disabled={loading}
/>
{#if error}
    <div class="invalid-feedback">
        {error}
    </div>
{/if}
