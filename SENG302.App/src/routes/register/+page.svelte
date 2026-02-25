<script lang="ts">
    import { onMount } from "svelte";
    import type { Book } from "$lib/types";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";

    let user = $state(null);
    let email = $state("");
    let displayName = $state("");
    let country = $state("");
    let password = $state("");
    let passwordConfirm = $state("");
    let loading = $state(false);
    let error = $state("");

    /**
     * Handles user registration by sending a POST request to the server with the user's details.
     * Validates that all fields are filled in before making the request. If registration is successful,
     * redirects the user to the home page. If there is an error, displays an appropriate message.
     */
    async function registerUser() {
        if (
            !email ||
            !displayName ||
            !country ||
            !password ||
            !passwordConfirm
        ) {
            error = "Please fill in all fields.";
            return;
        }

        try {
            loading = true;
            error = "";
            const response = await fetchWithCsrf(resolve(`/api/register`), {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    email,
                    displayName,
                    country,
                    password,
                    passwordConfirm,
                }),
            });

            const data = await response.json().catch(() => null);

            if (!response.ok) {
                error = data?.message || "Registration failed.";
                return;
            }

            goto(resolve(`/home/${data.userId}`));
        } catch (err) {
            error = "Failed to register user: " + (err as Error).message;
            console.error(err);
        } finally {
            loading = false;
        }
    }
</script>

<div class="container">
    <h1 class="text-center mb-4">Register</h1>

    {#if error}
        <div class="alert alert-danger" role="alert">{error}</div>
    {/if}

    <form onsubmit={registerUser}>
        <div class="mb-3">
            <input
                type="email"
                class="form-control"
                placeholder="Email"
                bind:value={email}
                disabled={loading}
            />
        </div>
        <div class="mb-3">
            <input
                type="text"
                class="form-control"
                placeholder="Display Name"
                bind:value={displayName}
                disabled={loading}
            />
        </div>
        <div class="mb-3">
            <input
                type="text"
                class="form-control"
                placeholder="Country"
                bind:value={country}
                disabled={loading}
            />
        </div>
        <div class="mb-3">
            <input
                type="password"
                class="form-control"
                placeholder="Password"
                bind:value={password}
                disabled={loading}
            />
        </div>
        <div class="mb-3">
            <input
                type="password"
                class="form-control"
                placeholder="Confirm Password"
                bind:value={passwordConfirm}
                disabled={loading}
            />
        </div>
        <button type="submit" class="login-button" disabled={loading}>
            {loading ? "Registering..." : "Register"}
        </button>
    </form>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
