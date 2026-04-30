<script lang="ts">
    let {
        id = $bindable(""),
        checked = $bindable()
    } = $props();
</script>

<div class="form-check form-switch">
    <input class ="form-check-input"
           type="checkbox"
           role="switch"
           id={id}
           bind:checked
    />
    <label class="form-check-label">
        {checked? "Enabled" : "Disabled"}
    </label>
</div>

<style>
    .form-check-label {
        min-width: 4rem;
        text-align: center;
    }
</style>