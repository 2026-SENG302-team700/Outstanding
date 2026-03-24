<script lang="ts">
    let password = $state("");
    let errors = $state({ password: "", passwordErrorIndicator: false });
    let error = $state("");
    let loading = $state(false);
</script>

<div class="mb-3">
    <input
        type="password"
        class="form-control"
        class:error={errors.password}
        class:is-invalid={errors.password ||
            errors.passwordErrorIndicator ||
            error}
        placeholder="Password *"
        bind:value={password}
        disabled={loading}
    />
    {#if errors.password}
        <div class="text-danger mt-1">{errors.password}</div>
    {/if}
</div>
