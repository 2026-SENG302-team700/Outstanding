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

    let errors = $state({
        email: "",
        displayName: "",
    });

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
            // Clear errors
            errors = {
                email: "",
                displayName: "",
            };

            // Front end Validation
            let valid = true;

            // Check email format
            const emailRegex = new RegExp(
                "^(?=.{5,254}$)(?=.{1,64}@)[a-zA-Z0-9!#$%&‘*+–/=?^_`{|}~]+" +
                    "(\\.[a-zA-Z0-9!#$%&‘*+–/=?^_`{|}~]+)*" +
                    "@(?=.{3,255}$)([A-Za-z0-9]+[-]*)+" +
                    "(\\.([-]*[A-Za-z0-9]+)+)+$",
            );
            if (!emailRegex.test(email) && email) {
                errors.email =
                    "Invalid email address. Email must be in the format ‘jane@doe.nz’";
                valid = false;
            }

            // Check Display name length
            if (displayName.length < 3 || displayName.length > 64) {
                errors.displayName =
                    "Display name must be between 3 and 64 characters";
                valid = false;
            }

            // Display Name format
            const displayNameRegex = /^[\p{L}0-9\s'-]+$/u;
            if (!displayNameRegex.test(displayName)) {
                errors.displayName =
                    "Display name must only include letters, spaces, hyphens or apostrophes.";
                valid = false;
            }

            // Return if error
            if (!valid) return;

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
                addToast("Profile edited successful");
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
                class:is-invalid={errors.displayName}
                bind:value={displayName}
                id="displayName"
            />
            {#if errors.displayName}
                <div class="invalid-feedback">
                    {errors.displayName}
                </div>
            {/if}
        </div>
        <div class="mb-3">
            <label for="userEmail" class="form-label">Email</label>
            <input
                type="text"
                class="form-control"
                class:is-invalid={errors.email}
                id="userEmail"
                bind:value={email}
            />
            {#if errors.email}
                <div class="invalid-feedback">
                    {errors.email}
                </div>
            {/if}
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
        <button
            type="button"
            class="btn btn-secondary"
            on:click={() => {
                goto(resolve("/home/profile"));
            }}>Cancel</button
        >
    </form>
</div>
