<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";

    let { loading = false, buttonType = "register", path = `/`} = $props();
</script>

<button class="btn btn-primary"
onclick={() => goto(resolve(route))}>
    {buttonType == "register"
        ? loading
            ? "Registering..."
            : "Register"
        : 
        buttonType == "login"
        ? loading
            ? "Logging In..."
            : "Login"
        : loading
            ? "Creating List..."
            : "Create List"}
</button>
