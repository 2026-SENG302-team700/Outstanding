<script lang="ts">
    let { value = $bindable(0) }: { value: number } = $props();
</script>

<div class="dropdown">
    <button
        type="button"
        class="btn dropdown-toggle btn-primary"
        data-bs-toggle="dropdown"
        aria-expanded="false"
    >
        {#if value == 0}
            Todo
        {/if}
        {#if value == 1}
            In Progress
        {/if}
        {#if value == 2}
            Done
        {/if}
    </button>
    <ul class="dropdown-menu">
        <li>
            <a class="dropdown-item" onclick={() => (value = 0)}> Todo </a>
        </li>
        <li>
            <a class="dropdown-item" onclick={() => (value = 1)}>
                In Progress
            </a>
        </li>
        <li>
            <a class="dropdown-item" onclick={() => (value = 2)}> Done </a>
        </li>
    </ul>
</div>
