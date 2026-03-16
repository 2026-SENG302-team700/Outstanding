<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { onMount } from "svelte";

    let taskStatus = $state(0); // represents the value of the enum in the backend
    let loading = $state(false);
    let listName = $state("");
    let error = $state("");
    let { params } = $props();

    onMount(() => {
        GetList();
    });

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
            class="btn btn-primary"
            on:click={() => goto(resolve(`/home/task-list/${params.slug}/create-task`))}
            >+ Create Task
        </button>
    </div>
</div>
