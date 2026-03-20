<script lang="ts">
    // import defaultLogo from '$team-700/SENG302.App/static/defaultProfile.png/';
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { countries } from "$lib/country/countries";
    import { addToast } from "$lib/toast/toast";
    import { user } from "$lib/stores/user";
    import ProfilePic from "$lib/profilepic/profilepic.svelte";

    let displayName = $state("");
    let email = $state("");
    let country = $state("");
    let files: FileList | null = $state(null);
    let pfpInput: HTMLInputElement;

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
                addToast("Profile edited successful");
                const updatedUser = await response.json();
                user.update(u => ({...u, displayName: updatedUser.displayName}));
                goto(resolve("/home"));
            }
        } catch (err) {
            addToast((err as Error).message);
        }
    }
    
    async function updatePfp() {
        if (!files || files.length === 0) return;
        
        try {
            const formData = new FormData();
            formData.append("file", files[0]);

            const response = await fetchWithCsrf(resolve(`/api/user/pfp`), {
                method: "PUT",
                body: formData
            });
            
            if (!response.ok) {
                throw new Error("Failed to save profile picture.");
            } else {
                const pfpResponse = await fetchWithCsrf(resolve('/api/user/pfp'), {
                    method: "GET",
                    credentials: "include",
                });
                const blob = await pfpResponse.blob();
                user.update(u => ({...u, pfpUrl: URL.createObjectURL(blob)}));
            }
        } catch (err) {
            addToast((err as Error).message, "error");
        }
    }
</script>

<div class="container d-flex flex-column flex-md-row">
    <div class="d-flex flex-column align-items-center justify-content-center m-3">
        <div class="position-relative d-inline-block">
            <ProfilePic pfpUrl={$user.pfpUrl} size="xl" />

            <button
                type="button"
                class="btn btn-sm btn-primary rounded-circle position-absolute bottom-0 end-0 p-4 lh-1 d-flex align-items-center justify-content-center"
                on:click={() => pfpInput.click()}
            >
                <i class="bi bi-pencil-square fs-2"></i>
            </button>

            <input
                    accept="image/webp, image/jpeg, image/png, image/gif, image/svg+xml"
                    bind:files
                    bind:this={pfpInput}
                    id="pfp"
                    name="pfp"
                    type="file"
                    class="d-none"
                    on:change={updatePfp}
            />            
        </div>
    </div>
    
    <div class="flex-grow-1 m-3">
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
            <button
                    type="button"
                    class="btn btn-secondary"
                    on:click={() => {
                goto(resolve("/home/profile"));
            }}>Cancel</button
            >
        </form>
    </div>
</div>
