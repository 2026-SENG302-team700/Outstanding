<script lang="ts">
    // import defaultLogo from '$team-700/SENG302.App/static/defaultProfile.png/';
    import { onMount } from "svelte";
    import type { Modal } from 'bootstrap';
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { countries } from "$lib/country/countries";
    import { addToast } from "$lib/toast/toast";
    import { user } from "$lib/stores/user";
    import regexPatterns from "../../../../../../SENG302.Shared/regexPatterns.json";
    import ProfilePic from "$lib/profilepic/profilepic.svelte";

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
    let userCode = $derived(digit1 + digit2 + digit3 + digit4 + digit5 + digit6);

    let codeError = $state("");
    let errors = $state({
        email: "",
        displayName: "",
    });
    // automatically trigger the checkCode when the length reaches 6
    $effect(() => {
        if (userCode.length === 6) {
            checkCode();
        }
    });

    onMount(async() => {
        retrieveUserData();

        const { Modal : BootstrapModal } = await import('bootstrap');
        
        if (modalElement){
            authModal = new BootstrapModal(modalElement);
        }
        
    });

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
        if (resendTimer > 0 || isSending) return;
        currentModalStep = "verify";
        isSending = true;
        authModal?.show();

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
    if (e.key === "Backspace" && !input.value && input.previousElementSibling) {
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
                user.update(u => ({...u, displayName: updatedUser.displayName}));
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
                codeError = data?.message || `Error ${response.status}: Invalid code.`;
                digit1 = digit2 = digit3 = digit4 = digit5 = digit6 = "";
                const firstInput = document.querySelector('#code-input input') as HTMLInputElement;
                firstInput?.focus();
            } else {
                currentModalStep = "update";
            }
        } catch (err) {
        codeError = "Connection error. Please try again later.";
    }
}
    /**
     * Method used to update the profile picture, confirm the conditions are right and then update
    */
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
    <div class="d-flex flex-column align-items-center m-3">
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
            <div class="mb-4">
                <h5 class="text-muted mb-2">Personal Information</h5>
                <hr class="mt-0" style="opacity: 0.15;">
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
                <hr class="mt-0" style="opacity: 0.15;">
                <div class="d-flex align-items-center justify-content-between">
                    <p class="small text-secondary mb-0">Change your password to keep your account secure.</p>
                    <button type="button" class="btn btn-outline-primary btn-sm" on:click={requestPasswordChange}>
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
            }}>Cancel</button
        >
    </form>
    </div>
</div>

<div class="modal fade" bind:this={modalElement} tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content p-4">
            <div class="modal-header border-0">
                <h5 class="modal-title fw-bold">
                    {currentModalStep === 'verify' ? 'Verify Your Identity' : 'Set New Password'}
                </h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                {#if currentModalStep === 'verify'}
                    <div class="text-centre">
                        <p class="text-secondary">
                            We've sent a 6-digit verification code to <br>
                            <span class="text-dark fw-bold">{email}</span>
                        </p>
                        
                        <div id="code-input" class="d-flex gap-2 mt-4 mb-4">
                            <input type="text" class="form-control form-control-lg text-center" maxlength="1" bind:value={digit1} on:input={handleInput} on:keydown={handleKeyDown}/>
                            <input type="text" class="form-control form-control-lg text-center" maxlength="1" bind:value={digit2} on:input={handleInput} on:keydown={handleKeyDown}/>
                            <input type="text" class="form-control form-control-lg text-center" maxlength="1" bind:value={digit3} on:input={handleInput} on:keydown={handleKeyDown}/>
                            <input type="text" class="form-control form-control-lg text-center" maxlength="1" bind:value={digit4} on:input={handleInput} on:keydown={handleKeyDown}/>
                            <input type="text" class="form-control form-control-lg text-center" maxlength="1" bind:value={digit5} on:input={handleInput} on:keydown={handleKeyDown}/>
                            <input type="text" class="form-control form-control-lg text-center" maxlength="1" bind:value={digit6} on:input={handleInput} on:keydown={handleKeyDown}/>
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
                    <form on:submit|preventDefault={() => console.log("Update logic goes here")}>
                        <div class="mb-3">
                            <label for="oldPassword" class="form-label small fw-bold text-secondary">Current Password</label>
                            <input type="password" class="form-control" id="oldPassword" bind:value={oldPassword} required />
                        </div>
                        <div class="mb-3">
                            <label for="newPassword" class="form-label small fw-bold text-secondary">New Password</label>
                            <input type="password" class="form-control" id="newPassword" bind:value={newPassword} required />
                        </div>
                        <div class="mb-3">
                            <label for="confirmPassword" class="form-label small fw-bold text-secondary">Confirm New Password</label>
                            <input type="password" class="form-control" id="confirmPassword" bind:value={confirmPassword} required />
                        </div>
                        <button type="submit" class="btn btn-primary w-100 py-2 mt-3">Update Password</button>
                    </form>
                {/if}
            </div>
        </div>
    </div>
</div>
