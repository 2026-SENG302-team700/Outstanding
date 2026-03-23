<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { onMount } from "svelte";
    import DatePicker from "$lib/datepicker/datepicker.svelte";
    import StatusDropdown from "$lib/statusdropdown/statusdropdown.svelte";

    let loading = $state(false);
    let error = $state("");
    let taskItem = $state(null);
    let { params } = $props();
    
    // edit mode variables
    let editMode = $state(false);
    let editedTitle = $state(undefined);
    let editedDesc = $state(undefined);
    let editedDueDate = $state(undefined);
    let editedStatus = $state(undefined);

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

    /**
     * checks changed fields and sets to undefined if same as pre-existing
     */
    function checkChangedFields() {
        if (editedTitle == taskItem.title) {
            editedTitle = undefined;
        }
        
        if (editedDesc == taskItem.desc) {
            editedDesc = undefined;
        }
        
        if (editedDueDate == taskItem.dueDate) {
            editedDueDate = undefined;
        }
        
        if (editedStatus == taskItem.status) {
            editedStatus = undefined;
        }
    }
    
    function fieldValidity() {
        console.log("to be implemented");
        return true;
    }
    
    async function updateTask() {
        checkChangedFields();

        if (!fieldValidity()) {
            return;
        }

        try {
            loading = true;
            error = "";

            const taskId = taskItem.id;
            
            const response = await fetchWithCsrf(
                resolve(`/api/taskItem/item/${params.slug}` as any),
                {
                    method: "PUT",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        taskId,
                        editedTitle,
                        editedDesc,
                        editedDueDate,
                        editedStatus,
                    }),
                },
            );

            const data = await response.json();
            if (!response.ok) {
                error = data.message || "Failed to update task.";
                3
                return;
            }

            taskItem = data;
        } catch (err) {
            error = `Failed to update task: ${err.message}`;
        } finally {
            loading = false;
        }
    }

    /**
     * Handles editMode and update toggling and uploading new data to API.
     */
    async function toggleEditMode() {
        if (editMode) {
            await updateTask();
            editMode = false;
        } else {
            editedStatus = taskItem.currentStatus;
            editedDueDate = taskItem.dueDate
                ? new Date(taskItem.dueDate).toISOString().split('T')[0]
                : "";
            editedTitle = taskItem.name;
            editedDesc = taskItem.description;
            editMode = true;
        }
    }
</script>

<div class="container">
    <div class="mb-3 card-body d-flex justify-content-between align-items-center">
        <button
            type="button"
            class="btn btn-secondary"
            on:click={() => goto("..")}
            >Back
        </button>

        <button
            type="button"
            class="btn btn-primary"
            on:click={toggleEditMode}
        >
            {#if editMode}
                Update
            {:else }
                <i class="bi bi-pencil-square"></i>
            {/if}
        </button>

    </div>
    {#if loading || taskItem === null}
        <div class="text-center text-muted py-4">Loading task...</div>
    {:else}
        <div style="display: flex; flex-direction: column;">
            {#if editMode}
                <strong>Title:</strong>
                <input
                    type="text"
                    class="form-control text-center mb-4 fs-3 fw-bold"
                    bind:value={editedTitle}
                    disabled={loading}
                />
            {:else}
                <h1 class="text-center mb-4">{taskItem.name}</h1>
            {/if}

            <div class="mb-2">
                {#if editMode}
                    <strong>Description:</strong>
                    <input
                        type="text"
                        class="form-control"
                        placeholder="Description"
                        bind:value={editedDesc}
                        disabled={loading}
                    />
                {:else}
                    <strong>Description:</strong>
                    {taskItem.description? taskItem.description : "No Description"}
                {/if}
            </div>
            <div class="mb-2">
                {#if editMode}
                    <strong>Due Date:</strong>
                    <DatePicker
                            bind:value={editedDueDate}
                            disabled={loading} />
                {:else if taskItem.dueDate === null}
                {:else}
                    <strong>Due Date:</strong>
                    {formatDate(taskItem.dueDate)}
                {/if}
            </div>
            <div class="mb-2">
                {#if editMode}
                    <div>
                        <strong>Status:</strong>
                        <StatusDropdown 
                            bind:value={editedStatus}
                        />
                    </div>
                {:else}
                    <!-- probably will search up how to make this 
                    work better at a later point in the task,
                    looks a little ugly. -->
                    <strong>Status:</strong>
                    {#if taskItem.currentStatus === 0}
                        TODO
                    {:else if taskItem.currentStatus === 1}
                        In Progress
                    {:else}
                        Done
                    {/if}
                {/if}

            </div>
            <div class="mb-2">
                <strong>Created At:</strong>
                {formatDate(taskItem.creationTime)}
            </div>
        </div>
    {/if}
</div>
