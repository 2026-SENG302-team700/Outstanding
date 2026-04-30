<script lang="ts">
    import { TaskItem } from "$lib/types"
    import { createSortable } from "@dnd-kit/svelte/sortable";
    let {
        task,
        column,
        index,
        accentColor,
        
    }: {
        task: TaskItem
        id: string;
        column: string;
        index: number;
        accentColor: string;
    } = $props();

    const sortable = createSortable({
        get id() {
            return id;
        },
        get index() {
            return index;
        },
        get group() {
            return column;
        },
        accept: "item",
        type: "item",
        feedback: "clone",
        get data() {
            return { group: column };
        },
    });
</script>

<li
        {@attach sortable.attach}
        class="item"
        data-shadow={sortable.isDragging ? "true" : undefined}
        data-accent-color={column}
        style:--accent-color={accentColor}
>
    <div
            class="task-card"
            class:status-todo={task.currentStatus === 0}
            class:status-inprogress={task.currentStatus === 1}
            class:status-done={task.currentStatus === 2}
            tabindex="0"
            role="button"
            on:click={() =>
            goto(`/home/task-list/${params.slug}/task/${task.taskId}`)}
            on:keydown={(e) => {
            if (e.key === "Enter" || e.key === " ") {
              goto(`/home/task-list/${params.slug}/task/${task.taskId}`);
            }
          }}
    >
        <div class="task-card-header">
            <span class="task-title">{task.name}</span>
            <span
                    class="status-badge"
                    class:badge-todo={task.currentStatus === 0}
                    class:badge-inprogress={task.currentStatus === 1}
                    class:badge-done={task.currentStatus === 2}
            >
              {#if task.currentStatus == 0}ToDo
              {:else if task.currentStatus === 1}In Progress
              {:else}Done{/if}
            </span>
        </div>
    </div>    
    <button
            {@attach sortable.attachHandle}
            class="handle"
            aria-label="Drag handle"
    ></button>
</li>

