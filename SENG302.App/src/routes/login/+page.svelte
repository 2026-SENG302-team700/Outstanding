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

    let email = $state("");
    let password = $state("");
    let loading = $state(false);
    let error = $state("");
    let currentModalStep = $state("verify");
    let modalElement: HTMLElement | undefined = $state();
    let authModal: Modal | undefined;
    let digit1 = $state("");
    let digit2 = $state("");
    let digit3 = $state("");
    let digit4 = $state("");
    let digit5 = $state("");
    let digit6 = $state("");
    let timeRemaining = $state(300);
    let timeRemainingText = $state("0");
    let userCode = $derived(
        digit1 + digit2 + digit3 + digit4 + digit5 + digit6,
    );
    let interval;

    let errors = $state({
        email: "",
        password: "",
        codeError: "",
        passwordErrorIndicator: false,
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

    /**
     * Function that is called when all digits are entered.
     * 
     * Checks that the code is valid
     */
    async function checkCode() {
        try {
            errors.codeError = "";
            
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
            
            
            const data = await response.json().catch(() => null);
            
            if (!response.ok) {
                errors.codeError =
                    data?.message || `Error ${response.status}: Invalid Code.`;
                digit1 = digit2 = digit3 = digit4 = digit5 = digit6 = "";
                const firstInput = document.querySelector(
                    "#code-input input",
                ) as HTMLInputElement;
                firstInput?.focus();
                clearInterval(interval)
                timeRemainingText = '0:00'
            } else {
                currentModalStep = "update";
                // Remove before merging
                authModal.hide();
            }
        } catch (err) {
            console.log(err)
            errors.codeError = "Connection error. Please try again later.";
        }
    }

    /**
     * Called when the forgot password is clicked, opens the modal
     * for the one time code, starts the timer and sends the one time code to the users email
     */
    async function requestNewPassword() {
        currentModalStep = "verify";
        
        authModal?.show();

        startTimerCountdown();
        clearResetCodeModalFields();

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
            } else {
                const data = await response.json().catch(() => null);
                console.log(data)
                console.log(data.message)
                errors.codeError = data?.message || "Failed to send code.";
            }
        } catch (err) {
            errors.codeError = "Failed to send email: " + (err as Error).message;
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
            <input
                type="type"
                class="form-control"
                class:error={errors.email}
                class:is-invalid={errors.email || error}
                placeholder="Email *"
                bind:value={email}
                disabled={loading}
            />
            {#if errors.email}
                <div class="text-danger mt-1">{errors.email}</div>
            {/if}
        </div>
        <div class="mb-3">
            <input
                type="password"
                class="form-control"
                class:error={errors.password}
                class:is-invalid={errors.password ||
                    errors.passwordErrorIndicator ||
                    error}
                placeholder="Password *"
                bind:value={password}
                disabled={loading}
            />
            {#if errors.password}
                <div class="text-danger mt-1">{errors.password}</div>
            {/if}
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
            onclick={() => requestNewPassword(false)}>Forgot Password?</button
        >
    </div>
</div>

<!-- Reset password modal -->
<div
    class="modal fade"
    bind:this={modalElement}
    tabindex="-1"
    aria-hidden="true"
    id="resetCodeModal"
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
                            We've sent a 6 digit verification code to <br />
                            <span class="text-dark fw-bold">{email}</span>
                        </p>
                        <p class="small">
                            Please check your inbox and enter the verification
                            code below to verify your email address. The code
                            will expire in <strong>{timeRemainingText}</strong>
                        </p>
                        <div id="code-input" class="d-flex gap-2 mt-4 mb-4">
                            <input
                                type="text"
                                class="form-control form-control-lg text-center"
                                maxlength="1"
                                bind:value={digit1}
                                oninput={handleInput}
                                onkeydown={handleKeyDown}
                            />
                            <input
                                type="text"
                                class="form-control form-control-lg text-center"
                                maxlength="1"
                                bind:value={digit2}
                                oninput={handleInput}
                                onkeydown={handleKeyDown}
                            />
                            <input
                                type="text"
                                class="form-control form-control-lg text-center"
                                maxlength="1"
                                bind:value={digit3}
                                oninput={handleInput}
                                onkeydown={handleKeyDown}
                            />
                            <input
                                type="text"
                                class="form-control form-control-lg text-center"
                                maxlength="1"
                                bind:value={digit4}
                                oninput={handleInput}
                                onkeydown={handleKeyDown}
                            />
                            <input
                                type="text"
                                class="form-control form-control-lg text-center"
                                maxlength="1"
                                bind:value={digit5}
                                oninput={handleInput}
                                onkeydown={handleKeyDown}
                            />
                            <input
                                type="text"
                                class="form-control form-control-lg text-center"
                                maxlength="1"
                                bind:value={digit6}
                                oninput={handleInput}
                                onkeydown={handleKeyDown}
                            />
                        </div>
                        {#if errors.codeError}
                            <div class="text-danger small mb-3 animate-fade-in">
                                <i class="bi bi-exclamation-circle-fill me-1"
                                ></i>
                                {errors.codeError}
                            </div>
                        {/if}
                    </div>
                {:else}{/if}
            </div>
            <div class="modal-footer">
                <button class="btn btn-primary w-100" onclick={checkCode}>
                    {#if currentModalStep === "verify"}
                        Reset Password
                    {/if}    
                </button>
            </div>
        </div>
    </div>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
