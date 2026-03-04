<script lang="ts">
    import {onMount} from "svelte";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";

    let username = $state("");
    
    onMount(() => {
        retrieveUsername();
    });

    /// <summary>
    /// Retrieves the users Display name from the backend
    /// </summary>
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
</script>

<nav class="navBar">
    <div style="display: flex; flex-direction: row; align-items: center; padding-top: 5px; ">
        <button
                class="btn btn-primary w-15"
                on:click={() => goto(resolve("/home"))}
                style="margin-left: 7px;"
        >
            Home
        </button>
        <div style="display: flex; flex-direction: row; align-items: center; flex: 1; justify-content: flex-end; margin-right: 5px;" >
            <p
                    style="font-size: 18px; vertical-align: bottom; margin-bottom: 0px; padding-bottom: 0px; font-weight: 500;"
            >
                {username}
            </p>
            <button
                    class="profile-button"
                    on:click={() => goto(resolve("/home/profile"))}
            >
                <img
                        class="profile-image"
                        src="/defaultProfile.png"
                        alt="Profile"
                />
            </button>
        </div>
        
    </div>
</nav>
<slot/>

<style>
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
        margin-left: 5px;
    }
    
    .navBar {
        top: 0;
        z-index: 1000;
        vertical-align: top;
        background-color: white;
        height: 50px;
        margin-top: 0px;
        padding-top: 0px;
        box-shadow: 1px 5px 4px rgba(214, 213, 210, 0.75), -1px -5px 4px rgba(214, 213, 210, 0.75);
        border-radius: 10px;
        margin-bottom: 20px;
    }
</style>