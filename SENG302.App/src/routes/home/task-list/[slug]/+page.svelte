<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { onMount } from "svelte";

    let loading = $state(false);
    let listName = $state("");
    let error = $state("");
    let name = $state("");
    let description = $state("");
    let { params } = $props();
    let errors = $state({
        name: "",
        description: "",
        dueDate: "",
        status: "",
    });

    onMount(() => {
        GetList();
    });

    function validateInputs(): boolean {
        let valid = true;
        // Reset errors
        errors = {
            name: "",
            description: "",
            dueDate: "",
            status: "",
        };

        // Check if passwords match
        if (password !== passwordConfirm) {
            errors.passwordConfirm = "Passwords do not match.";
            valid = false;
        }

        // Check password validity
        const passwordRegex =
            /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$/;
        if (!passwordRegex.test(password)) {
            errors.password =
                "Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters";
            valid = false;
        }

        // Check display name length
        if (displayName.length < 3 || displayName.length > 64) {
            errors.displayName =
                "Display name must be between 3 and 64 characters.";
            valid = false;
        }

        // Check display name validity
        const displayNameRegex = /^[\p{L}0-9\s'-]+$/u;
        if (!displayNameRegex.test(displayName)) {
            errors.displayName =
                "Display name must only include letters, spaces, hyphens or apostrophes.";
            valid = false;
        }

        // Check for empty fields
        if (!email) {
            errors.email = "Email is required.";
            valid = false;
        }

        if (!displayName) {
            errors.displayName = "Display name is required.";
            valid = false;
        }

        if (!selectedCountryCode) {
            errors.country = "Please select a country.";
            valid = false;
        }

        if (!password) {
            errors.password = "Password is required.";
            valid = false;
        }

        if (!passwordConfirm) {
            errors.passwordConfirm = "Please confirm your password.";
            valid = false;
        }

        return valid;
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
                resolve(`/api/taskList/${params.slug}`),
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
    <form on:submit={createList}>
        <div class="mb-3">
            <input
                type="text"
                class="form-control"
                class:error
                placeholder="Name *"
                bind:value={name}
                disabled={loading}
            />
            {#if error}
                <div class="text-danger mt-1">{error}</div>
            {/if}
            <input
                type="text"
                class="form-control"
                class:error
                placeholder="Description (Optional)"
                bind:value={name}
                disabled={loading}
            />
            {#if error}
                <div class="text-danger mt-1">{error}</div>
            {/if}

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
</style>
