<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { onMount } from "svelte";
    import { SvelteDate } from "svelte/reactivity";

    let loading = $state(false);
    let listName = $state("");
    let listId = $state("");
    let error = $state("");
    let name = $state("");
    var dateTime = new SvelteDate();
    let description = $state("");
    let { params } = $props();
    let errors = $state({
        name: "",
        description: "",
        dueDate: "",
        status: "",
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
            status: "",
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

            const response = await fetchWithCsrf(
                resolve(`/api/taskItem` as any),
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        taskId: 0 /*0 is used as a placeholder, and will be replaced with the appropriate id in the backend */,
                        taskListId: listId,
                    }),
                },
            );

            const data = await response.json().catch(() => null);

            console.log(data);

            if (!response.ok) {
                // in case front end form checks were tampered with,
                // we display a toast with the badrequest response
                // from the back end.
                addToast(data?.message || "An error occured.", "error");
                return;
            }

            localStorage.setItem("username", displayName);
            localStorage.setItem("userEmail", email);

            addToast("Registration successful. Please log in.", "success");

            goto(resolve(`/login`));

            localStorage.setItem("justRegistered", "true");
            goto(resolve(`/login`));
        } catch (err) {
            addToast(
                "Failed to register user: " + (err as Error).message,
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
                error = data || "Failed to create list.";
                return;
            }
            listName = data.name;
        } catch (err) {
            error = "Failed to create list: " + (err as Error).message;
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
    <form on:submit={createTask}>
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
                    <li><a class="dropdown-item" href="#">ToDo</a></li>
                    <li><a class="dropdown-item" href="#">Doing</a></li>
                    <li>
                        <a class="dropdown-item" href="#">Done</a>
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
