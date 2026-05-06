<script lang="ts">
    import TaskBoardColumn from "$lib/components/task-board/task-board-column.svelte"
    import TaskBoardItem from "$lib/components/task-board/task-board-item.svelte"
    import type { TaskItem } from "$lib/types.js";
    import {DragDropProvider, PointerSensor, KeyboardSensor} from '@dnd-kit/svelte';
    import {defaultPreset} from '@dnd-kit/dom';
    import {move} from '@dnd-kit/helpers';
    
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
    
    console.log(`initialTasks: ${JSON.stringify(initialTasks, null, 2)}`)
    
    let tasksForSnapshot = $state<Record<number, number[]>>(initialTasks)

    const columns = Object.keys(initialTasks);
    let snapshot = $state(structuredClone(initialTasks));

    console.log(`columns = ${JSON.stringify(columns, null, 2)}`)
    
    const sensors = [
        PointerSensor.configure({
            activatorElements(source) {
                return [source.element, source.handle];
            },
        }),
        KeyboardSensor,
    ];
    

    function onDragStart() {
        snapshot = $state.snapshot(tasksForSnapshot);;
    }

    function onDragOver(event: any) {
        const { source } = event.operation;
        if (source && source.type==="column") return;
        tasksForSnapshot = move(tasksForSnapshot, event)
    }

    function onDragEnd(event: any) {
        const {source} = event.operation;
        if (event.cancelled) {
            tasksForSnapshot = $state.snapshot(snapshot);;
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
                {console.log(`Tasks for each column: ${JSON.stringify(tasks.filter((t) => t.currentStatus === parseInt(column)), null, 2)}`)}
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