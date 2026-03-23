<script lang="ts">
    export let pfpData: PfpData | null = null;
    export let size: "small" | "large" | "xl" = "small";
</script>

<div class="">
    {#if pfpData}
        <div class="profile-image-parent {size}">
            <img
                alt="profile"
                src={pfpData.imageSource}
                style="scale:{pfpData.zoom}; translate: {pfpData.offsetX}px {pfpData.offsetY}px"
            />
        </div>
    {:else}
        <i class="bi bi-person-circle icon {size}" />
    {/if}

    <style>
        .profile-image-parent {
            object-fit: cover;
            border-radius: 50%;
            aspect-ratio: 1 / 1;
            overflow: hidden;
        }

        .profile-image-parent.small {
            width: 100%;
            height: 100%;
        }
        .profile-image-parent.large {
            width: 150px;
            height: 150px;
        }
        .profile-image-parent.xl {
            width: 300px;
            height: 300px;
        }
        .icon.small {
            font-size: 40px;
            line-height: 1;
        }
        .icon.large {
            font-size: 150px;
            line-height: 1;
        }

        .icon.xl {
            font-size: 300px;
            line-height: 1;
        }
    </style>
</div>
