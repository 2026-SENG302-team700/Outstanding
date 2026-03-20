<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { onMount } from "svelte";

    let loading = $state(false);
    let error = $state("");
    let taskItem = $state(null);
    let { params } = $props();

    onMount(() => {
        fetchTaskItem();
    });

    /**
     * Fetches the task item in order
     * to display the information
     */
    async function fetchTaskItem() {
        try {
            loading = true;
            const response = await fetchWithCsrf(
                resolve(`/api/taskItem/item/${params.slug}`),
                {
                    method: "GET",
                    credentials: "include",
                },
            );
            const data = await response.json();
            if (!response.ok) {
                error = data.message || "Failed to fetch task lists.";
                return;
            }
            taskItem = data;
        } catch (err) {
            error = "Failed to fetch task lists: " + (err as Error).message;
        } finally {
            loading = false;
        }
    }

    /**
     * formats the string based on the users locale
     */
    function formatDate(dateString: string) {
        const date = new Date(dateString);
        return date.toLocaleDateString(); // automatically uses user's locale
    }
</script>

<div class="container">
    <div class="mb-3">
        <button
            type="button"
            class="btn btn-secondary"
            on:click={() => goto("..")}
            >Back
        </button>
    </div>
    {#if loading || taskItem === null}
        <div class="text-center text-muted py-4">Loading task...</div>
    {:else}
        <div style="display: flex; flex-direction: column;">
            <h1 class="text-center mb-4">{taskItem.name}</h1>

            <div style="white-space: pre-wrap; word-wrap: break-word;">
                <strong>Description:</strong>
                {taskItem.description}
            </div>
            <div class="mb-2">
                <strong>Due Date:</strong>
                {#if taskItem.dueDate === null}
                    No Due Date
                {:else}
                    {formatDate(taskItem.dueDate)}
                {/if}
            </div>
            <div class="mb-2">
                <strong>Status:</strong>
                {#if taskItem.currentStatus === 0}
                    TODO
                {:else if taskItem.currentStatus === 1}
                    In Progress
                {:else}
                    Done
                {/if}
            </div>
            <div class="mb-2">
                <strong>Created At:</strong>
                {formatDate(taskItem.creationTime)}
            </div>
        </div>
    {/if}
</div>
