<script lang="ts">
    import profileImg from "$lib/assets/images/defaultProfile.png";

    let zoom = $state(300.0);
    let longDim = "width";

    let imageStyle = $derived(`${longDim}: ${zoom}px;`);

    const image = encodeURI(profileImg);

    async function updateDimensions() {
        try {
            const dimensions = await getImgDimensions(image);

            if (dimensions.width < dimensions.height) {
                longDim = "width";
            } else {
                longDim = "height";
            }
            console.log(longDim);
        } catch (error) {
            console.error("image not found");
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

    updateDimensions();
</script>

<div class="image-editor-content">
    <div class="image-editor-image-parent">
        <img
            draggable="true"
            src={image}
            alt="New profile"
            class="image-editor-image-editing"
            style={imageStyle}
        />
    </div>

    <label for="zoom-range" class="form-label">Zoom</label>
    <input
        type="range"
        class="form-range"
        max="600.0"
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
