<script lang="ts">
    import {
        draggable,
        droppable,
        type DragDropState,
    } from "@thisux/sveltednd";
    import { flip } from "svelte/animate";
    import { fade } from "svelte/transition";
    import { cubicOut } from "svelte/easing";

    interface Task {
        id: string;
        title: string;
    }

    let tasks = $state<Task[]>([
        { id: "1", title: "Design review" },
        { id: "2", title: "Code review" },
        { id: "3", title: "Deploy to prod" },
    ]);

    function handleDrop(state: DragDropState<Task>) {
        const { draggedItem, targetContainer, dropPosition } = state;
        const dragIndex = tasks.findIndex((t) => t.id === draggedItem.id);
        let dropIndex = parseInt(targetContainer ?? "0");
        if (dropPosition === "after") dropIndex++;

        if (dragIndex !== -1) {
            const [task] = tasks.splice(dragIndex, 1);
            const adjusted = dragIndex < dropIndex ? dropIndex - 1 : dropIndex;
            tasks.splice(adjusted, 0, task);
        }
    }
</script>

<div class="task-list">
    {#each tasks as task, index (task.id)}
        <div
            use:draggable={{ container: index.toString(), dragData: task }}
            use:droppable={{
                container: index.toString(),
                callbacks: { onDrop: handleDrop },
            }}
            animate:flip={{ duration: 200, easing: cubicOut }}
            class="task-item task-parent"
        >
            <div class="task">
                {task.title}
            </div>
        </div>
    {/each}
</div>

<style>
    .task {
        padding: 20px;
        border: 1px solid black;
        transition: 1s;
        background-color: aliceblue;
    }

    .task-parent {
        padding-bottom: 5px;
        padding-top: 5px;
    }
</style>
