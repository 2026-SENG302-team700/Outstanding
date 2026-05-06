<script lang="ts">
    import { formatDate } from "$lib/datepicker/formatDate";
    import { resolve } from "$app/paths";
    import { goto } from "$app/navigation";
    
    let {
        task
    }: {
        task: {
            taskId: number,
            taskListId: number,
            name: string,
            description: string,
            currentStatus: 0 | 1 | 2
        }
    } = $props();

    /**
     * Turns taskStatus number to string to representing a CSS stylesheet class.
     * **/
    function taskStatusStyling(taskStatus: number): string {
        if (taskStatus == 0) {
            return "status-todo";
        }
        if (taskStatus == 1) {
            return "status-inprogress";
        }
        if (taskStatus == 2) {
            return "status-done";
        }
    }

    /** 
     * Shortens description down for the item.
     * **/
    function shortenDesc(text: string | null, length: number) {
        if (!text) return "No description";
        if (text.length <= length) return text;
        return text.slice(0, length) + "...";
    }
    
</script>

<div class="board-task-card {taskStatusStyling(task.currentStatus)}"
    tabindex="0"
    role="button"
    onclick={() => goto(resolve(`/home/task-list/${task.taskListId}/task/${task.taskId}`))}
    onkeydown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
            goto(resolve(`/home/task-list/${task.taskListId}/task/${task.taskId}`));
        }
    }}>
    <div class="task-card-header">
        <span class="task-title">{task.name}</span>
    </div>
    <p class="task-description">{shortenDesc(task.description, 30)}</p>
    <span class="due-date">
        🗓 {task.dueDate === null ? "No due date" : formatDate(task.dueDate)}
    </span>
</div>

<style>
    .board-task-card {
        background: white;
        border: 1px solid lightgrey;
        border-left: 4px solid white;
        border-radius: 8px;
        padding: 8px 12px;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.07);
        transition:
                box-shadow 0.2s ease,
                transform 0.1s ease;
        cursor: pointer;
    }

    .board-task-card:hover {
        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
        transform: translateY(-1px);
    }

    .status-todo {
        border-left-color: grey;
    }
    .status-inprogress {
        border-left-color: blue;
    }
    .status-done {
        border-left-color: lightgreen;
    }

    .task-title {
        font-weight: 600;
        font-size: 0.875rem;
        color: black;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .task-description {
        font-size: 0.775rem;
        color: grey;
        margin-bottom: 8px;
    }

    .due-date {
        font-size: 0.75rem;
        color: #9ca3af;
    }
</style>