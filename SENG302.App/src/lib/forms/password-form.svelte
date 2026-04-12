<script lang="ts">
    let {
        password = $bindable(""),
        error = "",
        loading = false,
        passConfirm = false,
    } = $props();
</script>

<div class="mb-3">
    <input
        type="password"
        class="form-control"
        class:error
        class:is-invalid={error}
        autocomplete="new-password"
        placeholder={!passConfirm ? "Password *" : "Confirm Password"}
        bind:value={password}
        disabled={loading}
    />
    {#if error}
        <div class="invalid-feedback">{error}</div>
    {/if}
</div>
