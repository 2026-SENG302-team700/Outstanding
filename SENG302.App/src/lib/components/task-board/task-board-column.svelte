<script lang="ts">
  import { CollisionPriority } from "@dnd-kit/abstract";
  import { createDroppable } from "@dnd-kit/svelte";
  import type { TaskItem } from "$lib/types.js";
  import TaskBoardItem from "$lib/components/task-board/task-board-item.svelte";
  import { flip } from "svelte/animate";
  import { cubicOut } from "svelte/easing";
  import { fly } from "svelte/transition";

  let {
    id,
    index,
    row,
    tasks,
  }: {
    id: string;
    index: number;
    row: number[];
    tasks: Record<number, TaskItem>;
  } = $props();

  /**
   * Turns taskStatus number to string to be displayed as a column header
   * **/
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

  /**
   * Turns taskStatus number to string to representing a CSS stylesheet class.
   * **/
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

  const droppable = createDroppable({
    get id() {
      return id;
    },
    get index() {
      return index;
    },
    accept: ["column", "item"],
    collisionPriority: CollisionPriority.Low,
    type: "column",
  });
</script>

<div class="board-column" {@attach droppable.attach}>
  <div class="column-header {columnHeaderStyling(id)}">
    {taskStatusToString(id)}
  </div>
  <ul>
    {#each row as itemId, itemIndex (itemId)}
      <div animate:flip={{ duration: 300, easing: cubicOut }}>
        <div in:fly={{ y: -20, duration: 300 }}>
          <TaskBoardItem
            task={tasks[itemId]}
            id={itemId}
            column={id}
            index={itemIndex}
          />
        </div>
      </div>
    {/each}
  </ul>
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
