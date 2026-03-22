<script lang="ts">
    import { onMount } from "svelte";

    let { onImageSubmit, inputImage } = $props();

    const profileSize = $state(300.0);

    let imageSrc = $state("");

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

    /**
     * Insert an image into this ImageEditor object
     * Also calculates which side of the image is the long side,
     * and calculates the max size the image can be
     * Note: this function is called in onMount if inputImage is set
     * @param url the data url of the image
     */
    export async function setImg(url: string) {
        try {
            imageSrc = url;
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
     * Takes the current position and zoom of the
     * image and stamps it onto a CanvasRenderingContext2D
     * @param ctx The CanvasRenderingContext2D of a canvas
     */
    async function stampImageOntoCTX(ctx: CanvasRenderingContext2D) {
        const size = newWidth * (zoom / profileSize);

        const scale = width / size;

        const cropSize = profileSize * scale;

        const x = (width / 2) - cropSize / 2 - xOffset * scale;
        const y = (height / 2) - cropSize / 2 - yOffset * scale;

        const img = new Image();
        img.src = imageSrc;

        await img.decode();
        
        ctx.drawImage(img, x, y, cropSize, cropSize, 0, 0, profileSize, profileSize);
    }

    onMount(() => {
        if (inputImage) {
            setImg(inputImage);
        }
        
        document.addEventListener("mouseup", () => {
            isMoving = false;
        });
        document.addEventListener("mousemove", (e: MouseEvent) => {
            imageMoveEvent(e);
        });

        const canvas: HTMLCanvasElement = document.getElementById(
            "editCanvas",
        ) as HTMLCanvasElement;

        const ctx = canvas.getContext("2d");

        if (ctx) {
            document
                .getElementById("submitButton")
                ?.addEventListener("click", async (e) => {
                    ctx.reset();
                    await stampImageOntoCTX(ctx);
                    const url = canvas.toDataURL('image/jpeg');
                    onImageSubmit(url);
                });
        }
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
            alt="New profile"
            class="image-editor-image-editing"
            style={imageStyle}
        />
    </div>

    <label for="zoom-range" class="form-label">Zoom</label>
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
    <canvas
        id="editCanvas"
        class="image-editor-image-parent"
        style="height: {profileSize}px; width: {profileSize}px; display: none;"
        width='{profileSize}'
        height='{profileSize}'
    ></canvas>
    <button id="submitButton" class="btn btn-primary w-15">Submit</button>
</div>

<style>
    .image-editor-content {
        display: flex;
        flex-direction: column;
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
