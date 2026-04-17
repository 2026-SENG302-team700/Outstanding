<script lang="ts">
    import { onMount } from "svelte";
    import type { Modal } from "bootstrap";
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
    let modalElement: HTMLElement | undefined = $state(); 
    let authModal: Modal | undefined;
    let resendTimer = $state(0);
    let isSending = $state(false);
    let currentModalStep = $state("verify");
    let newPassword = $state("");
    let confirmPassword = $state("");
    let oldPassword = $state("");
    let digit1 = $state("");
    let digit2 = $state("");
    let digit3 = $state("");
    let digit4 = $state("");
    let digit5 = $state("");
    let digit6 = $state("");
    let userCode = $derived(
        digit1 + digit2 + digit3 + digit4 + digit5 + digit6,
    );
    let updatingPassword = $state(false)

    let codeError = $state("");
    let imageEditor: ImageEditor;
    let pfpCancelButton: HTMLButtonElement;

    let errors = $state({
        email: "",
        displayName: "",
        oldPassword: "",
        newPassword: "",
        confirmPassword: ""
    });
    // automatically trigger the checkCode when the length reaches 6
    $effect(() => {
        if (userCode.length === 6) {
            checkCode();
        }
    });

    onMount(async () => {
        retrieveUserData();

        const { Modal: BootstrapModal } = await import("bootstrap");

        if (modalElement) {
            authModal = new BootstrapModal(modalElement);
        }
    });

    /**
     * Clears all currently set errors
     */
    function clearErrors() {
        errors.email = "";
        errors.displayName = "";
        errors.oldPassword = "";
        errors.newPassword = "";
        errors.confirmPassword = "";
    }

    /**
     * Clears all the modal data
     */
    function clearModalData() {
        codeError = "";
        digit1 = "";
        digit2 = "";
        digit3 = "";
        digit4 = "";
        digit5 = "";
        digit6 = "";
    }

    /**
     * Start a new timer for the resend button to ensure the user cant spam their email
     */
    function startResendCountdown() {
        resendTimer = 30;
        const interval = setInterval(() => {
            resendTimer--;
            if (resendTimer <= 0) clearInterval(interval);
        }, 1000);
    }

    /**
     * Method used to send a new code to the user. Check the conditions are right and send a PUT request to the backend
     */
    async function requestPasswordChange() {
        clearModalData()
        currentModalStep = "verify";

        authModal?.show();
        
        if (!isSending && resendTimer == 0) {
            isSending = true;
            resendTimer = 0;
            try {
                const response = await fetchWithCsrf(
                    resolve(`/api/user/password/code/generation`),
                    {
                        method: "PUT",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify({
                            email: email,
                        }),
                    },
                );

                if (response.ok) {
                    addToast("Verification code sent!", "success");
                    startResendCountdown();
                } else {
                    const data = await response.json().catch(() => null);
                    codeError = data?.message || "Failed to send code.";
                }
            } catch (err) {
                codeError = "Failed to send email: " + (err as Error).message;
            } finally {
                isSending = false;
            }
        }
    }

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
        clearErrors()

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
    /// Automatically refocus on the next input box
    /// </summary>
    function handleInput(e: Event) {
        const input = e.target as HTMLInputElement;
        if (input.value && input.nextElementSibling) {
            (input.nextElementSibling as HTMLInputElement).focus();
        }
    }
    /// <summary>
    /// Move the focus back one box when backspace is clicked and the input box is empty
    /// </summary>
    function handleKeyDown(e: KeyboardEvent) {
        const input = e.target as HTMLInputElement;
        if (
            e.key === "Backspace" &&
            !input.value &&
            input.previousElementSibling
        ) {
            (input.previousElementSibling as HTMLInputElement).focus();
        }
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
     * Validated the inputs to a change password request and sets the appropriate errors
     * the new passwords must match and be of the correct form
     * returns whether result is valid or not
     */
    function validateChangePasswordInputs() {
        clearErrors()
        var isValid = true;
        
        // Check not empty
        if (!oldPassword) {
            errors.oldPassword = "field is required";
            isValid = false;
        }
        if (!newPassword) {
            errors.newPassword = "field is required";
            isValid = false;
        }
        if (!confirmPassword) {
            errors.confirmPassword = "field is required";
            isValid = false;
        }
        
        // checks passwords match
        if (newPassword !== confirmPassword) {
            isValid = false;
            errors.confirmPassword = "Passwords do not match"
            confirmPassword = "";
        }

        // check password is valid
        const passwordRegex = new RegExp(regexPatterns.user.password);
        if (!passwordRegex.test(newPassword)) {
            isValid = false;
            errors.newPassword = "Password must be at least 8 characters long including at least one of each " +
                "uppercase, lowercase, numbers and special characters"
            newPassword = "";
            confirmPassword = "";
        }
        
        return isValid;
    }
    /**
     * Check the code the user supplied when the user clicks the verify button. Send a post request to the backend with the provided code.
     */
    async function checkCode() {
        if (userCode.length < 6) return;
        try {
            codeError = "";
            const response = await fetchWithCsrf(
                resolve(`/api/user/password/code/validation`),
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        Email: email,
                        Code: userCode,
                    }),
                },
            );

            if (!response.ok) {
                const data = await response.json().catch(() => null);
                codeError =
                    data?.message || `Error ${response.status}: Invalid code.`;
                digit1 = digit2 = digit3 = digit4 = digit5 = digit6 = "";
                const firstInput = document.querySelector(
                    "#code-input input",
                ) as HTMLInputElement;
                firstInput?.focus();
            } else {
                currentModalStep = "update";
            }
        } catch (err) {
            codeError = "Connection error. Please try again later.";
        }
    }

        /**
         * validates the input data and sends a request to the backend to update the users password
         */
        async function updatePassword() {
            const valid = validateChangePasswordInputs();
            if (!valid) return;
            
            updatingPassword = true;
            try {
                const response = await fetchWithCsrf(resolve(`/api/user/password`),
                    {
                        method: "PUT",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify({
                            oldPassword: oldPassword,
                            newPassword: newPassword,
                            newPasswordConfirm: confirmPassword
                        })
                    }
                );
                if (response.ok) {
                    addToast("New password updated successfully")
                    authModal.hide()
                    goto(resolve("/home/profile"));
                    return;
                }
                
                const data = await response.json().catch(() => null);
                if (response.status === 400){
                    switch (data.message) {
                        case "Old password does not match password on file":
                            errors.oldPassword = "Old password does not match password on file";
                            oldPassword = "";
                            break;
                        case "Passwords do not match":
                            errors.confirmPassword = "Password does not match";
                            confirmPassword = "";
                            break;
                        case "Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters":
                            errors.newPassword = "Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters";
                            newPassword = "";
                            confirmPassword = "";
                            break;
                        case "New password can't be the same as old password":
                            errors.newPassword = "New password can't be the same as old password";
                            newPassword = "";
                            confirmPassword = "";
                            break;
                    }
                } else {
                    addToast("Failed to update password ");
                }
            } catch (err) {
                addToast("Failed to update password");
            } finally {
                updatingPassword = false;
            }
        }
    
    /**
     * Sends an image to the image editor
     */
    async function sendToEditor() {
        console.log("recieve file");
        if (!files || files.length === 0) {
            return;
        }
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

            const response = await fetchWithCsrf(
                resolve(`/api/user/pfp` as any),
                {
                    method: "PUT",
                    body: formData,
                },
            );

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
                    pfpInput.click();
                }}
            >
                <i class="bi bi-pencil-square fs-2"></i>
            </button>
        </div>
    </div>

    <div class="flex-grow-1 m-3">
        <form on:submit|preventDefault={updateUser}>
            <div class="mb-4">
                <h5 class="text-muted mb-2">Personal Information</h5>
                <hr class="mt-0" style="opacity: 0.15;" />
                <div class="mb-3">
                    <label for="displayName" class="form-label"
                        >Display Name</label
                    >
                    <input
                            type="text"
                            class="form-control"
                            class:is-invalid={errors.displayName}
                            bind:value={displayName}
                            placeholder="Display Name *"
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
                            placeholder="Email *"
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
                        class="form-select"
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
            </div>
            <div class="mt-5 mb-4">
                <h5 class="text-muted mb-2">Account Security</h5>
                <hr class="mt-0" style="opacity: 0.15;" />
                <div class="d-flex align-items-center justify-content-between">
                    <p class="small text-secondary mb-0">
                        Change your password to keep your account secure.
                    </p>
                    <button
                        type="button"
                        class="btn btn-outline-primary btn-sm"
                        on:click={requestPasswordChange}
                    >
                        Update Password
                    </button>
                </div>
            </div>
            <button type="submit" class="btn btn-primary">Update</button>
            <button
                    type="button"
                    class="btn btn-secondary"
                    on:click={() => {
                goto(resolve("/home/profile"));
            }}>Cancel</button>
    </form>
    </div>
</div>

<!-- update password modal -->
<div
    class="modal fade"
    bind:this={modalElement}
    tabindex="-1"
    aria-hidden="true"
>
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content p-4">
            <div class="modal-header border-0">
                <h5 class="modal-title fw-bold">
                    {currentModalStep === "verify"
                        ? "Verify Your Identity"
                        : "Set New Password"}
                </h5>
            </div>
            <div class="modal-body">
                {#if currentModalStep === "verify"}
                    <div class="text-centre">
                        <p class="text-secondary">
                            We've sent a 6-digit verification code to <br />
                            <span class="text-dark fw-bold">{email}</span>
                        </p>

                        <div id="code-input" class="d-flex gap-2 mt-4 mb-4">
                            <input
                                type="text"
                                class="form-control form-control-lg text-center"
                                maxlength="1"
                                bind:value={digit1}
                                on:input={handleInput}
                                on:keydown={handleKeyDown}
                            />
                            <input
                                type="text"
                                class="form-control form-control-lg text-center"
                                maxlength="1"
                                bind:value={digit2}
                                on:input={handleInput}
                                on:keydown={handleKeyDown}
                            />
                            <input
                                type="text"
                                class="form-control form-control-lg text-center"
                                maxlength="1"
                                bind:value={digit3}
                                on:input={handleInput}
                                on:keydown={handleKeyDown}
                            />
                            <input
                                type="text"
                                class="form-control form-control-lg text-center"
                                maxlength="1"
                                bind:value={digit4}
                                on:input={handleInput}
                                on:keydown={handleKeyDown}
                            />
                            <input
                                type="text"
                                class="form-control form-control-lg text-center"
                                maxlength="1"
                                bind:value={digit5}
                                on:input={handleInput}
                                on:keydown={handleKeyDown}
                            />
                            <input
                                type="text"
                                class="form-control form-control-lg text-center"
                                maxlength="1"
                                bind:value={digit6}
                                on:input={handleInput}
                                on:keydown={handleKeyDown}
                            />
                        </div>

                        {#if codeError}
                            <div class="text-danger small mb-3 animate-fade-in">
                                <i class="bi bi-exclamation-circle-fill me-1"></i> {codeError}
                            </div>
                        {/if}

                        <button
                            class="btn btn-link btn-sm text-decoration-none"
                            on:click={requestPasswordChange}
                            disabled={resendTimer > 0 || isSending}
                        >
                            {#if resendTimer > 0}
                                Resend code in {resendTimer}s
                            {:else if isSending}
                                Sending...
                            {:else}
                                Resend Code
                            {/if}
                        </button>
                    </div>
                {:else}
                    <form on:submit|preventDefault={() => updatePassword()}>
                        <div class="mb-3">
                            <label for="oldPassword" class="form-label small fw-bold text-secondary">Current Password</label>
                            <input type="password" class="form-control {errors.oldPassword ? 'is-invalid' : ''}" id="oldPassword" bind:value={oldPassword}  />
                            {#if errors.oldPassword}
                                <div class="invalid-feedback">
                                    {errors.oldPassword}
                                </div>
                            {/if}
                        </div>
                        <div class="mb-3">
                            <label for="newPassword" class="form-label small fw-bold text-secondary">New Password</label>
                            <input type="password" class="form-control {errors.newPassword ? 'is-invalid' : ''}" id="newPassword" bind:value={newPassword}  />
                            {#if errors.newPassword}
                                <div class="invalid-feedback">
                                    {errors.newPassword}
                                </div>
                            {/if}
                        </div>
                        <div class="mb-3">
                            <label for="confirmPassword" class="form-label small fw-bold text-secondary">Confirm New Password</label>
                            <input type="password" class="form-control {errors.confirmPassword ? 'is-invalid' : ''}" id="confirmPassword" bind:value={confirmPassword}  />
                            {#if errors.confirmPassword}
                                <div class="invalid-feedback">
                                    {errors.confirmPassword}
                                </div>
                            {/if}
                        </div>
                        <button type="submit" class="btn btn-primary w-100 py-2 mt-3" disabled={updatingPassword}>{updatingPassword ? "Updating..." : "Update Password"}</button>
                    </form>
                {/if}
                <button
                        type="button"
                        class="btn btn-secondary w-100 py-2 mt-3"
                        data-bs-dismiss="modal"
                        aria-label="Close"
                >Cancel</button>
            </div>
        </div>
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
                <input
                    accept="image/webp, image/jpeg, image/png, image/gif, image/svg+xml"
                    bind:files
                    bind:this={pfpInput}
                    id="pfp"
                    name="pfp"
                    type="file"
                    class="d-none"
                    on:cancel={() => {pfpCancelButton.click()}}
                    on:change={async () => {
                        await sendToEditor();
                        pfpInput.value = '';
                    }
                    
                    }
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
                    class="btn btn-primary"
                    data-bs-dismiss="modal">Submit</button
                >
                <button
                    type="button"
                    class="btn btn-secondary"
                    data-bs-dismiss="modal"
                    bind:this={pfpCancelButton}
                >Cancel</button>
            </div>
        </div>
    </div>
</div>
