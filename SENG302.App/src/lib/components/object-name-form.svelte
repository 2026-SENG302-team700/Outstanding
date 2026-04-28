<script lang="ts">
    let {
        displayName = $bindable(""),
        error = "",
        loading = false,
        type = "user",
    } = $props();
</script>

<div class="mb-3">
    <input
        type="text"
        class="form-control"
        class:is-invalid={error}
        autocomplete={type == "user" ? "name" : ""}
        placeholder={type == "user"
            ? "Display Name *"
            : type == "task"
              ? "Title *"
              : "Name *"}
        bind:value={displayName}
        disabled={loading}
    />
    {#if error}
        <div class="invalid-feedback">{error}</div>
    {/if}
</div>
