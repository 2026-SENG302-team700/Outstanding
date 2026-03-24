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
    <button
            type="button"
            class="btn btn-outline-secondary btn-sm"
            {disabled}
            onclick={() => value = ""}
    >Clear</button>
</div>
