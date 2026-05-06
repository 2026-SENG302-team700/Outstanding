<script lang="ts">
    let {
        digit1 = $bindable(""),
        digit2 = $bindable(""),
        digit3 = $bindable(""),
        digit4 = $bindable(""),
        digit5 = $bindable(""),
        digit6 = $bindable(""),
        error = "",
    } = $props();

    /// <summary>
    /// Automatically refocus on the next input box
    /// </summary>
    function handleInput(e: Event) {
        const input = e.target as HTMLInputElement;
        if (input.value && input.nextElementSibling) {
            (input.nextElementSibling as HTMLInputElement).focus();
        }
    }

    /// <summary>
    /// Move the focus back one box when backspace is clicked and the input box is empty
    /// </summary>
    function handleKeyDown(e: KeyboardEvent) {
        const input = e.target as HTMLInputElement;
        if (
            e.key === "Backspace" &&
            !input.value &&
            input.previousElementSibling
        ) {
            (input.previousElementSibling as HTMLInputElement).focus();
        }
    }
</script>

<div id="code-input" class="d-flex gap-2 mt-2 mb-4">
    <input
        type="text"
        class="form-control form-control-lg text-center"
        maxlength="1"
        bind:value={digit1}
        oninput={handleInput}
        onkeydown={handleKeyDown}
    />
    <input
        type="text"
        class="form-control form-control-lg text-center"
        maxlength="1"
        bind:value={digit2}
        oninput={handleInput}
        onkeydown={handleKeyDown}
    />
    <input
        type="text"
        class="form-control form-control-lg text-center"
        maxlength="1"
        bind:value={digit3}
        oninput={handleInput}
        onkeydown={handleKeyDown}
    />
    <input
        type="text"
        class="form-control form-control-lg text-center"
        maxlength="1"
        bind:value={digit4}
        oninput={handleInput}
        onkeydown={handleKeyDown}
    />
    <input
        type="text"
        class="form-control form-control-lg text-center"
        maxlength="1"
        bind:value={digit5}
        oninput={handleInput}
        onkeydown={handleKeyDown}
    />
    <input
        type="text"
        class="form-control form-control-lg text-center"
        maxlength="1"
        bind:value={digit6}
        oninput={handleInput}
        onkeydown={handleKeyDown}
    />
</div>
{#if error}
    <div class="text-danger small mb-3 animate-fade-in">
        <i class="bi bi-exclamation-circle-fill me-1"></i>
        {error}
    </div>
{/if}
