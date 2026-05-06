<script lang="ts">
    import TaskBoardColumn from "$lib/components/task-board/task-board-column.svelte"
    import TaskBoardItem from "$lib/components/task-board/task-board-item.svelte"
    import type { TaskItem } from "$lib/types.js";
    import {DragDropProvider, PointerSensor, KeyboardSensor} from '@dnd-kit/svelte';
    import {defaultPreset} from '@dnd-kit/dom';
    import { addToast } from "$lib/toast/toast";
    import {move} from '@dnd-kit/helpers';
    import { fetchWithCsrf } from "$lib/csrf";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    
    let {
        tasks,
    }: {
        tasks: TaskItem[];
    } = $props();

    // Creating this taskMap and passing it to the column component to be used to index tasks was inspired by Claude AI
    const taskMap: Record<number, TaskItem> = Object.fromEntries(
        tasks.map(t => [t.taskId, t])
    );
    
    
    const initialTasks: Record<number, number[]> = {
        0: tasks.filter((t) => t.currentStatus === 0).map((_: TaskItem, index: number) => _.taskId).slice(),
        1: tasks.filter((t) => t.currentStatus === 1).map((_: TaskItem, index: number) => _.taskId).slice(),
        2: tasks.filter((t) => t.currentStatus === 2).map((_: TaskItem, index: number) => _.taskId).slice(),
    };
    
    
    let tasksForSnapshot = $state<Record<number, number[]>>(initialTasks)

    const columns = Object.keys(initialTasks);
    let snapshot = $state(structuredClone(initialTasks));
    
    
    const sensors = [
        PointerSensor.configure({
            activatorElements(source) {
                return [source.element, source.handle];
            },
        }),
        KeyboardSensor,
    ];

    /**
     * Update tasks current status
     * @param task to be updated 
     * @param column which represents the new task status
     */
    async function updateTask(task: TaskItem, curStatus: string) {
        console.log(task)
        console.log(curStatus)
        let currentStatus: number = parseInt(curStatus);
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
                        currentStatus
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
    
    
    function onDragStart() {
        snapshot = $state.snapshot(tasksForSnapshot);
    }
    
    function onDragOver(event: any) {
        const { source } = event.operation;
        if (source && source.type==="column") return;
        tasksForSnapshot = move(tasksForSnapshot, event)
    }
    
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


<!--<TaskBoardColumn taskStatus=0>-->
<!--    {#each tasks.filter((t) => t.currentStatus === 0) as task}-->
<!--        <TaskBoardItem task={task} />-->
<!--    {/each}-->
<!--</TaskBoardColumn>-->
<!--<TaskBoardColumn taskStatus=1>-->
<!--    {#each tasks.filter((t) => t.currentStatus === 1) as task}-->
<!--        <TaskBoardItem task={task} />-->
<!--    {/each}-->
<!--</TaskBoardColumn>-->
<!--<TaskBoardColumn taskStatus=2>-->
<!--    {#each tasks.filter((t) => t.currentStatus === 2) as task}-->
<!--        <TaskBoardItem task={task} />-->
<!--    {/each}-->
<!--</TaskBoardColumn>-->