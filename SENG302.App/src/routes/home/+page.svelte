<script lang="ts">
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";

    let loading = $state(false);
    let error = $state("");
    let taskLists = $state([]);

    onMount(() => {
        fetchLists();
    });

    // TODO: Implement fetching task lists from the backend
    async function fetchLists() {
        return [];
    }
</script>

<div class="container">
    <div style="display: flex; flex-direction: row; ">
        <h1
                class="text-center mb-4"
                style="flex: 1; justify-content: center"
        >
            Home Screen (WIP)
        </h1>
        <div style="display: flex; flex-direction: column">
            <button
                    class="profile-button"
                    on:click={() => goto(resolve("/profile"))}
            >
                <img class="profile-image" src="/defaultProfile.png" alt="Profile">

            </button>
            <p style="font-size: 14px; vertical-align: center; ">Profile</p>
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
            <table class="table table-hover"></table>
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
