<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";

    let { loading = false, buttonType = "list", path = `/` } = $props();
</script>

<button
    type="button"
    class="btn btn-primary"
    onclick={() => goto(resolve(path))}
>
    {buttonType == "list"
        ? loading
            ? "Redirecting..."
            : "Add task list"
        : buttonType == "task"
          ? loading
              ? "Redirecting..."
              : "Add Task "
          : "ADD ENTRY HERE "}
    <!-- if someone uses this component without updating it to include the correct text, this is to help remind them -->
</button>
