<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { onMount } from "svelte";
    import { addToast } from "$lib/toast/toast";
    import { validateTaskInput } from "$lib/validity/taskValidity";
    import DatePicker from "$lib/datepicker/datepicker.svelte";
    import StatusDropdown from "$lib/statusdropdown/status-dropdown.svelte";
    import CancelButton from "$lib/components/cancel-button.svelte";
    import DescriptionForm from "$lib/components/description-form.svelte";
    import ObjectNameForm from "$lib/components/object-name-form.svelte";

    let taskStatus = $state(0); // represents the value of the enum in the backend
    let loading = $state(false);
    let listName = $state("");
    let error = $state("");
    let name = $state("");
    let taskDue = $state(new Date("0001-01-01"));
    let description = $state("");
    let { params } = $props();
    let errors = $state({
        name: "",
        description: "",
        dueDate: "",
        taskStatus: "",
    });

    let datePicker: HTMLInputElement | undefined = $state();

    onMount(() => {
        GetList();

        if (datePicker)
            datePicker.addEventListener("invalid", function (event) {
                event.preventDefault();
                errors.dueDate = "Please input a valid date";
            });
    });

    /**
     * queries the backend with the information for creating a task. Throws errors if the backend finds
     * issues with the query, and reloads the page once the the query has been excepted.
     */
    async function createTask() {
        const dueDate = new Date(taskDue);

        if (dueDate.getFullYear() !== 1) {
            dueDate.setHours(23, 59, 59, 999);
        }

        const validationData = validateTaskInput(
            name,
            description,
            dueDate,
            taskStatus,
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
            const response = await fetchWithCsrf(
                resolve(`/api/taskItem` as any),
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        taskListId: params.slug,
                        name: name.trim(),
                        description: description.trim(),
                        DueDate: dueDate,
                        currentStatus: taskStatus,
                    }),
                    credentials: "include",
                },
            );

            if (!response.ok) {
                // in case front end form checks were tampered with,
                // we display a toast with the badrequest response
                // from the back end.

                addToast(
                    (await response.text()) || "An error occured.",
                    "error",
                );
                return;
            }

            addToast("Task creation successful.", "success");

            goto(resolve(`/home/task-list/${params.slug}`));
        } catch (err) {
            addToast(
                "Failed to create task: " + (err as Error).message,
                "error",
            );
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
</script>

<div class="container">
    <div style="display: flex; flex-direction: row; ">
        <h1
            class="text-break text-center mb-4"
            style="flex: 1; justify-content: center; width: 1000px;"
        >
            {listName}
        </h1>
    </div>
    <div class="mb-3">
        <CancelButton path={"."} />
    </div>
    <form on:submit|preventDefault={createTask}>
        <div class="mb-3">
            <ObjectNameForm
                bind:displayName={name}
                error={errors.name}
                {loading}
                type={"task"}
            />
        </div>

        <div class="mb-3">
            <DescriptionForm
                bind:description
                error={errors.description}
                {loading}
            />
        </div>

        <div class="mb-3 d-flex align-items-center gap-2">
            <label class="form-label mb-0">Status:</label>
            <StatusDropdown bind:value={taskStatus} />
            {#if error}
                <div class="text-danger mt-1">{errors.taskStatus}</div>
            {/if}
        </div>

        <div class="mb-3">
            <div class="d-flex align-items-center gap-2">
                <label class="form-label mb-0">Due Date:</label>
                <DatePicker
                    bind:value={taskDue}
                    error={errors.dueDate}
                    bind:date={datePicker}
                    disabled={loading}
                />
            </div>
            {#if errors.dueDate}
                <div class="invalid-feedback d-block">
                    {errors.dueDate}
                </div>
            {/if}
        </div>

        <div>
            <button
                type="submit"
                class="btn btn-primary w-100"
                disabled={loading}
            >
                {loading ? "Creating..." : "Create"}
            </button>
        </div>
    </form>
</div>

<style>
    .row {
        display: grid;
        grid-template-columns: 20% 80%;
        width: 100%;
    }

    .due-text {
        margin: 8%;
        position: relative;
    }
</style>
