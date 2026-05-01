<script lang="ts">
    let {
        taskStatus,
        children
    }: {
        taskStatus: 0 | 1 | 2,
        children: Snippet
    } = $props();
    
    function taskStatusToString(taskStatus: number): string {
        if (taskStatus == 0) {
            return "Todo";
        }
        if (taskStatus == 1) {
            return "In Progress";
        }
        if (taskStatus == 2) {
            return "Done";
        }
    }
    
    function columnHeaderStyling(taskStatus: number): string {
        if (taskStatus == 0) {
            return "status-todo-header";
        }
        if (taskStatus == 1) {
            return "status-inprogress-header";
        }
        if (taskStatus == 2) {
            return "status-done-header";
        }
    }
    
</script>

<div class="board-column">
    <div class="column-header {columnHeaderStyling(taskStatus)}">
        {taskStatusToString(taskStatus)}
    </div>
    {@render children?.()}
</div>

<style>
    .board-column {
        flex: 1;
        background: #f3f4f6;
        border-radius: 10px;
        padding: 10px;
        min-height: 200px;
        display: flex;
        flex-direction: column;
        gap: 8px;
    }

    .column-header {
        font-weight: 700;
        font-size: 0.95rem;
        padding: 6px 10px;
        border-radius: 6px;
        margin-bottom: 4px;
        text-align: center;
    }

    .status-todo-header {
        background: #e5e7eb;
        color: #374151;
    }
    .status-inprogress-header {
        background: #dbeafe;
        color: #1d4ed8;
    }
    .status-done-header {
        background: #dcfce7;
        color: #15803d;
    }
</style>