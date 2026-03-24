<script lang="ts">
    import { addToast } from "$lib/toast/toast";
    import { onDestroy, onMount } from "svelte";

    const profileSize = $state(250.0);

    let imageSrc = $state("");
    let imageFile = $state();

    // svelte-ignore state_referenced_locally
    let zoom = $state(profileSize);
    let shortDim = $state("width");

    let width = 0;
    let height = 0;

    let newWidth = $state(0);
    let newHeight = $state(0);

    let xOffset = $state(0);
    let yOffset = $state(0);

    let isMoving = false;
    let movingOffset = { x: 0, y: 0 };

    let imageStyle = $derived(
        `${shortDim}: ${zoom}px;
        translate:
            ${xOffset + (profileSize / 2 - 1) - (newWidth * (zoom / profileSize)) / 2}px
            ${yOffset + (profileSize / 2 - 1) - (newHeight * (zoom / profileSize)) / 2}px;`,
    );

    const mimeTypes = [
        "image/webp",
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/svg+xml"
    ]

    /**
     * Insert an image into this ImageEditor object
     * Also calculates which side of the image is the long side,
     * and calculates the max size the image can be
     * @param file the image file
     */
    export async function setImg(file: File) {

        if (!(mimeTypes.includes(file.type))) {
            addToast("Invalid image, supported file types are .jpeg, .png, .svg, .gif .webp", "error");
            return;
        }

        reset();
        try {
            imageFile = file;
            imageSrc = URL.createObjectURL(file);

            const dimensions = await getImgDimensions(imageSrc);
            width = dimensions.width;
            height = dimensions.height;

            if (width < height) {
                shortDim = "width";
                newWidth = profileSize;
                newHeight = (newWidth / width) * height;
            } else {
                shortDim = "height";
                newHeight = profileSize;
                newWidth = (newHeight / height) * width;
            }
        } catch (error) {
            console.log("image not found");
        }
    }

    /**
     * Get the dimensions of an image
     * @param url the url of the image
     */
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

    /**
     * Reset the image and all properties
     */
    export async function reset() {
        zoom = profileSize;
        if (imageSrc) URL.revokeObjectURL(imageSrc);
        imageSrc = "";
        imageFile = null;
        xOffset = 0;
        yOffset = 0;
    }

    /**
     * Whenever the mouse moves this event is called, which moves the image and clamps the position
     * @param event
     */
    function imageMoveEvent(event: MouseEvent) {
        if (event !== undefined && isMoving) {
            xOffset += event.x - movingOffset.x;
            yOffset += event.y - movingOffset.y;

            movingOffset.x = event.x;
            movingOffset.y = event.y;

            clampOffset();
        }
    }

    /**
     * Clamps the position within the bounds
     */
    function clampOffset() {
        const xLeeway = profileSize - newWidth * (zoom / profileSize);
        const yLeeway = profileSize - newHeight * (zoom / profileSize);

        xOffset = Math.max(xOffset, xLeeway / 2);
        xOffset = Math.min(xOffset, -xLeeway / 2);

        yOffset = Math.max(yOffset, yLeeway / 2);
        yOffset = Math.min(yOffset, -yLeeway / 2);
    }

    /**
     * When starting to click on the image, this function is called
     * It sets the initial position of the mouse so that these coordinates
     * can be used later to compare with the relative move of the mouse
     * @param moveX x mouse position
     * @param moveY y mouse position
     */
    function startMove(moveX: number, moveY: number) {
        isMoving = true;
        movingOffset.x = moveX;
        movingOffset.y = moveY;
    }

    /**
     * sets the zoom and clamps the position
     * @param newZoom the new zoom, in pixel width (starts at profileSize)
     */
    function updateZoom(newZoom: number) {
        const zoomDiff = newZoom / zoom;

        yOffset *= zoomDiff;
        xOffset *= zoomDiff;

        zoom = newZoom;
        clampOffset();
    }

    /**
     * Translate the data into a more general format, one that the backend can understand
     */
    export function exportData(): { data: PfpData; file: File } | null {

        if (imageFile == null) {
            return null;
        }

        return {
            data: {
                // Create a new object url because we don't want it to get dereferenced
                imageSource: URL.createObjectURL(imageFile as File),
                offsetX: (xOffset + (profileSize / 2 - 1) - (newWidth * (zoom / profileSize)) / 2) / profileSize,
                offsetY: (yOffset + (profileSize / 2 - 1) - (newHeight * (zoom / profileSize)) / 2) / profileSize,
                zoom: (zoom / Math.min(width, height)) / profileSize,
            },
            file: imageFile as File,
        };
    }

    onMount(() => {
        document.addEventListener("mouseup", () => {
            isMoving = false;
        });
        document.addEventListener("mousemove", (e: MouseEvent) => {
            imageMoveEvent(e);
        });
    });

    onDestroy(() => {
        if (imageSrc) URL.revokeObjectURL(imageSrc);
    });
</script>

<div class="image-editor-content">
    <div
        class="image-editor-image-parent"
        role="button"
        tabindex="-1"
        style="width: {profileSize}px; height: {profileSize}px;"
        onmousedown={(e: MouseEvent) => startMove(e.x, e.y)}
    >
        <img
            draggable="false"
            src={imageSrc}
            alt={imageSrc == "" ? "" : "New profile"}
            class="image-editor-image-editing"
            style={imageStyle}
        />
    </div>

    <div style="height: 15px;"></div>
    <input
        type="range"
        class="form-range"
        max={profileSize * 10}
        min={profileSize}
        id="zoom-range"
        oninput={(e: Event) => {
            if (e.target)
                updateZoom(parseFloat((<HTMLInputElement>e.target).value));
        }}
        value={zoom}
    />
    
</div>

<style>
    .image-editor-content {
        display: flex;
        flex-direction: column;
        align-items: center;
    }

    .image-editor-image-editing {
        position: relative;
        user-select: none;
    }

    .image-editor-image-parent {
        border: 1px solid black;
        border-radius: 50%;
        overflow: hidden;
    }
</style>
