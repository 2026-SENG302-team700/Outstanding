<script lang="ts">
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import type { Modal } from "bootstrap";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { addToast } from "$lib/toast/toast";
    import regexPatterns from "../../../../SENG302.Shared/regexPatterns.json";
    import AuthenticatorButton from "$lib/components/authenticator-button.svelte";
    import CancelButton from "$lib/components/cancel-button.svelte";
    import PasswordForm from "$lib/components/password-form.svelte";
    import EmailForm from "$lib/components/email-form.svelte";
    import CodeForm from "$lib/components/code-form.svelte";

    let email = $state("");
    let password = $state("");
    let loading = $state(false);
    let error = $state("");

    const ModalStep = {
        EMAIL_INPUT: "emailInput",
        VERIFY: "verify",
        RESET_PASSWORD: "resetPassword",
    };
    let currentModalStep = $state(ModalStep.EMAIL_INPUT);
    let modalElement: HTMLElement | undefined = $state();
    let authModal: Modal | undefined;
    let resetEmail = $state("");
    // The "reset password" modal has 4 statuses, stored in 'loadingStatus'
    // '0' for when first opening up the modal
    // '1' for sending reset email
    // '2' for sending cancel email
    // '3' for verifying code
    // '4' for updating password
    let loadingStatus = $state(0);
    let confirmResetEmail = $state("");
    let digit1 = $state("");
    let digit2 = $state("");
    let digit3 = $state("");
    let digit4 = $state("");
    let digit5 = $state("");
    let digit6 = $state("");
    let timeRemaining = $state(300);
    let timeRemainingText = $state("05:00");
    let userCode = $derived(
        digit1 + digit2 + digit3 + digit4 + digit5 + digit6,
    );
    let interval;

    let newPassword = $state("");
    let confirmPassword = $state("");

    let errors = $state({
        email: "",
        password: "",
        codeError: "",
        passwordErrorIndicator: false,
        resetEmail: "",
        codeFormEmail: "",
        newPassword: "",
        confirmPassword: "",
    });

    onMount(async () => {
        const { Modal: BootstrapModal } = await import("bootstrap");

        if (modalElement) {
            authModal = new BootstrapModal(modalElement);
        }
    });

    /**
     * Format a given number of seconds into a user friendly readable time for the countdown timer
     * @param seconds
     */
    function formatTime(seconds: number) {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins.toString().padStart(2, "0")}:${secs.toString().padStart(2, "0")}`;
    }

    /**
     * Starts the countdown for the timer
     */
    function startTimerCountdown() {
        timeRemaining = 300;
        clearInterval(interval);

        interval = setInterval(() => {
            timeRemaining--;
            timeRemainingText = formatTime(timeRemaining);
            if (timeRemaining <= 0) clearInterval(interval);
        }, 1000);
    }

    /**
     * Clears input fields containing digits if code is resent
     */
    function clearResetCodeModalFields() {
        digit1 = digit2 = digit3 = digit4 = digit5 = digit6 = "";
        errors.codeError = "";
    }

    /**
     * Handles user login by sending a POST request to the server with the user's email and password.
     * Validates that all fields are filled in before making the request. If login is successful,
     * redirects the user to the profile page. If there is an error, displays an appropriate message.
     */
    async function loginUser() {
        let valid = true;
        // Reset errors
        errors = {
            email: "",
            password: "",
            passwordErrorIndicator: false,
        };

        // Check email format
        const emailRegex = new RegExp(regexPatterns.user.email);
        if (email && !emailRegex.test(email)) {
            errors.email =
                "Invalid email address. Email must be in the format ‘jane@doe.nz’";
        }

        // Validate inputs
        if (!email) {
            errors.email = "Email is required.";
            valid = false;
        }

        if (!password) {
            errors.password = "Password is required.";
            valid = false;
        }

        if (!valid) {
            return;
        }

        try {
            loading = true;
            error = "";
            const response = await fetchWithCsrf(resolve(`/api/login`), {
                method: "POST",
                credentials: "include",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    email,
                    passwordString: password,
                }),
            });

            const data = await response.json().catch(() => null);

            if (response.status === 404 || response.status === 401) {
                password = "";
                errors.email = data?.message || "Invalid email or password.";
                errors.passwordErrorIndicator = true;
                return;
            }

            if (response.status === 400) {
                errors.email = data.message;
                return;
            }

            if (!response.ok) {
                password = "";
                error = data?.message || "Failed to login user: !response.ok";
                addToast(error, "error");
                console.error("!response.ok outside of 400, 401 and 404.");
                return;
            }
            // If login is succesful then redirect the user to the home page and show a toast notification for NFR
            addToast(`Welcome to Outstanding ${data?.message}!`);
            goto(resolve(`/home`));
        } catch (err) {
            password = "";
            error = "Failed to login user: " + (err as Error).message;
            addToast(error, "error");
            console.error(err);
        } finally {
            loading = false;
        }
    }

    /**
     * Send the email for one time code to the backend and starts the timer
     * Code will be sent to the email if it is valid and the
     * modal will progress to the next stage
     * shows relavent errors otherwise
     */
    async function sendVerificationCode() {
        // reset error
        errors.resetEmail = "";
        loadingStatus = 1;

        // Validate email on front end
        let valid = true;

        if (!resetEmail) {
            errors.resetEmail = "Email is required.";
            valid = false;
        }

        const emailRegex = new RegExp(regexPatterns.user.email);
        if (resetEmail && !emailRegex.test(resetEmail)) {
            errors.resetEmail =
                "Invalid email address. Email must be in the format ‘jane@doe.nz’";
            valid = false;
        }

        if (!valid) {
            loadingStatus = 0;
            return;
        }

        try {
            const response = await fetchWithCsrf(
                resolve(`/api/user/password/reset/code/generation`),
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        email: resetEmail,
                    }),
                },
            );

            if (response.ok) {
                addToast("Password reset email sent", "success");

                currentModalStep = ModalStep.VERIFY;

                startTimerCountdown();
                clearResetCodeModalFields();
            } else {
                const data = await response.json().catch(() => null);
                errors.resetEmail = data?.message || "Failed to send code.";
            }
        } catch (err) {
            errors.resetEmail = "Failed to send code " + (err as Error).message;
        }

        loadingStatus = 0;
    }

    /**
     * Cancel the password reset code
     */
    async function cancelCode() {
        errors.codeError = "";
        loadingStatus = 2;

        const response = await fetchWithCsrf(
            `/api/user/password/reset/code/cancel`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    Email: resetEmail,
                }),
            },
        );

        const data = await response.json().catch(() => null);

        if (!response.ok) {
            errors.codeError = data?.message;
        } else {
            currentModalStep = ModalStep.EMAIL_INPUT;
            authModal.hide();
        }

        loadingStatus = 0;
    }

    /**
     * Function that is called when all digits are entered.
     * Checks that the code is valid
     */
    async function checkCode() {
        loadingStatus = 3;
        try {
            errors.codeError = "";
            errors.codeFormEmail = "";

            if (resetEmail !== confirmResetEmail) {
                errors.codeFormEmail = "Emails do not match";
                loadingStatus = 0;
                return;
            }

            const response = await fetchWithCsrf(
                resolve(`/api/user/password/reset/code/validation`),
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        Email: confirmResetEmail,
                        Code: userCode,
                    }),
                },
            );

            const data = await response.json().catch(() => null);

            if (!response.ok) {
                errors.codeError = data?.message;
                digit1 = digit2 = digit3 = digit4 = digit5 = digit6 = "";
                const firstInput = document.querySelector(
                    "#code-input input",
                ) as HTMLInputElement;
                firstInput?.focus();
            } else {
                currentModalStep = ModalStep.RESET_PASSWORD;
            }
        } catch (err) {
            errors.codeError = "Connection error. Please try again later.";
        }

        loadingStatus = 0;
    }

    /**
     * Called when the forgot password is clicked, opens the modal
     * for entering the email.
     */
    async function requestNewPassword() {
        errors.resetEmail = "";
        errors.codeFormEmail = "";

        resetEmail = "";
        confirmResetEmail = "";
        newPassword = "";
        confirmResetEmail = "";

        currentModalStep = ModalStep.EMAIL_INPUT;
        authModal?.show();
    }

    /**
     * Checks passwords match and are strong
     */
    function validateUpdatePasswordInputs() {
        var isValid = true;

        // checks fields are filled
        if (!newPassword) {
            errors.newPassword = "Field is required";
            isValid = false;
        }
        if (!confirmPassword) {
            errors.confirmPassword = "Field is required";
            isValid = false;
        }
        // checks passwords match
        if (newPassword !== confirmPassword) {
            isValid = false;
            errors.confirmPassword = "Passwords do not match";
            confirmPassword = "";
        }

        // check password is valid
        const passwordRegex = new RegExp(regexPatterns.user.password);
        if (!passwordRegex.test(newPassword)) {
            isValid = false;
            errors.newPassword =
                "Password must be at least 8 characters long including at least one of each " +
                "uppercase, lowercase, numbers and special characters";
            newPassword = "";
            confirmPassword = "";
        }
        return isValid;
    }

    /**
     * Validates and performs the update to the users password
     */
    async function resetPassword() {
        errors.newPassword = "";
        errors.confirmPassword = "";

        if (!validateUpdatePasswordInputs()) {
            return;
        }

        try {
            loadingStatus = 4;
            const response = await fetchWithCsrf(
                resolve(`/api/user/password/reset`),
                {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        newPassword: newPassword,
                        newPasswordConfirm: confirmPassword,
                    }),
                    credentials: "include",
                },
            );
            if (response.ok) {
                addToast("New password updated successfully");
                authModal.hide();
                return;
            }

            const data = await response.json().catch(() => null);
            if (response.status === 400) {
                switch (data.message) {
                    case "Passwords do not match":
                        errors.confirmPassword = "Password does not match";
                        confirmPassword = "";
                        break;
                    case "Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters":
                        errors.newPassword =
                            "Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters";
                        newPassword = "";
                        confirmPassword = "";
                        break;
                }
            } else {
                addToast("Failed to update password", "error");
            }
        } catch (err) {
            addToast("Failed to update password", "error");
        } finally {
            loadingStatus = 0;
        }
    }
</script>

<div class="container">
    <div class="mb-3">
        <CancelButton path="/"></CancelButton>
    </div>
    <h1 class="text-center mb-4">Login</h1>

    <form
        onsubmit={(e) => {
            e.preventDefault();
            loginUser();
        }}
    >
        <div class="mb-3">
            <EmailForm error={errors.email} {loading} bind:email />
        </div>
        <div class="mb-3">
            <PasswordForm bind:password error={errors.password} {loading} />
        </div>
        <div class="mb-3">
            <AuthenticatorButton buttonType={"login"} />
        </div>
        <div class="mb-3">
            <button
                class="btn btn-primary w-100"
                hidden={errors.email !=
                    "Account is not validated yet, check your emails."}
                onclick={() => goto(resolve("/register/verification"))}
            >
                Verify Email
            </button>
        </div>
    </form>
    <div class="mb-3 mt-3">
        <button
            class="btn btn-link btn-sm text-decoration-none"
            onclick={requestNewPassword}>Forgot Password?</button
        >
    </div>
</div>

<!-- Reset password modal -->
<div
    class="modal fade"
    role="dialog"
    data-bs-backdrop="static"
    data-bs-keyboard="false"
    bind:this={modalElement}
    tabindex="-1"
    aria-hidden="true"
>
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content p-4">
            <form
                onsubmit={() => {
                    if (loadingStatus == 0) {
                        if (currentModalStep === ModalStep.EMAIL_INPUT) {
                            sendVerificationCode();
                        } else if (currentModalStep === ModalStep.VERIFY) {
                            checkCode();
                        } else if (
                            currentModalStep === ModalStep.RESET_PASSWORD
                        ) {
                            resetPassword();
                        } else {
                            authModal.hide();
                        }
                    }
                }}
            >
                <div class="modal-header border-0">
                    <h5 class="modal-title fw-bold">
                        {currentModalStep === ModalStep.EMAIL_INPUT
                            ? "Request Reset Code"
                            : currentModalStep === ModalStep.VERIFY
                              ? "Verify Your Identity"
                              : "Reset Password"}
                    </h5>
                </div>

                <div class="modal-body">
                    {#if currentModalStep === ModalStep.EMAIL_INPUT}
                        <div class="text-center">
                            <p class="text-secondary">
                                We'll send a verification code to your email
                            </p>

                            <div class="mb-3 text-start">
                                <input
                                    id="email"
                                    class="form-control"
                                    placeholder="Email *"
                                    bind:value={resetEmail}
                                />
                            </div>
                            {#if errors.resetEmail}
                                <div class="text-danger mt-1">
                                    {errors.resetEmail}
                                </div>
                            {/if}
                        </div>
                    {:else if currentModalStep === ModalStep.VERIFY}
                        <div class="text-centre">
                            <p class="small">
                                Please check your inbox and enter the
                                verification code below to verify your email
                                address. The code will expire in <strong
                                    >{timeRemainingText}</strong
                                >
                            </p>
                            <div>
                                <p class="mb-2 small">
                                    Please re-enter your email here:
                                </p>
                                <EmailForm
                                    {loading}
                                    error={errors.codeFormEmail}
                                    bind:email={confirmResetEmail}
                                />
                            </div>
                            <div>
                                <p class="m-0 small">
                                    Please enter the verification code here:
                                </p>
                                <CodeForm
                                    bind:digit1
                                    bind:digit2
                                    bind:digit3
                                    bind:digit4
                                    bind:digit5
                                    bind:digit6
                                    error={errors.codeError}
                                />
                            </div>
                        </div>
                    {:else}
                        <div class="mb-3">
                            <label
                                for="newPassword"
                                class="form-label small fw-bold text-secondary"
                            >
                                New Password *
                            </label>
                            <PasswordForm
                                bind:password={newPassword}
                                error={errors.newPassword}
                            />
                        </div>

                        <div class="mb-3">
                            <label
                                for="newPasswordRepeat"
                                class="form-label small fw-bold text-secondary"
                            >
                                Confirm New Password *
                            </label>
                            <PasswordForm
                                bind:password={confirmPassword}
                                error={errors.confirmPassword}
                            />
                        </div>
                    {/if}
                </div>
                <div class="modal-footer">
                    <button
                        type="submit"
                        class="btn btn-primary w-100"
                        disabled={loadingStatus != 0}
                    >
                        {#if loadingStatus == 1}
                            Sending...
                        {:else if loadingStatus == 3}
                            Verifying...
                        {:else if loadingStatus == 4}
                            Updating Password...
                        {:else if currentModalStep === ModalStep.VERIFY}
                            Reset Password
                        {:else if currentModalStep === ModalStep.EMAIL_INPUT}
                            Get reset code
                        {:else if currentModalStep === ModalStep.RESET_PASSWORD}
                            Update Password
                        {/if}
                    </button>
                    <button
                        type="button"
                        class="btn btn-secondary w-100"
                        disabled={loadingStatus != 0}
                        onclick={() => {
                            if (currentModalStep === ModalStep.RESET_PASSWORD) {
                                cancelCode();
                            } else if (currentModalStep === ModalStep.VERIFY) {
                                cancelCode();
                            } else {
                                authModal.hide();
                            }
                        }}
                    >
                        {#if loadingStatus == 2}
                            Cancelling...
                        {:else}
                            Cancel
                        {/if}
                    </button>
                </div>
            </form>
        </div>
    </div>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
