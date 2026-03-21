<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import regexPatterns from "../../../../../SENG302.Shared/regexPatterns.json";

    let loading = $state(false);
    let error = $state("");
    let name = $state("");

    /// <summary>
    /// Creates a new task list for the user with the given name. Trims and then validates the name
    /// before sending the request to the backend. If creation is successful, navigates
    /// back to the home screen. If there is an error, displays the error message.
    /// </summary>
    async function createList() {
        const trimmedName = name.trim();

        const errors = [];

        if (trimmedName == "") {
            error = "Task list name cannot be empty";
            return;
        }

        if (trimmedName.length < 3 || trimmedName.length > 128) {
            errors.push(
                "List name is required and must be between 3 and 128 characters long",
            );
        }

        const nameRegex = new RegExp(
            regexPatterns.taskList.name.pattern,
            regexPatterns.taskList.name.flags,
        );
        if (!nameRegex.test(trimmedName)) {
            errors.push(
                "List name cannot contain characters other than letters, spaces, hyphens, apostrophes, or numbers",
            );
        }

        if (errors.length > 0) {
            error = errors.join("\n").trim();
            return;
        }

        try {
            loading = true;
            error = "";
            const response = await fetchWithCsrf(`/api/taskList`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    Name: trimmedName,
                }),
                credentials: "include",
            });

            const data = await response.text();
            if (!response.ok) {
                error = data || "Failed to create list.";
                return;
            }

            goto(resolve(`/home`));
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
            Name your new task list
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
                <div class="text-danger mt-1" style="white-space: pre-wrap">
                    {error}
                </div>
            {/if}
        </div>
        <div>
            <button
                type="submit"
                class="btn btn-primary w-100"
                disabled={loading}
            >
                {loading ? "Creating..." : "Create List"}
            </button>
        </div>
    </form>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
