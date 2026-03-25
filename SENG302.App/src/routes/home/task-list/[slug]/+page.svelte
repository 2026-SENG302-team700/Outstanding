<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { onMount } from "svelte";
    import { formatDate } from "$lib/datepicker/formatDate";

    let loading = $state(false);
    let listName = $state();
    let tasks = $state([]);
    let error = $state("");
    let { params } = $props();

    onMount(() => {
        GetList();
        GetTasks();
    });

    /// <Summary>
    /// Fetches tasks of the certain task list from the backend
    /// and stores them in the frontend as an array of objects
    ///
    /// <Summary>
    async function GetTasks() {
        try {
            loading = true;
            error = "";
            const response = await fetchWithCsrf(
                resolve(`/api/taskItem/${params.slug}` as any),
                {
                    method: "GET",
                    credentials: "include",
                },
            );
            const data = await response.json();
            if (!response.ok) {
                error = data;
                return;
            }
            tasks = data;
        } catch (err) {
            error = "Failed to get tasks: " + (err as Error).message;
        } finally {
            loading = false;
        }
    }

    /// <summary>
    /// Creates a new task list for the user with the given name. Validates the name
    /// before sending the request to the backend. If creation is successful, navigates
    /// back to the home screen. If there is an error, displays the error message.
    /// </summary>
    async function GetList() {
        try {
            loading = true;
            error = "";
            const response = await fetchWithCsrf(
                resolve(`/api/taskList/${params.slug}` as any),
                {
                    method: "GET",
                    credentials: "include",
                },
            );

            const data = await response.json();
            if (!response.ok) {
                error = data || "Failed to get list.";
                return;
            }
            listName = data.name;
        } catch (err) {
            error = "Failed to get list: " + (err as Error).message;
        } finally {
            loading = false;
        }
    }

    /**
     * shorten the length of the displayed description to 'number' characters, add '...' onto the end of the description to indicate more.
     * @param text the description to shorten
     * @param length length of description to cut down too
     */
    function shortenDesc(text: string | null, length: number) {
        if (!text) return "No Description";
        if (text.length <= length) return text;
        return text.slice(0, length) + "...";
    }
</script>

<div class="container">
    <div style="display: flex; flex-direction: row; ">
        <h1 class="text-center mb-4" style="flex: 1; justify-content: center;">
            {listName}
        </h1>
    </div>
    <div class="mb-3">
        <button
            type="button"
            class="btn btn-primary"
            on:click={() =>
                goto(resolve(`/home/task-list/${params.slug}/create-task`))}
            >+ Add Task
        </button>
    </div>
    {#if loading && tasks.length === 0}
        <div class="text-center text-muted py-4">Loading tasks...</div>
    {:else if tasks.length === 0}
        <div class="text-center text-muted py-4">
            No tasks yet. Create your first task above!
        </div>
    {:else}
        <div class="mb-3">
            {#each tasks as task}
                <div
                    class="task-card"
                    tabindex="0"
                    role="button"
                    on:click={() =>
                        goto(
                            `/home/task-list/${params.slug}/task/${task.taskId}`,
                        )}
                    on:keydown={(e) => {
                        if (e.key === "Enter" || e.key === " ") {
                            goto(
                                `/home/task-list/${params.slug}/task/${task.taskId}`,
                            );
                        }
                    }}
                >
                    <div>
                        <span class="fw-bold">Title: </span>
                        <span>{task.name}</span>
                    </div>

                    <div>
                        <span class="fw-bold">Description: </span>
                        <span>
                            {shortenDesc(task.description, 50)}
                        </span>
                    </div>

                    <div>
                        <span class="fw-bold">Status: </span>
                        <span>
                            {#if task.currentStatus === 0}
                                TODO
                            {:else if task.currentStatus === 1}
                                In Progress
                            {:else}
                                Done
                            {/if}
                        </span>
                    </div>

                    <div>
                        <span class="fw-bold">Due Date: </span>
                        <span>
                            {task.dueDate === null
                                ? "No Due Date"
                                : formatDate(task.dueDate)}
                        </span>
                    </div>
                </div>
            {/each}
        </div>
    {/if}
</div>

<style>
    .task-card {
        border: 1px solid lightgrey;
        box-shadow: 0px 0px 5px lightgrey;
        margin-bottom: 8px;
    }

    .task-card[role="button"] {
        cursor: pointer;
        outline: none;
        border-radius: 6px;
        padding: 5px;
    }

    .task-card[role="button"]:focus-visible {
        box-shadow: 0 0 0 3px #4a90e2;
        background-color: #f0f6ff;
    }
</style>
