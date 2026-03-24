<script lang="ts">
    export let pfpData: PfpData | null = null;
    export let size: "small" | "large" | "xl" = "small";
    const sizeValues = {
        "small" : 40.0,
        "large" : 150.0,
        "xl" : 300.0
    }

    function getScalar() {
        // 300 is here because it is the profileSize in imageEditor
        // All of the profile picture manipulations are relative to that size
        return (sizeValues[size] / 300.0);
    }

</script>

<!--  -->
<div class="">
    {#if pfpData}
        <div 
            class="profile-image-parent"
            style="
                width: {sizeValues[size]}px;
                height: {sizeValues[size]}px;"
            >
            <img
                alt="profile"
                src={pfpData.imageSource}
                style="transform-origin: top left;
                        scale:{pfpData.zoom * getScalar()};
                        translate: {pfpData.offsetX * getScalar()}px {pfpData.offsetY * getScalar()}px"
            />
        </div>
    {:else}
        <i class="bi bi-person-circle icon {size}" />
    {/if}

    <style>

        .profile-image-parent {
            border-radius: 50%;
            aspect-ratio: 1 / 1;
            overflow: hidden;
            border: 1px solid black;
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
