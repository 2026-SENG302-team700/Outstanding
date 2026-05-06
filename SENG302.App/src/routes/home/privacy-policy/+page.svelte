<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
</script>

<div class="container py-4">
    <div class="card shadow-sm p-4">

        <h1 class="mb-3">Privacy Policy</h1>

        <p>
            content...
        </p>

        <hr />

        <h1>Cookie Management</h1>

        <p>
            content...
        </p>

        <hr />

        <button
                class="btn btn-primary mt-3"
                on:click={() => goto(resolve("/home"))}
        >
            Back to Home
        </button>

    </div>
</div>

<style>
    h1, h4, h5 {
        font-weight: 600;
    }

    ul {
        margin-bottom: 1rem;
    }

    .card {
        border-radius: 12px;
    }
</style>