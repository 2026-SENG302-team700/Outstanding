<script lang="ts">
    import TaskBoardColumn from "$lib/components/task-board/task-board-column.svelte"
    import TaskBoardItem from "$lib/components/task-board/task-board-item.svelte"
    import type { TaskItem } from "$lib/types.js";
    import {DragDropProvider, PointerSensor, KeyboardSensor} from '@dnd-kit/svelte';
    import {defaultPreset} from '@dnd-kit/dom';
    import { addToast } from "$lib/toast/toast";
    import {move} from '@dnd-kit/helpers';
    import { fetchWithCsrf } from "$lib/csrf";
    import { resolve } from "$app/paths";
    
    let {
        tasks,
    }: {
        tasks: TaskItem[];
    } = $props();

    // Creating this taskMap of task Id to task object & passing it to the column component to be used to index tasks was inspired by Claude AI
    const taskMap: Record<number, TaskItem> = Object.fromEntries(
        tasks.map(t => [t.taskId, t])
    );
    
    // Creates a record (Map/Dictionary) of the status to the list of task Id's with that status
    const initialTasks: Record<number, number[]> = {
        0: tasks.filter((t) => t.currentStatus === 0).map((_: TaskItem, index: number) => _.taskId).slice(),
        1: tasks.filter((t) => t.currentStatus === 1).map((_: TaskItem, index: number) => _.taskId).slice(),
        2: tasks.filter((t) => t.currentStatus === 2).map((_: TaskItem, index: number) => _.taskId).slice(),
    };
    
    
    let tasksForSnapshot = $state<Record<number, number[]>>(initialTasks)

    const columns = Object.keys(initialTasks);
    let snapshot = $state(structuredClone(initialTasks));

    /**
     * sensors are used to detect user input via mouse, touch and keyboard.
     * PointerSensor is for mouse, keyboard and stylus, KeyboardSensor is self explanatory.
     * activatorElements are the objects that the Pointer can initiate a drag operation with. (in our case, its
     * the grab handles on the task.)
     */
    const sensors = [
        PointerSensor.configure({
            activatorElements(source) {
                return [source.element, source.handle];
            },
        }),
        KeyboardSensor,
    ];

    /**
     * Updates the tasks current status
     * @param task to be updated 
     * @param column which represents the new task status
     */
    async function updateTask(task: TaskItem, curStatus: string) {
        let status: number = parseInt(curStatus);
        try {
            const response = await fetchWithCsrf(
                resolve(`/api/taskItem/item/${task.taskId}` as any),
                {
                    method: "PUT",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        taskId: task.taskId,
                        name: task.name,
                        description: task.description,
                        dueDate: task.dueDate || null,
                        currentStatus: status
                    }),
                },
            );
            const data = await response.json().catch(() => null);
            if (!response.ok) {
                addToast(data?.message || "Failed to update task (Internal Server Error occurred).");
            }
        } catch (err) {
            addToast(`Failed to update task: ${err.message}`);
        }
    }

    /**
     * creates a new copy of the snapshot every time a task item is dragged
     */
    function onDragStart() {
        snapshot = $state.snapshot(tasksForSnapshot);
    }
    

    /**
     * takes an event (drag operation). then checks what it is dragging,
     * In this case, we can only drag a task, and it calls the "move" function
     * @param event
     */
    function onDragOver(event: any) {
        tasksForSnapshot = move(tasksForSnapshot, event)
    }

    /**
     * if the drag event is cancelled, then revert back to the original copy.
     * Otherwise, if the status of the task is changed, it will find the status whose tasks is changed and call the
     * updateTask function to update it in the backend.
     * @param event 
     */
    async function onDragEnd(event: any) {
        const {source} = event.operation;
        if (event.cancelled) {
            tasksForSnapshot = $state.snapshot(snapshot);;
        }
        const curSnapshot = $state.snapshot(tasksForSnapshot)
        for (let col in Object.keys(curSnapshot)) {
            for (const id of Object.values(curSnapshot[col])) {
                if (taskMap[id].currentStatus !== parseInt(col)) {
                    await updateTask(taskMap[id], col)
                }
            }
        }
        reorderReloadTasks()
    }

    /**
     * Sends ordering information to backend to persist dnd changes. Sends the list of taskId to the backend.
     */
    async function reorderReloadTasks(): void {
        try {
            await fetchWithCsrf(resolve('/api/taskItem/order'), {
                method: "PATCH",
                credentials: "include",
                headers: {'content-type': 'application/json'},
                body: JSON.stringify(Object.values($state.snapshot(tasksForSnapshot)).flat())
            });
        } catch (err) {
            console.error(err);
            addToast("An error occurred.", "error");
        }

    }
    
</script>

<DragDropProvider
        plugins={defaultPreset.plugins}
        {sensors}
        {onDragStart}
        {onDragOver}
        {onDragEnd}
>
    <div class="wrapper">
        <div class="board-columns">
            {#each columns as column, columnIndex (column)}
                <TaskBoardColumn
                        id={column}
                        index={columnIndex}
                        row={tasksForSnapshot[column]}
                        tasks={taskMap}
                />
            {/each}
        </div>
    </div>    
</DragDropProvider>


<style>
    .board-columns {
        display: flex;
        flex-direction: row;
        gap: 16px;
        align-items: flex-start;
    }
</style>