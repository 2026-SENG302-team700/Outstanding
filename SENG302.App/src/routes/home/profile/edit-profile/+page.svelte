<script lang="ts">
    // import defaultLogo from '$team-700/SENG302.App/static/defaultProfile.png/';
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { countries } from "$lib/country/countries";
    import { addToast } from "$lib/toast/toast";
    import { user } from "$lib/stores/user";
    import regexPatterns from "../../../../../../SENG302.Shared/regexPatterns.json";

    let displayName = $state("");
    let email = $state("");
    let country = $state("");
    let passwordEdit = $state(false);
    let editVerified = $state(false);
    let currentPassword = $state("");
    let newPassword = $state("");
    let newPasswordConfirm = $state("");
    let verificationCode = $state("");
    let tempCode = "bob"; //TESTING PURPOSES ONLY

    let errors = $state({
        email: "",
        displayName: "",
        password: "",
        verification: "",
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
            const response = await fetchWithCsrf(`/api/user`, {
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

    /**
     * Handles front end validation
     * Updates error messages for invalid fields
     *
     * returns true if all fields are valid, false otherwise
     */
    function isValid() {
        // Clear errors
        errors = {
            email: "",
            displayName: "",
            password:"",
            verification: ""
        };

        // Front end Validation
        let valid = true;

        // Check email format
        const emailRegex = new RegExp(regexPatterns.user.email);
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
        const displayNameRegex = new RegExp(
            regexPatterns.user.displayName.pattern,
            regexPatterns.user.displayName.flags,
        );
        if (!displayNameRegex.test(displayName)) {
            errors.displayName =
                "Display name must only include letters, spaces, hyphens or apostrophes.";
            valid = false;
        }

        // Fields not empty
        if (!email) {
            errors.email = "Email is required.";
            valid = false;
        }

        if (!displayName) {
            errors.displayName = "Display name is required.";
            valid = false;
        }

        return valid;
    }

    function initiatePasswordUpdate() {
        passwordEdit = true;
    }

    async function updatePassword() {}

    /// <summary>
    /// Updates the users information with the provided information
    /// </summary>
    async function updateUser() {
        // Return if error
        if (!isValid()) return;

        try {
            const response = await fetchWithCsrf(`/api/user`, {
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
            } else {
                const data = await response.json().catch(() => null);
                switch (data.errorType) {
                    // check for duplicate email, throws regular error rather than "something went wrong"
                    case "DuplicateEmailException":
                        email = "";
                        errors.email = data.message;
                        break;
                    default:
                        addToast(data?.message || "An error occured.", "error");
                        break;
                }
                return;
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

        <div class="mb-3">
            {#if !passwordEdit && !editVerified} <!--access verification code "inputter"-->
                <button
                    type="reset"
                    class="btn btn-warning"
                    on:click={() => initiatePasswordUpdate()}
                >
                    Edit Password
                </button>
            {/if}
            {#if passwordEdit && !editVerified} <!--input verification code-->
                <label for="displayName" class="form-label"
                    >PLACEHOLDER VERIFICATION CODE</label
                >
                <input
                    type="text"
                    class="form-control"
                    class:is-invalid={errors.verification}
                    bind:value={verificationCode}
                    id="displayName"
                />
                <button
                    type="reset"
                    class="btn btn-warning"
                    on:click={() => initiatePasswordUpdate()}
                >
                    Verify Code
                </button>
            {/if}
            {#if passwordEdit && editVerified} <!--edit password-->
                <label for="displayName" class="form-label"
                    >Current Password</label
                >
                <input
                    type="password"
                    class="form-control"
                    class:is-invalid={errors.password}
                    bind:value={currentPassword}
                    id="displayName"
                />
                <label for="displayName" class="form-label">New Password</label>
                <input
                    type="password"
                    class="form-control"
                    class:is-invalid={errors.password}
                    bind:value={newPassword}
                    id="displayName"
                />
                <label for="displayName" class="form-label"
                    >Confirm New Password</label
                >
                <input
                    type="password"
                    class="form-control"
                    class:is-invalid={errors.password}
                    bind:value={newPasswordConfirm}
                    id="displayName"
                />
            {/if}
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
