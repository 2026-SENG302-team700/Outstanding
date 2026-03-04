<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import {onMount} from "svelte";

    let loading = $state(false);
    let error = $state("");
    let name = $state("");
    let username = $state("");

    onMount(() => {
        retrieveUsername();
    });

    async function retrieveUsername() {
        try {
            const response = await fetchWithCsrf(resolve(`/api/user`), {
                method: "GET",
                credentials: "include",
            });

            const data = await response.json();
            if (!response.ok) {
                username = data.message || "Failed to fetch username.";
                goto(resolve("/"));
                return;
            }
            username = data.displayName;
        } catch (err) {
            error = "Failed to fetch username: " + (err as Error).message;
            goto(resolve("/"));
        }
    }

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
    <div style="display: flex; flex-direction: row; ">
        <h1 class="text-center mb-4" style="flex: 1; justify-content: center;">Name your new task list</h1>
        <div style="display: flex; flex-direction: column; align-items: center; justify-content: flex-end;">
            <button
                    class="profile-button"
                    on:click={() => goto(resolve("/profile"))}
            >
                <img
                        class="profile-image"
                        src="/defaultProfile.png"
                        alt="Profile"
                />
            </button>
            <p
                    style="font-size: 14px; vertical-align: center; font-weight: 500;"
            >
                {username}
            </p>
        </div>
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

    .profile-image {
        width: 100%;
        height: 100%;
        object-fit: cover;
        display: inline-block;
        border-radius: 50%;
    }

    .profile-button {
        width: 40px;
        height: 40px;
        justfiy-content: flex-end;
        border: None;
        background-color: white;
        border-radius: 50%;
    }
</style>
