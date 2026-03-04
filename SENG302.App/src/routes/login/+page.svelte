<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { addToast } from "$lib/toast/toast";


    let email = $state("");
    let password = $state("");
    let loading = $state(false);
    let error = $state("");

    /**
     * Handles user login by sending a POST request to the server with the user's email and password.
     * Validates that all fields are filled in before making the request. If login is successful,
     * redirects the user to the profile page. If there is an error, displays an appropriate message.
     */
    async function loginUser() {
        if (!email || !password) {
            error = "Please fill in all fields.";
            addToast(error, "error");
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
                    passwordKey: password,
                }),
            });

            const data = await response.json().catch(() => null);

            if (!response.ok) {
                error = data?.message || "Email or Password is incorrect";
                addToast(error, "error");
                return;
            }
            localStorage.setItem("userEmail", email);
            localStorage.setItem("username", data.username)
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
                    placeholder="Email"
                    bind:value={email}
                    disabled={loading}
            />
        </div>
        <div class="mb-3">
            <input
                    type="password"
                    class="form-control"
                    placeholder="Confirm Password"
                    bind:value={password}
                    disabled={loading}
            />
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