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
    
    function clearFields() {
        email = "";
        password = "";
    }
    
    async function loginUser() {
        let valid = true;
        // Reset errors
        errors = {
            email: "",
            password: "",
        };

        // Check email format
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (email && !emailRegex.test(email)) {
            errors.email =
                "Invalid email address. Email must be in the format ‘jane@doe.nz’";
        }

        // Validate inputs
        if (!email) {
            email = "";
            errors.email = "Email is required.";
            valid = false;
        }

        if (!password) {
            password = "";
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

            if (response.status === 404 || response.status === 401) {
                clearFields();
                error = "Invalid email or password.";
                addToast(error, "error");
                return;
            }

            if (response.status === 400) {
                email = "";
                errors.email =
                    "Invalid email address. Email must be in the format ‘jane@doe.nz’";
                return;
            }

            const data = await response.json().catch(() => null);
            if (!response.ok) {
                clearFields();
                error = data?.message || "Invalid email or password.";
                addToast(error, "error");
                return;
            }
            goto(resolve(`/home`));
        } catch (err) {
            clearFields();
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
                class:is-invalid={errors.email}
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
                class:is-invalid={errors.password}
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
