<script lang="ts">
    let zoom = $state(1.0);
    let maxImageStyle = $state("width: 300px");

    let imageUrl = $state('/tallProfile.png');

    async function setImg(url : string) {
        imageUrl = url;
        const dimensions = await getImgDimensions(url);
        
        if (dimensions.width < dimensions.height) {
            maxImageStyle = "width: 300px";
        } else {
            maxImageStyle = "height: 300px";
        }
    }

    async function getImgDimensions(url : string) {
        const img = new Image();
        img.src = url;

        await img.decode();
        try {
            return {
                width: img.naturalWidth,
                height: img.naturalHeight
            };
        } catch (error) {
            throw new Error(`Could not load image at ${url}, ${error}`);
        }
    }

    setImg(imageUrl);
</script>

<div class="image-editor-content">
    <div class="image-editor-image-parent">
        <img
            draggable=true
            src={imageUrl}
            alt="New profile"
            class="image-editor-image-editing"
            style={maxImageStyle}
        >
    </div>

    <label for="zoom-range" class="form-label">Zoom</label>
    <input type="range" class="form-range" max=600.0 min=300.0 id="zoom-range" bind:value={zoom}>

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