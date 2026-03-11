<script lang="ts">
    // import defaultLogo from '$team-700/SENG302.App/static/defaultProfile.png/';
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { countries } from "$lib/country/countries";
    import { addToast } from "$lib/toast/toast";
    import { user } from "$lib/stores/user";

    let displayName = $state("");
    let email = $state("");
    let country = $state("");

    onMount(() => {
        retrieveUserData();
    });

    /// <summary>
    /// Sends GET request for the users data and
    /// sets the feilds the the retrieved data
    /// </summary>
    async function retrieveUserData() {
        try {
            const response = await fetchWithCsrf(resolve(`/api/user`), {
                method: "GET",
                credentials: "include",
            });

            const data = await response.json();
            if (!response.ok) {
                email = data.message || "Failed to fetch email.";
                displayName = data.message || "Failed to fetch username.";
                goto(resolve("/"));
                return;
            }
            email = data.email;
            displayName = data.displayName;
            country = data.country;
        } catch (err) {
            email = "Failed to fetch email: " + (err as Error).message;
            displayName = "Failed to fetch username: " + (err as Error).message;
            goto(resolve("/"));
        }
    }

    /// <summary>
    /// Updates the users information with the provided information
    /// </summary>
    async function updateUser() {
        try {
            const response = await fetchWithCsrf(resolve(`/api/user`), {
                method: "PUT",
                credentials: "include",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    email,
                    displayName,
                    country,
                }),
            });

            if (response.ok) {
                addToast("Update Successful");
                const updatedUser = await response.json();
                user.set(updatedUser);
                goto(resolve("/home"));
            }
        } catch (err) {
            addToast((err as Error).message);
        }
    }
</script>

<div class="container">
    <form on:submit|preventDefault={updateUser}>
        <div class="mb-3">
            <label for="displayName" class="form-label">Display Name</label>
            <input
                type="text"
                class="form-control"
                bind:value={displayName}
                id="displayName"
            />
        </div>
        <div class="mb-3">
            <label for="userEmail" class="form-label">Email</label>
            <input
                type="email"
                class="form-control"
                id="userEmail"
                bind:value={email}
            />
        </div>
        <div class="mb-3">
            <label for="country" class="form-label">Country</label>
            <select
                class="form-control"
                class:country-select={!country}
                bind:value={country}
                id="country"
            >
                {#each countries as country}
                    <option value={country.code}>
                        {country.name}
                    </option>
                {/each}
            </select>
        </div>
        <button type="submit" class="btn btn-primary">Update</button>
    </form>
</div>
