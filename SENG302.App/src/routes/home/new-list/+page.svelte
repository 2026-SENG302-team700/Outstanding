<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";

    let loading = $state(false);
    let error = $state("");
    let name = $state("");

    /// <summary>
    /// Creates a new task list for the user with the given name. Validates the name
    /// before sending the request to the backend. If creation is successful, navigates
    /// back to the home screen. If there is an error, displays the error message.
    /// </summary>
    async function createList() {
        if (!name) {
            error = "Please enter a name for the list.";
            return;
        }

        try {
            loading = true;
            error = "";
            let userEmail = localStorage.getItem("userEmail");
            console.log(
                "Creating list with name:",
                name,
                "for user:",
                userEmail,
            );
            const response = await fetchWithCsrf(resolve(`/api/tasks`), {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    Name: name,
                    userEmail: userEmail,
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
    <h1 class="text-center mb-4">Name your new task list</h1>
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
                placeholder="Name"
                bind:value={name}
                disabled={loading}
            />
        </div>
        <div>
            <button
                type="submit"
                class="btn btn-primary w-100"
                disabled={loading}
            >
                {loading ? "Creating..." : "Create List"}
            </button>
            {#if error}
                <div class="alert alert-danger" role="alert">{error}</div>
            {/if}
        </div>
    </form>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
