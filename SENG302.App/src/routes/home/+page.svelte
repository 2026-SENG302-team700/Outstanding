<script lang="ts">
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";

    let loading = $state(false);
    let error = $state("");
    let taskLists = $state([]);

    onMount(() => {
        fetchLists();
    });

    // TODO: Implement fetching task lists from the backend
    async function fetchLists() {
        return [];
    }
</script>

<div class="container">
    <h1 class="text-center mb-4">Home Screen (WIP)</h1>
    <div class="card-body d-flex justify-content-between align-items-center">
        <h5 class="card-title mb-0">Your Task Lists</h5>
        <button
            class="btn btn-primary"
            on:click={() => goto(resolve("/home/new-list"))}
        >
            Create New Task List
        </button>
    </div>
    {#if loading && taskLists.length === 0}
        <div class="text-center text-muted py-4">Loading task lists...</div>
    {:else if taskLists.length === 0}
        <div class="text-center text-muted py-4">
            No task lists yet. Create your first task list above!
        </div>
    {:else}
        <div class="table-responsive">
            <table class="table table-hover"></table>
        </div>
    {/if}
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
