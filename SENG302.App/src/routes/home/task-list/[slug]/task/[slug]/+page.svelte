<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { onMount } from "svelte";
    import { validateTaskInput } from '$lib/validity/taskValidity';
    import { formatDate } from "$lib/datepicker/formatDate"

    import DatePicker from "$lib/datepicker/datepicker.svelte";
    import StatusDropdown from "$lib/statusdropdown/statusdropdown.svelte";

    let loading = $state(false);
    let error = $state("");
    let taskItem = $state(null);
    let { params } = $props();
    
    // edit mode variables
    let editMode = $state(false);
    let editedName = $state(undefined);
    let editedDesc = $state(undefined);
    let editedDueDate = $state(undefined);
    let editedStatus = $state(undefined);
    let errors = $state({
        name: "",
        description: "",
        dueDate: "",
        taskStatus: "",
    });

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
     * Updates task in backend w/ frontend validation checks
     */
    async function updateTask() {
        const validationData = validateTaskInput(
            editedName,
            editedDesc,
            editedDueDate,
            editedStatus
        );

        if (!validationData.isValid) {
            errors.name = validationData.name;
            errors.description = validationData.description;
            errors.dueDate = validationData.dueDate;
            errors.taskStatus = validationData.taskStatus;
            return;
        }

        try {
            loading = true;
            error = "";

            const taskId = taskItem.taskId;
            
            const response = await fetchWithCsrf(
                resolve(`/api/taskItem/item/${params.slug}` as any),
                {
                    method: "PUT",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        taskId: taskId,
                        name: editedName,
                        description: editedDesc,
                        dueDate: editedDueDate || null,
                        currentStatus: editedStatus,
                    })
                }
            );

            const data = await response.json();
            if (!response.ok) {
                error = data.message || "Failed to update task.";
                return;
            }
            taskItem = data;
        } catch (err) {
            error = `Failed to update task: ${err.message}`;
        } finally {
            loading = false;
            goto("..")
        }
    }

    /**
     * Handles editMode and update toggling and uploading new data to API.
     */
    async function toggleEditMode() {
        if (editMode) {
            await updateTask();
        } else {
            editedStatus = taskItem.currentStatus;
            editedDueDate = taskItem.dueDate
                ? taskItem.dueDate.split('T')[0]
                : "";
            editedName = taskItem.name;
            editedDesc = taskItem.description;
            editMode = true;
        }
    }
</script>

<div class="container">
    <div class="mb-3 card-body d-flex justify-content-between align-items-center">
        {#if editMode}
            <button
                    type="button"
                    class="btn btn-secondary"
                    on:click={() => goto("../../..")}
            >Back
            </button>  
        {:else}
            <button
                    type="button"
                    class="btn btn-secondary"
                    on:click={() => goto("..")}
            >Back
            </button>
        {/if}
        
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
        <div style="display: flex; flex-direction: column;"
            class="mb-4">
            {#if editMode}
                <strong>Title:</strong>
                <input
                    type="text"
                    class="form-control text-center fs-3 fw-bold"
                    class:is-invalid={errors.name}
                    bind:value={editedName}
                    disabled={loading}
                />
                {#if errors.name}
                    <div class="invalid-feedback">
                        {errors.name}
                    </div>
                {/if}
            {:else}
                <h1 class="text-center mb-4">{taskItem.name}</h1>
            {/if}

            <div class="mb-2">
                {#if editMode}
                    <strong>Description:</strong>
                    <input
                        type="text"
                        class="form-control"
                        class:is-invalid="{errors.description}"
                        placeholder="Description"
                        bind:value={editedDesc}
                        disabled={loading}
                    />
                    {#if errors.description}
                        <div class="invalid-feedback">
                            {errors.description}
                        </div>
                    {/if}
                {:else}
                    <strong>Description:</strong>
                    {taskItem.description? taskItem.description : "No Description"}
                {/if}
            </div>
            <div class="mb-2">
                <strong>Due Date:</strong>
                {#if editMode}
                    <DatePicker
                            bind:value={editedDueDate}
                            error={errors.dueDate}
                            disabled={loading} />
                    {#if errors.dueDate}
                        <div class="invalid-feedback d-block">
                            {errors.dueDate}
                        </div>
                    {/if}
                {:else if taskItem.dueDate === null}
                        No Due Date
                {:else}
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
