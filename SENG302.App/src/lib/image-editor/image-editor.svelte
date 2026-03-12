<script lang="ts">
    import testImage from "$lib/assets/images/target.jpg";
    import { onMount } from "svelte";

    let imageSrc = $state("");

    let zoom = $state(300.0);
    let shortDim = $state("width");

    let width = 0;
    let height = 0;

    let newWidth = 0;
    let newHeight = 0;

    let xOffset = $state(0);
    let yOffset = $state(0);

    let isMoving = false;

    let imageStyle = $derived(
        `${shortDim}: ${zoom}px; translate: ${xOffset + 149 - (newWidth * (zoom / 300)) / 2}px ${yOffset + 149 - (newHeight * (zoom / 300)) / 2}px;`,
    );

    async function setImg(url: string) {
        try {
            imageSrc = url;
            const dimensions = await getImgDimensions(imageSrc);
            width = dimensions.width;
            height = dimensions.height;

            if (width < height) {
                shortDim = "width";
                newWidth = 300;
                newHeight = (newWidth / width) * height;
            } else {
                shortDim = "height";
                newHeight = 300;
                newWidth = (newHeight / height) * width;
            }
        } catch (error) {
            console.log("image not found");
        }
    }

    async function getImgDimensions(url: string) {
        try {
            const img = new Image();
            img.src = url;

            await img.decode();

            return {
                width: img.naturalWidth,
                height: img.naturalHeight,
            };
        } catch (error) {
            throw new Error(`Could not load image at ${url}, ${error}`);
        }
    }

    onMount(() => {
        setImg(testImage);
    });
</script>

<div class="image-editor-content">
    <div
        class="image-editor-image-parent"
        role="button"
        tabindex="-1"
        onmousedown={() => (isMoving = true)}
        onmouseup={() => (isMoving = false)}
        onmouseleave={() => (isMoving = false)}
        onmousemove={() => {
            if (isMoving) {
                console.log(event);
            }
        }}
    >
        <img
            draggable="false"
            src={imageSrc}
            alt="New profile"
            class="image-editor-image-editing"
            style={imageStyle}
        />
    </div>

    <label for="zoom-range" class="form-label">Zoom</label>
    <input
        type="range"
        class="form-range"
        max="2000.0"
        min="300.0"
        id="zoom-range"
        bind:value={zoom}
    />
</div>

<style>
    .image-editor-content {
        display: flex;
        flex-direction: column;
    }

    .image-editor-image-editing {
        position: relative;
    }

    .image-editor-image-parent {
        width: 300px;
        height: 300px;
        border: 1px solid black;
        border-radius: 50%;
        overflow: hidden;
    }
</style>
