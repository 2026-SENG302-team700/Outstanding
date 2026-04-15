<script lang="ts">
    let { loading = false, buttonType = "register" } = $props();
</script>

<button type="submit" class="btn btn-primary w-100" disabled={loading}>
    {buttonType == "register"
        ? loading
            ? "Registering..."
            : "Register"
        : loading
          ? "Logging In..."
          : "Login"}
</button>
