<script lang="ts">
    import { createSortable } from "@dnd-kit/svelte/sortable"
    import { formatDate } from "$lib/datepicker/formatDate";
    import { resolve } from "$app/paths";
    import { goto } from "$app/navigation";
    
    let {
        taskId,
        taskListId,
        name,
        description,
        dueDate
    } : {
        taskId: string;
        taskListId: string;
        name: string;
        description: string;
        dueData: Date;
    } = $props();
</script>

<li 
        {@attach sortable.attach}
        class="item"
        data-shadow={sortable.isDragging ? "true" : undefined}
>
    <div
            class="task-card-small status-inprogress"
            tabindex="0"
            role="button"
            on:click={() =>
              goto(
                resolve(
                  `/home/task-list/${task.taskListId}/task/${task.taskId}`,
                ),
              )}
            on:keydown={(e) => {
              if (e.key === "Enter" || e.key === " ")
                goto(
                  resolve(
                    `/home/task-list/${task.taskListId}/task/${task.taskId}`,
                  ),
                );
            }}
    >
        <div class="task-card-header">
            <span class="task-title">{task.name}</span>
        </div>
        <p class="task-description">{shortenDesc(task.description, 30)}</p>
        <span class="due-date"
        >🗓 {task.dueDate === null
            ? "No due date"
            : formatDate(task.dueDate)}</span
        >
    </div>
    <button
            {@attach sortable.attachHandle}
            class="handle"
            aria-label="Drag handle"
    ></button>
</li>        

<style>
    .item {
        display: flex;
        align-items: center;
        justify-content: space-between;
        box-sizing: border-box;
        padding: 0 20px;
        border: none;
    }
</style>