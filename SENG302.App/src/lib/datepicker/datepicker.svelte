<script lang="ts">
    let { value = $bindable(new Date("0001-01-01")), error = "", disabled = false }: {
        value: Date,
        error?: string,
        disabled?: boolean
    } = $props();
</script>

<div class="d-flex align-items-center gap-2">
    <input
            type="date"
            class="form-control w-auto"
            class:is-invalid={error}
            bind:value
            {disabled}
    />
</div>
{#if error}
    <div class="invalid-feedback d-block">{error}</div>
{/if}
