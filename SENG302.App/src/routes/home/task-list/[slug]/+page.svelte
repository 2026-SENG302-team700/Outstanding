<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { onMount } from "svelte";

    let loading = $state(false);
    let listName = $state();
    let tasks = $state([]);
    let error = $state("");
    let { params } = $props();

    onMount(() => {
        GetList();
        GetTasks();
    });

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
            console.log(response);
            const data = await response.json();
            console.log("Data: ", data);
            if (!response.ok) {
                error = data;
                return;
            }
            console.log(`The data: ${data}`);
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
     * formats the string based on the users locale
     */
    function formatDate(dateString: string) {
        const date = new Date(dateString);
        return date.toLocaleDateString(); // automatically uses user's locale
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
            >+ Create Task
        </button>
    </div>
    {#if loading && tasks.length === 0}
        <div class="text-center text-muted py-4">Loading tasks...</div>
    {:else if tasks.length === 0}
        <div class="text-center text-muted py-4">
            No tasks yet. Create your first task above!
        </div>
    {:else}
        <div
            class="table-responsive"
            style="max-height: 300px; overflow: scroll;"
        >
            <table class="table table-hover">
                <thead style="position: sticky; top: 0;">
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                        <th>Status</th>
                        <th>Due Date</th>
                    </tr>
                </thead>
                <tbody>
                    {#each tasks as task}
                        <tr
                            on:click={() =>
                                goto(
                                    `/home/task-list/${params.slug}/task/${task.taskId}`,
                                )}
                            style="cursor: pointer; white-space: pre;"
                        >
                            <td class="text-truncate" style="max-width: 200px;"
                                >{task.name}</td
                            >
                            <td class="text-truncate" style="max-width: 200px;"
                                >{task.description}</td
                            >
                            {#if task.currentStatus === 0}
                                <td>{"TODO"}</td>
                            {:else if task.currentStatus === 1}
                                <td>{"In Progress"}</td>
                            {:else}
                                <td>{"Done"}</td>
                            {/if}
                            {#if task.dueDate === null}
                                <td>No Due Date</td>
                            {:else}
                                <td>{formatDate(task.dueDate)}</td>
                            {/if}
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    {/if}
</div>
