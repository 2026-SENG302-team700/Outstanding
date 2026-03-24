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
    import ProfilePic from "$lib/profilepic/profilepic.svelte";
    import ImageEditor from "$lib/image-editor/image-editor.svelte";

    let displayName = $state("");
    let email = $state("");
    let country = $state("");
    let files: FileList | null = $state(null);
    let pfpInput: HTMLInputElement;
    let imageEditor: ImageEditor;

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
                user.update((u) => ({
                    ...u,
                    displayName: updatedUser.displayName,
                }));
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

    /**
     * Sends an image to the image editor
     */
    async function sendToEditor() {
        if (!files || files.length === 0) return;
        imageEditor.setImg(files[0]);
    }

    /**
     * Updates the profile picture on the back end
     * @param imageData the x, y and zoom of the new profile picture
     * @param imageFile the file to upload
     */
    async function updatePfp(imageData: PfpData, imageFile: File) {
        try {
            const formData = new FormData();
            formData.append("file", imageFile);
            formData.append("x", imageData.offsetX.toString());
            formData.append("y", imageData.offsetY.toString());
            formData.append("zoom", imageData.zoom.toString());

            const response = await fetchWithCsrf(resolve(`/api/user/pfp`), {
                method: "PUT",
                body: formData,
            });

            if (!response.ok) {
                if (response.status == 500) {
                    throw new Error("Failed to upload picture");
                } else {
                    throw new Error(await response.text());
                }
            } else {
                user.update((u) => ({
                    ...u,
                    pfpData: imageData,
                }));
            }
        } catch (err) {
            addToast((err as Error).message, "error");
        }
    }
</script>

<div class="container d-flex flex-column flex-md-row">
    <div
        class="d-flex flex-column align-items-center justify-content-center m-3"
    >
        <div class="position-relative d-inline-block">
            <ProfilePic pfpData={$user.pfpData} size="xl" />

            <button
                type="button"
                class="btn btn-sm btn-primary rounded-circle position-absolute bottom-0 end-0 p-4 lh-1 d-flex align-items-center justify-content-center"
                data-bs-toggle="modal"
                data-bs-target="#pfpInputModal"
                on:click={() => {
                    imageEditor.reset();
                }}
            >
                <i class="bi bi-pencil-square fs-2"></i>
            </button>
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

<!-- Modal for pfp selection -->
<div
    class="modal fade"
    id="pfpInputModal"
    data-bs-backdrop="static"
    data-bs-keyboard="false"
    tabindex="-1"
    aria-labelledby="pfpInputModalLabel"
    aria-hidden="true"
>
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h1 class="modal-title fs-5" id="pfpInputModalLabel">
                    Edit Profile Picture
                </h1>
                <button
                    type="button"
                    class="btn-close"
                    data-bs-dismiss="modal"
                    aria-label="Close"
                ></button>
            </div>
            <div class="modal-body">
                <button
                    type="button"
                    class="btn btn-primary"
                    on:click={() => pfpInput.click()}
                >
                    Choose Image
                </button>

                <input
                    accept="image/webp, image/jpeg, image/png, image/gif, image/svg+xml"
                    bind:files
                    bind:this={pfpInput}
                    id="pfp"
                    name="pfp"
                    type="file"
                    class="d-none"
                    on:change={sendToEditor}
                />

                <ImageEditor bind:this={imageEditor} />
            </div>
            <div class="modal-footer">
                <button
                    type="button"
                    on:click={() => {
                        const data = imageEditor.exportData();
                        if (data) {
                            updatePfp(data.data, data.file);
                        } else {
                            addToast("No file selected!", "error");
                        }
                    }}
                    class="btn btn-primary" data-bs-dismiss="modal">Submit</button
                >
                <button
                    type="button"
                    class="btn btn-secondary"
                    data-bs-dismiss="modal">Cancel</button
                >
            </div>
        </div>
    </div>
</div>
