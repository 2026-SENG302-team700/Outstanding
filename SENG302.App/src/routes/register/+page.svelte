<script lang="ts">
  import { onMount } from "svelte";
  import type { Book } from "$lib/types";
  import { goto } from "$app/navigation";
  import { resolve } from "$app/paths";
  import { fetchWithCsrf } from "$lib/csrf";
  import { countries } from "$lib/countries";

  let user = $state(null);
  let email = $state("");
  let displayName = $state("");
  let selectedCountryCode = $state("");
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
      !selectedCountryCode ||
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
          passwordKey: password,
          country: selectedCountryCode,
        }),
      });

      const data = await response.json().catch(() => null);

      if (!response.ok) {
        error = data?.message || "Registration failed.";
        return;
      }

      localStorage.setItem("userEmail", email);
      goto(resolve(`/home`));
    } catch (err) {
      error = "Failed to register user: " + (err as Error).message;
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
  <h1 class="text-center mb-4">Register</h1>

  {#if error}
    <div class="alert alert-danger" role="alert">{error}</div>
  {/if}

  <form on:submit|preventDefault={registerUser}>
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
      <select
        class="form-control"
        bind:value={selectedCountryCode}
        disabled={loading}
        required
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
      <button type="submit" class="btn btn-primary w-100" disabled={loading}>
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
