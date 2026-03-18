<script lang="ts">
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
    let errors = $state({
        email: "",
        displayName: "",
        country: "",
        password: "",
        passwordConfirm: "",
    });

    function validateInputs(): boolean {
        let valid = true;
        // Reset errors
        errors = {
            email: "",
            displayName: "",
            country: "",
            password: "",
            passwordConfirm: "",
        };

        // Check email format
        const emailRegex = new RegExp(
            "^(?=.{5,254}$)(?=.{1,64}@)[a-zA-Z0-9!#$%&‘*+–/=?^_`{|}~]+" +
                "(\\.[a-zA-Z0-9!#$%&‘*+–/=?^_`{|}~]+)*" +
                "@(?=.{3,255}$)([A-Za-z0-9]+[-]*)+" +
                "(\\.([-]*[A-Za-z0-9]+)+)+$",
        );
        if (!emailRegex.test(email) && email) {
            (errors.email =
                "Invalid email address. Email must be in the format ‘jane@doe.nz’"),
                "error";
            valid = false;
        }

        // Check if passwords match
        if (password !== passwordConfirm && password && passwordConfirm) {
            errors.passwordConfirm = "Passwords do not match.";
            valid = false;
        }

        // Check display name length
        if (displayName.length < 3 || displayName.length > 64) {
            errors.displayName =
                "Display name must be between 3 and 64 characters.";
            valid = false;
        }

        // Check display name validity
        const displayNameRegex = /^[\p{L}0-9\s'-]+$/u;
        if (!displayNameRegex.test(displayName)) {
            errors.displayName =
                "Display name must only include letters, spaces, hyphens or apostrophes.";
            valid = false;
        }

        // Check for empty fields
        if (!email) {
            errors.email = "Email is required.";
            valid = false;
        }

        if (!displayName) {
            errors.displayName = "Display name is required.";
            valid = false;
        }

        if (!selectedCountryCode) {
            errors.country = "Please select a country.";
            valid = false;
        }

        if (!password) {
            errors.password = "Password is required.";
            valid = false;
        }

        if (!passwordConfirm) {
            errors.passwordConfirm = "Please confirm your password.";
            valid = false;
        }

        // Check if passwords match
        if (password !== passwordConfirm && password && passwordConfirm) {
            // actual error stuff
            errors.passwordConfirm = "Passwords do not match.";
            valid = false;
        }

        // Check password validity
        const passwordRegex =
            /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$/;
        if (!passwordRegex.test(password) && password) {
            // actual error stuff
            errors.password =
                "Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters";
            password = "";
            passwordConfirm = "";
            valid = false;
        }

        // Check display name length
        if (
            (displayName.length < 3 || displayName.length > 64) &&
            displayName
        ) {
            errors.displayName =
                "Display name must be between 3 and 64 characters.";
            valid = false;
        }

        return valid;
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
                    passwordString: password,
                    passwordConfirm: password,
                    country: selectedCountryCode,
                }),
            });

            const data = await response.json().catch(() => null);

            console.log(data);

            if (!response.ok) {
                // in case front end form checks were tampered with,
                // we display a toast with the badrequest response
                // from the back end.

                switch (data.errorType) {
                    // check for duplicate email, throws regular error rather than "something went wrong"
                    case "DuplicateEmailException":
                        email = "";
                        errors.email =
                            data?.message ||
                            "This email address is already in use by another account.";
                        break;
                    default:
                        addToast(data?.message || "An error occured.", "error");
                        break;
                }
                return;
            }
            // set email in local storage for validation page
            localStorage.setItem("email", email)
            addToast("Registration successful. Please log in.", "success");
            goto(resolve(`/register/verification`));
        } catch (err) {
            console.error(err);
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

    <form on:submit|preventDefault={registerUser}>
        <div class="mb-3">
            <input
                type="text"
                class="form-control"
                class:is-invalid={errors.email}
                placeholder="Email *"
                bind:value={email}
                disabled={loading}
            />
            {#if errors.email}
                <div class="invalid-feedback">
                    {errors.email}
                </div>
            {/if}
        </div>
        <div class="mb-3">
            <input
                type="text"
                class="form-control"
                class:is-invalid={errors.displayName}
                placeholder="Display Name *"
                bind:value={displayName}
                disabled={loading}
            />
            {#if errors.displayName}
                <div class="invalid-feedback">
                    {errors.displayName}
                </div>
            {/if}
        </div>
        <div class="mb-3">
            <select
                class="form-control"
                class:country-select={!selectedCountryCode}
                class:is-invalid={errors.country}
                bind:value={selectedCountryCode}
                disabled={loading}
            >
                <option value="">Select Country *</option>
                {#each countries as country}
                    <option value={country.code}>
                        {country.name}
                    </option>
                {/each}
            </select>
            {#if errors.country}
                <div class="invalid-feedback">
                    {errors.country}
                </div>
            {/if}
        </div>
        <div class="mb-3">
            <input
                type="password"
                class="form-control"
                class:is-invalid={errors.password}
                placeholder="Password *"
                bind:value={password}
                disabled={loading}
            />
            {#if errors.password}
                <div class="invalid-feedback">
                    {errors.password}
                </div>
            {/if}
        </div>
        <div class="mb-3">
            <input
                type="password"
                class="form-control"
                class:is-invalid={errors.passwordConfirm}
                placeholder="Confirm Password *"
                bind:value={passwordConfirm}
                disabled={loading}
            />
            {#if errors.passwordConfirm}
                <div class="invalid-feedback">
                    {errors.passwordConfirm}
                </div>
            {/if}
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

    .country-select {
        color: #666666; /* Bootstrap's placeholder color */
    }
</style>
