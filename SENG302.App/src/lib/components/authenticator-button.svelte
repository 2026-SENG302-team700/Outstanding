<script lang="ts">
    let { loading = false, buttonType = "register" } = $props();
</script>

<button
    type="submit"
    class={"btn btn-primary " + (buttonType == "update" ? "" : "w-100")}
    disabled={loading}
>
    {buttonType == "register"
        ? loading
            ? "Registering..."
            : "Register"
        : buttonType == "update"
          ? loading
              ? "Updating..."
              : "Update"
          : loading
            ? "Logging In..."
            : "Login"}
</button>
