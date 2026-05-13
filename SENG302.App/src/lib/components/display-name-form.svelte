<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";

    let {
        displayName = $bindable(""),
        error = "",
        loading = false,
        registering = false,
    } = $props();
    let displayPolicyLink = $state(false);

    /**
     * If one of the state variable changes, check if the error is related to profanities in the display name.
     * If so, then change the displayPolicyLink variable to thus make visible, the link to content policy page.
     */
    $effect(() => {
        if (
            error === "Display Name Contains Profanities! Remove Profanities!"
        ) {
            displayPolicyLink = true;
        }
    });
</script>

<div class="mb-3">
    <input
        type="text"
        class="form-control"
        class:is-invalid={error}
        autocomplete="name"
        placeholder={"Display Name *"}
        bind:value={displayName}
        disabled={loading}
    />
    {#if error}
        <div class="invalid-feedback">
            {error}
            {#if displayPolicyLink}
                <a
                    role="button"
                    onclick={() => {
                        registering
                            ? goto(
                                  resolve(
                                      "/policies/content-policy?from=register",
                                  ),
                              )
                            : goto(
                                  resolve("/policies/content-policy?from=home"),
                              );
                    }}
                    class="text-decoration-underline"
                >
                    Content Policy
                </a>
            {/if}
        </div>
    {/if}
</div>

<style>
    .policy-link {
        color: #1d4ed8;
        text-decoration: underline;
    }
</style>
