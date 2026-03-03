<script lang="ts">
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";

    let loading = $state(false);
    let error = $state("");
    let taskLists = $state([]);
    let username = $state("")
    
    onMount(() => {
        fetchLists();
        retrieveUsername()
    });

    /// <summary>
    /// Retrieves the User username from storage & checks they are non null before assigning them to reactive variables
    /// </summary>
    function retrieveUsername() {
        const userData = localStorage.getItem("username");
        if (userData !== null) {
            username = userData;
        } else {
            username = "Profile";
        }
    }

    /// <summary>
    /// Fetches the task lists associated with the current user's email from the backend.
    /// If the request is successful, updates the taskLists state with the retrieved data.
    /// If there is an error, updates the error state with the error message.
    /// </summary>
    
    async function fetchLists() {
        try {
            var userEmail = localStorage.getItem("userEmail");
            console.log("Fetching task lists for user:", userEmail);
            loading = true;
            const response = await fetchWithCsrf(
                resolve(`/api/tasks/user/${encodeURIComponent(userEmail!)}`),
            );
            const data = await response.json();
            if (!response.ok) {
                error = data.message || "Failed to fetch task lists.";
                return;
            }
            taskLists = data;
        } catch (err) {
            error = "Failed to fetch task lists: " + (err as Error).message;
        } finally {
            loading = false;
        }
    }
</script>

<div class="container">
    <div style="display: flex; flex-direction: row; ">
        <h1
                class="text-center mb-4"
                style="flex: 1; justify-content: center"
        >
            Home
        </h1>
        <div style="display: flex; flex-direction: column; align-items: center">
            <button
                    class="profile-button"
                    on:click={() => goto(resolve("/profile"))}
            >
                <img class="profile-image" src="/defaultProfile.png" alt="Profile">

            </button>
            <p style="font-size: 14px; vertical-align: center; font-weight: 500;">{username}</p>
        </div>
    </div>
    
    <div class="card-body d-flex justify-content-between align-items-center">
        <h5 class="card-title mb-0">Your Task Lists</h5>
        <button
            class="btn btn-primary"
            on:click={() => goto(resolve("/home/new-list"))}
        >
            Add Task List
        </button>
    </div>
    {#if loading && taskLists.length === 0}
        <div class="text-center text-muted py-4">Loading task lists...</div>
    {:else if taskLists.length === 0}
        <div class="text-center text-muted py-4">
            No task lists yet. Create your first task list above!
        </div>
    {:else}
        <div class="table-responsive">
            <table class="table table-hover">
                <thead>
                    <tr>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {#each taskLists as list}
                        <tr
                            class="cursor-pointer"
                            on:click={() =>
                                goto(resolve(`/home/list/${list.id}`))}
                        >
                            <td>{list.name}</td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    {/if}
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
        width:40px; 
        height:40px; 
        justfiy-content: flex-end; 
        border: None; 
        background-color: white; 
        border-radius: 50%
    }
</style>
