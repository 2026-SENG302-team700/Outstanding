<script lang="ts">
    import { goto } from "$app/navigation";
    import type { TaskItem } from "../types";
    import { createSortable } from "@dnd-kit/svelte/sortable";
    import { formatDate } from "$lib/datepicker/formatDate";
    let {
        id,
        index,
        task,
    }: {
        id: number;
        index: number;
        task: TaskItem;
    } = $props();

    const sortable = createSortable({
        get id() {
            return id;
        },
        get index() {
            return index;
        },
    });

    /**
     * shorten the length of the displayed description to 'number' characters, add '...' onto the end of the description to indicate more.
     * @param text the description to shorten
     * @param length length of description to cut down too
     */
    function shortenDesc(text: string | null, length: number) {
        if (!text) return "No Description";
        if (text.length <= length) return text;
        return text.slice(0, length) + "...";
    }
</script>

<div
    {@attach sortable.attach}
    class="task-card"
    data-shadow={sortable.isDragging ? "true" : undefined}
    class:status-todo={task.currentStatus === 0}
    class:status-inprogress={task.currentStatus === 1}
    class:status-done={task.currentStatus === 2}
    tabindex="0"
    role="button"
    onclick={() =>
        goto(`/home/task-list/${task.taskListId}/task/${task.taskId}`)}
    onkeydown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
            goto(`/home/task-list/${task.taskListId}/task/${task.taskId}`);
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
            {#if task.currentStatus == 0}To Do
            {:else if task.currentStatus === 1}In Progress
            {:else}Done{/if}
        </span>
    </div>
    

    <p class="task-description">{shortenDesc(task.description, 50)}</p>

    <div class="task-footer">
        <span class="due-date">
            🗓 {task.dueDate === null
                ? "No Due Date"
                : formatDate(task.dueDate)}
        </span>
    </div>
</div>

<style>
    .task-card {
        display: inherit;
        background: white;
        border: 1px solid lightgrey;
        border-left: 4px solid #e5e7eb;
        border-radius: 8px;
        padding: 12px 16px;
        margin-bottom: 10px;
        cursor: pointer;
        box-shadow: 0px 0px 5px lightgrey;
        transition:
            box-shadow 0.2s ease,
            transform 0.1s ease;
    }

    .handle {
        border: 5px solid lightgrey;
        background: white;
        border-radius: 10px;
        width: 30px;
        height: 50px;
        position: absolute;
    }

    .task-card:hover {
        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
        transform: translateY(-1px);
    }

    .task-card:focus-visible {
        box-shadow: 0 0 0 3px #4a90e2;
    }

    .status-todo {
        border-left-color: darkgray;
    }
    .status-inprogress {
        border-left-color: blue;
    }
    .status-done {
        border-left-color: lightgreen;
    }

    .task-card-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 6px;
    }

    .task-title {
        font-weight: 600;
        font-size: 1rem;
        color: black;
    }

    .task-description {
        color: grey;
        font-size: 0.875rem;
        margin-bottom: 10px;
    }

    .task-footer {
        display: flex;
        justify-content: flex-start;
    }

    .due-date {
        font-size: 0.8rem;
        color: #9ca3af;
    }

    /* Status badges */
    .status-badge {
        font-size: 0.75rem;
        font-weight: 600;
        padding: 2px 10px;
        border-radius: 999px;
    }

    .badge-todo {
        background: white;
        color: darkgray;
    }
    .badge-inprogress {
        background: white;
        color: blue;
    }
    .badge-done {
        background: white;
        color: lightgreen;
    }
</style>
