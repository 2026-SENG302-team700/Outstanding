<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { addToast } from "$lib/toast/toast";
    import regexPatterns from "../../../../SENG302.Shared/regexPatterns.json";
    import SubmitButton from "$lib/components/submit-button.svelte";
    import CancelButton from "$lib/components/cancel-button.svelte";

    let email = $state("");
    let password = $state("");
    let loading = $state(false);
    let error = $state("");

    let errors = $state({
        email: "",
        password: "",
        passwordErrorIndicator: false,
    });

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
            const response = await fetchWithCsrf(`/api/login`, {
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
            goto(resolve(`/home`), { pushState: '' });
        } catch (err) {
            password = "";
            error = "Failed to login user: " + (err as Error).message;
            addToast(error, "error");
            console.error(err);
        } finally {
            loading = false;
        }
    }
</script>

<div class="container">
    <div class="mb-3">
        <CancelButton path="/"></CancelButton>
    </div>
    <h1 class="text-center mb-4">Login</h1>

    <form on:submit|preventDefault={loginUser}>
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
            <SubmitButton buttonType={"login"} />
        </div>
        <div class="mb-3">
            <button
                class="btn btn-primary w-100"
                hidden={errors.email !=
                    "Account is not validated yet, check your emails."}
                    on:click={() => goto(resolve("/register/verification"))}
            >
                Verify Email
            </button>
        </div>
    </form>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
