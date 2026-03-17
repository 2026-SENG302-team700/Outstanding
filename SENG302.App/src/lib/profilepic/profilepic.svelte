<script lang="ts">
    export let pfpUrl: string | null = null;
    export let size: 'small' | 'large' = 'small';
</script>

<div class="">
    {#if pfpUrl}
        <img
                class="profile-image {size}"
                src={pfpUrl}
        />
    {:else}
        <i class="bi bi-person-circle icon {size}" />
    {/if}

    <style>
        .profile-image {
            object-fit: cover;
            border-radius: 50%;
        }
        .profile-image.small {
            width: 100%;
            height: 100%;
        }
        .profile-image.large {
            width: 150px;
            height: 150px;
        }
        .icon.small {
            font-size: 40px;
            line-height: 1;
        }
        .icon.large {
            font-size: 150px;
            line-height: 1;
        }
    </style>

</div>