<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";

    let loading = $state(false);
    let error = $state("");
    let name = $state("");

    /**
     * Handles the creation of a new task list by sending a
     * POST request to the server with the list's name and
     * the user's email. Validates that the name field is filled
     */
    async function createList() {
        if (!name) {
            error = "Please enter a name for the list.";
            return;
        }

        try {
            loading = true;
            error = "";
            let userEmail = localStorage.getItem("userEmail");
            const response = await fetchWithCsrf(resolve(`/api/lists`), {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    name,
                    userEmail,
                }),
            });

            const data = await response.json().catch(() => null);

            if (!response.ok) {
                error = data?.message || "Failed to create list.";
                return;
            }

            goto(resolve(`/home`));
        } catch (err) {
            error = "An unexpected error occurred.";
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
        </div>
    </form>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
