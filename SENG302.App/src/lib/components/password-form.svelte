<script lang="ts">
    let {
        password = $bindable(""),
        error = "",
        loading = false,
        passConfirm = false,
    } = $props();

    let showPassword = $state(false);
</script>

<div class="mb-3">
    <div class="input-group">
        <input
                type={showPassword ? "text" : "password"}
                class="form-control"
                class:error
                class:is-invalid={error}
                autocomplete="new-password"
                placeholder={!passConfirm ? "Password *" : "Confirm Password"}
                bind:value={password}
                disabled={loading}
        />

        <button
                type="button"
                class="btn btn-outline-secondary"
                onclick={() => (showPassword = !showPassword)}
                disabled={loading}
                aria-label="Toggle password visibility"
        >
            <i class={showPassword ? "bi bi-eye-slash" : "bi bi-eye"}></i>
        </button>
            {#if error}
        <div class="invalid-feedback">{error}</div>
    {/if}
    </div>
</div>