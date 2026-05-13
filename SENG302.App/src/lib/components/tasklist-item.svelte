<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";

    let { taskList } = $props();
</script>

<div
    class="text-break border-bottom task-item-box p-2"
    tabindex="0"
    role="button"
    onclick={() => goto(resolve(`/home/task-list/${taskList.id}`))}
    onkeydown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
            goto(resolve(`/home/task-list/${taskList.id}`));
        }
    }}
>
    <span class="fs-5">{taskList.name}</span>
</div>
