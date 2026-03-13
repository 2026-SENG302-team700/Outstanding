<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { onMount } from "svelte";
    import { SvelteDate } from "svelte/reactivity";
    import { addToast } from "$lib/toast/toast";

    let taskStatus = $state(0); // represents the value of the enum in the backend
    let loading = $state(false);
    let listName = $state("");
    let error = $state("");
    let name = $state("");
    var dateTime = new SvelteDate();
    let description = $state("");
    let { params } = $props();
    let errors = $state({
        name: "",
        description: "",
        dueDate: "",
        taskStatus: "",
    });
    var currentTime = new SvelteDate();

    onMount(() => {
        GetList();
    });

    /**
     * enforces form formatting is correct in the front end, for speed.
     */
    function validateInputs(): boolean {
        let valid = true;
        // Reset errors
        errors = {
            name: "",
            description: "",
            dueDate: "",
            taskStatus: "",
        };

        // Check if name and description length
        if (name.length > 128 || name.length < 3) {
            errors.name =
                "Task name must be between 3 and 128 characters long! Currently its " +
                name.length +
                " characters long.";
            valid = false;
        }
        if (description.length > 2048) {
            errors.name =
                "The description cannot be longer than 2048 characters long! Currently its " +
                description.length +
                " characters long.";
            valid = false;
        }

        // Check date validity
        if (dateTime < currentTime) {
            errors.dueDate = "Date cannot be in the past!";
            valid = false;
        }

        return valid;
    }

    async function createTask() {
        if (!validateInputs()) return;

        try {
            loading = true;

            //const year = dateTime.getFullYear();
            //const month = String(dateTime.getMonth() + 1).padStart(2, "0");
            //const day = String(dateTime.getDay()).padStart(2, "0");
            //const hours = String(dateTime.getHours()).padStart(2, "0");
            //const mins = String(dateTime.getDay()).padStart(2, "0");
            //const secs = String(dateTime.getDay()).padStart(2, "0");
            //
            //const newDate = `${year}-${month}-${day}T${hours}:${mins}:${secs}`;

            var bod = JSON.stringify({
                taskListId: params.slug,
                name,
                description,
                DueDate: dateTime,
                currentStatus: taskStatus,
            });

            const response = await fetchWithCsrf(resolve(`/api/taskItem`), {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    taskListId: params.slug,
                    name,
                    description,
                    DueDate: dateTime,
                    currentStatus: taskStatus,
                }),
                credentials: "include",
            });

            const data = await response.json().catch(() => null);
            console.log(bod);

            if (!response.ok) {
                // in case front end form checks were tampered with,
                // we display a toast with the badrequest response
                // from the back end.
                addToast(data?.message || "An error occured.", "error");
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
        <h1 class="text-center mb-4" style="flex: 1; justify-content: center;">
            {listName}
        </h1>
    </div>
    <div class="mb-3">
        <button
            type="button"
            class="btn btn-secondary"
            on:click={() => goto(resolve("/home"))}
            >Cancel
        </button>
    </div>
    <form on:submit|preventDefault={createTask}>
        <div class="mb-3">
            <input
                type="text"
                class="form-control"
                class:is-invalid={errors.name}
                placeholder="Name *"
                bind:value={name}
                disabled={loading}
            />
            {#if errors.name}
                <div class="invalid-feedback">
                    {errors.name}
                </div>
            {/if}
        </div>

        <div class="mb-3">
            <input
                type="text"
                class="form-control"
                class:is-invalid={errors.description}
                placeholder="Description (Optional)"
                bind:value={description}
                disabled={loading}
            />
            {#if errors.description}
                <div class="invalid-feedback">
                    {errors.description}
                </div>
            {/if}
        </div>

        <div class="mb-3">
            <div class="dropdown">
                <button
                    type="button"
                    class="btn dropdown-toggle btn-primary"
                    data-bs-toggle="dropdown"
                    aria-expanded="false"
                >
                    Select status
                </button>
                <ul class="dropdown-menu">
                    <li>
                        <a
                            class="dropdown-item"
                            on:click={() => (taskStatus = 0)}>ToDo</a
                        >
                    </li>
                    <li>
                        <a
                            class="dropdown-item"
                            on:click={() => (taskStatus = 1)}
                            ref="#">Doing</a
                        >
                    </li>
                    <li>
                        <a
                            class="dropdown-item"
                            on:click={() => (taskStatus = 2)}>Done</a
                        >
                    </li>
                </ul>
            </div>
            {#if error}
                <div class="text-danger mt-1">{error}</div>
            {/if}
        </div>

        <div class="mb-3">
            <div class="row">
                <p class="due-text">Due Date</p>
                <input
                    type="datetime-local"
                    class="form-control"
                    class:is-invalid={errors.dueDate}
                    bind:value={dateTime}
                    disabled={loading}
                />
                {#if errors.dueDate}
                    <div class="invalid-feedback">
                        {errors.dueDate}
                    </div>
                {/if}
            </div>
        </div>
        <div>
            <button
                type="submit"
                class="btn btn-primary w-100"
                disabled={loading}
            >
                {loading ? "Creating..." : "Create Task"}
            </button>
        </div>
    </form>
</div>

<style>
    @import url("https://stackpath.bootstrapcdn.com/bootstrap/5.3.0/css/bootstrap.min.css");
    .cursor-pointer {
        cursor: pointer;
    }

    .row {
        display: grid;
        grid-template-columns: 20% 80%;
        width: 100%;
    }

    .due-test {
        height: 100%;
        text-align: center;
    }
</style>
