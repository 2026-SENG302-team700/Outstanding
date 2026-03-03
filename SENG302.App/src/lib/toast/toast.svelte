<script lang="ts">
    import { toasts, type Toast } from "./toast";
</script>

<div class="toast-container" style="position: fixed; top: 1rem; right: 1rem; z-index: 1000;">
    {#each $toasts as toast}
        <div class="toast-notification" class:success={toast.type === "success"} class:error={toast.type === "error"} class:info={toast.type === "info"}>
            {toast.message}
        </div>
    {/each}
</div>

<style>
    .toast-notification {
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 0.25rem;
        margin-bottom: 0.5rem;
        box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
        font-weight: bold;
        opacity: 0.9;
    }

    .toast-notification.success {
        background-color: #28a745;
    }

    .toast-notification.error {
        background-color: #dc3545;
    }

    .toast-notification.info {
        background-color: #17a2b8;
    }
</style>