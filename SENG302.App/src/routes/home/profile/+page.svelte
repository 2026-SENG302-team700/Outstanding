<script lang="ts">
    // import defaultLogo from '$team-700/SENG302.App/static/defaultProfile.png/';
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { countries } from "$lib/country/countries";
    import { addToast } from "$lib/toast/toast";
    import ProfilePic from "$lib/profilepic/profilepic.svelte";
    import { user } from "$lib/stores/user";

    let email = $state("");
    let username = $state("");
    let pfpUrl: string | null = $state(null);

    onMount(() => {
        retrieveUserData();
    });

    /// <summary>
    /// Gets User email and Username from local storage
    /// Checks if they non null before assigning them to the reactive variables
    /// </>summary>
    async function retrieveUserData() {
        try {
            const response = await fetchWithCsrf(resolve(`/api/user`), {
                method: "GET",
                credentials: "include",
            });

            const data = await response.json();
            if (!response.ok) {
                email = data.message || "Failed to fetch email.";
                username = data.message || "Failed to fetch username.";
                goto(resolve("/"));
                return;
            }
            email = data.email;
            username = data.displayName;
        } catch (err) {
            email = "Failed to fetch email: " + (err as Error).message;
            username = "Failed to fetch username: " + (err as Error).message;
            goto(resolve("/"));
        }
    }

    async function logoutUser() {
        try {
            const response = await fetchWithCsrf(resolve(`/api/logout`), {
                method: "DELETE",
                credentials: "include",
            });
            
            if (response.status === 500) {
                addToast("Failed to logout. Refresh Webpage", "error")
                return;
            }
            else {
                goto("/");
            }
        } catch (err) {
            addToast(err.message, "error");
        }
    }
</script>

<div class="display: flex; flex-direction: row;">
    <div class="profile-box">
        <button
            class="btn btn-primary ms-auto"
            onclick={() => goto(resolve("/home/profile/edit-profile"))}
            >Edit Profile</button
        >
        <ProfilePic pfpUrl={$user.pfpUrl} size="large" />
        <p class="username">{username}</p>
        <p class="user_email">Email: {email}</p>
        <button
            type="button"
            class="btn btn-outline-danger"
            onclick={logoutUser}
        >
            Logout
        </button>
    </div>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }

    .profile-image {
        width: 20%;
        height: 20%;

        display: inline-block;
        object-fit: cover;
        border-radius: 50%;
    }

    .profile-box {
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: center;
    }

    .username {
        margin-top: 20px;
        font-weight: 600;
        font-style: normal;
        font-size: 2rem;
        font-family: Arial;
    }

    .user_email {
        font-weight: 400;
        font-size: 1rem;
        font-family: Arial;
    }
</style>
