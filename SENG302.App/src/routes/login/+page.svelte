<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { addToast } from "$lib/toast/toast";

    let email = $state("");
    let password = $state("");
    let loading = $state(false);
    let error = $state("");

    let errors = $state({
        email: "",
        password: "",
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
        };

        // Check email format
        const emailRegex = /^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$/;
        if (email && !emailRegex.test(email)) {
            errors.email =
                "Invalid email address. Email must be in the format ‘jane@doe.nz’";
            return;
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

            if (response.status === 404) {
                error = "Invalid email or password.";
                addToast(error, "error");
                return;
            }

            if (response.status === 400) {
                errors.email =
                    "Invalid email address. Email must be in the format ‘jane@doe.nz’";
                return;
            }

            const data = await response.json().catch(() => null);
            if (!response.ok) {
                error = data?.message || "Invalid email or password.";
                addToast(error, "error");
                return;
            }
            // If login is succesful then redirect the user to the home page and show a toast notification for NFR
            addToast(`Welcome to Outstanding ${data?.message}!`)
            goto(resolve(`/home`));
        } catch (err) {
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
        <button
            type="button"
            class="btn btn-secondary"
            on:click={() => goto(resolve("/"))}>Cancel</button
        >
    </div>
    <h1 class="text-center mb-4">Login</h1>

    <form on:submit|preventDefault={loginUser}>
        <div class="mb-3">
            <input
                type="type"
                class="form-control"
                class:error={errors.email}
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
                placeholder="Password *"
                bind:value={password}
                disabled={loading}
            />
            {#if errors.password}
                <div class="text-danger mt-1">{errors.password}</div>
            {/if}
        </div>
        <div>
            <button
                type="submit"
                class="btn btn-primary w-100"
                disabled={loading}
            >
                {loading ? "Loading..." : "Login"}
            </button>
        </div>
    </form>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
