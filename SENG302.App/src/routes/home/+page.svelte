<script lang="ts">
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import CreateItemButton from "$lib/components/create-item-button.svelte";

    let loading = $state(false);
    let error = $state("");
    let taskLists = $state([]);

    onMount(() => {
        fetchLists();
    });


    /// <summary>
    /// Fetches the logged-in user's task lists from the server
    // using there authorization token and updates the component state.
    async function fetchLists() {
        try {
            loading = true;
            const response = await fetchWithCsrf(resolve(`/api/taskList`), {
                method: "GET",
                credentials: "include",
            });
            const data = await response.json();
            if (!response.ok) {
                error = data.message || "Failed to fetch task lists.";
                return;
            }
            taskLists = data;
        } catch (err) {
            error = "Failed to fetch task lists: " + (err as Error).message;
        } finally {
            loading = false;
        }
    }
</script>

<div class="container">
    <div style="display: flex; flex-direction: row; ">
        <h1 class="text-center mb-4" style="flex: 1; justify-content: center">
            Home
        </h1>
    </div>

    <div class="card-body d-flex justify-content-between align-items-center">
        <h5 class="card-title mb-0">Your Task Lists</h5>
        <CreateItemButton buttonType={"list"} {loading} path="/home/new-list" />
    </div>
    {#if loading && taskLists.length === 0}
        <div class="text-center text-muted py-4">Loading task lists...</div>
    {:else if taskLists.length === 0}
        <div class="text-center text-muted py-4">
            No task lists yet. Create your first task list above!
        </div>
    {:else}
        <div
            class="overflow-y-auto bg-white text-dark mt-2"
            style="max-height: 400px;"
        >
            <span class="fs-5 p-2 mb-2"><b>Name</b></span>
            {#each taskLists as taskList}
                <div
                    class="text-break border-bottom task-item-box p-2"
                    tabindex="0"
                    role="button"
                    style="width: 1270px;"
                    onclick={() => goto(resolve(`/home/task-list/${taskList.id}`))}
                    onkeydown={(e) => {
                        if (e.key === "Enter" || e.key === " ") {
                            goto(resolve(`/home/task-list/${taskList.id}`));
                        }
                    }}
                >
                    <span class="fs-5">{taskList.name}</span>
                </div>
            {/each}
        </div>
    {/if}
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }

    .task-item-box {
        background-color: transparent;
        transition: background-color 0.2s ease;
    }

    .task-item-box:hover {
        background-color: #f8f8f8;
    }
</style>
