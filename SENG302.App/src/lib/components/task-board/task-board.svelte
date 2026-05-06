<script lang="ts">
    import TaskBoardColumn from "$lib/components/task-board/task-board-column.svelte"
    import TaskBoardItem from "$lib/components/task-board/task-board-item.svelte"
    
    let {
        tasks,
    }: {
        tasks: Array;
    } = $props();
</script>

<div class="board-columns">
    <TaskBoardColumn taskStatus=0>
        {#each tasks.filter((t) => t.currentStatus === 0) as task}
            <TaskBoardItem task={task} />
        {/each}
    </TaskBoardColumn>
    <TaskBoardColumn taskStatus=1>
        {#each tasks.filter((t) => t.currentStatus === 1) as task}
            <TaskBoardItem task={task} />
        {/each}
    </TaskBoardColumn>
    <TaskBoardColumn taskStatus=2>
        {#each tasks.filter((t) => t.currentStatus === 2) as task}
            <TaskBoardItem task={task} />
        {/each}
    </TaskBoardColumn>
</div>

<style>
    .board-columns {
        display: flex;
        gap: 16px;
        align-items: flex-start;
    }
</style>