<script lang="ts">
    import { onMount } from "svelte";
    import type { Book } from "$lib/types";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { countries } from "$lib/country/countries";
    import { addToast } from "$lib/toast/toast";

    let user = $state(null);
    let email = $state("");
    let displayName = $state("");
    let selectedCountryCode = $state("");
    let password = $state("");
    let passwordConfirm = $state("");
    let loading = $state(false);
    let error = $state("");

    function validateInputs(): boolean {
        // Check all fields are filled
        if (
            !email ||
            !displayName ||
            !selectedCountryCode ||
            !password ||
            !passwordConfirm
        ) {
            addToast("Please fill in all fields.", "error");
            return false;
        }

        // Check for malformed emails
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
            addToast(
                "Invalid email address. Email must be in the format ‘jane@doe.nz’",
                "error",
            );
            return false;
        }

        // Check for mismatching passwords
        if (password != passwordConfirm) {
            addToast("Passwords do not match", "error");
            return false;
        }

        // Check password validity
        const passwordRegex =
            /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$/;
        if (!passwordRegex.test(password)) {
            addToast(
                "Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters",
                "error",
            );
            return false;
        }

        // Check display name length
        if (displayName.length < 3 || displayName.length > 64) {
            addToast(
                "Display name must be between 3 and 64 characters",
                "error",
            );
            return false;
        }

        // Check display name validity
        const displayNameRegex = /^[A-Za-z\s'-]+$/;
        if (!displayNameRegex.test(displayName)) {
            addToast(
                "Display name must only include letters, spaces, hyphens or apostrophes",
                "error",
            );
            return false;
        }
        return true;
    }
    /**
     * Handles user registration by sending a POST request to the server with the user's details.
     * Validates that all fields are filled in before making the request. If registration is successful,
     * redirects the user to the home page. If there is an error, displays an appropriate message.
     */
    async function registerUser() {
        if (!validateInputs()) return;

        try {
            loading = true;

            const response = await fetchWithCsrf(resolve(`/api/register`), {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    email,
                    displayName,
                    passwordKey: password,
                    country: selectedCountryCode,
                    password,
                    passwordConfirm,
                }),
            });

            const data = await response.json().catch(() => null);

            if (!response.ok) {
                // in case front end form checks were tampered with,
                // we display a toast with the badrequest response
                // from the back end.
                if (response.status == 400)
                    addToast(
                        data?.message ||
                            "User registration is missing information.",
                        "error",
                    );
                else if (response.status == 401) {
                    addToast(
                        data?.message || "This email is already in use.",
                        "error",
                    );
                }

                // if registration fails for some other reason, display
                // a generic error message.
                else {
                    addToast(data?.message || "Registration Failed.", "error");
                }
                return;
            }

            localStorage.setItem("username", displayName);
            localStorage.setItem("userEmail", email);

            addToast("Registration successful. Please log in.", "success");

            goto(resolve(`/login`));

            localStorage.setItem("justRegistered", "true");
            goto(resolve(`/login`));
        } catch (err) {
            addToast(
                "Failed to register user: " + (err as Error).message,
                "error",
            );
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
    <h1 class="text-center mb-4">Register</h1>

    {#if error}
        <div class="alert alert-danger" role="alert">{error}</div>
    {/if}

    <form on:submit|preventDefault={registerUser}>
        <div class="mb-3">
            <input
                type="text"
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
            <select
                class="form-control"
                bind:value={selectedCountryCode}
                disabled={loading}
            >
                <option value="">Select Country</option>
                {#each countries as country}
                    <option value={country.code}>
                        {country.name}
                    </option>
                {/each}
            </select>
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
        <div>
            <button
                type="submit"
                class="btn btn-primary w-100"
                disabled={loading}
            >
                {loading ? "Registering..." : "Register"}
            </button>
        </div>
    </form>
</div>

<style>
    .cursor-pointer {
        cursor: pointer;
    }
</style>
