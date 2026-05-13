<script lang="ts">
    import { countries } from "$lib/country/countries";
    let {
        selectedCountryCode = $bindable(""),
        error = "",
        loading = false,
    } = $props();
</script>

<select
    class="form-select"
    class:country-select={!selectedCountryCode}
    class:is-invalid={error}
    bind:value={selectedCountryCode}
    disabled={loading}
    id="country"
>
    <option value="">Select Country *</option>
    {#each countries as country}
        <option value={country.code}>
            {country.name}
        </option>
    {/each}
</select>
{#if error}
    <div class="invalid-feedback">
        {error}
    </div>
{/if}
