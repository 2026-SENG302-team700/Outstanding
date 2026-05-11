<script lang="ts">
  import { formatDate } from "$lib/datepicker/formatDate";
  import { resolve } from "$app/paths";
  import { goto } from "$app/navigation";
  import type { TaskItem } from "$lib/types.js";
  import { createSortable } from "@dnd-kit/svelte/sortable";

  let {
    task,
    id,
    column,
    index,
    taskLists = [],
  }: {
    task: TaskItem;
    id: string;
    column: string;
    index: number;
    taskLists?: { id: number; name: string }[];
  } = $props();

  /**
   * Get the name of a list for when viewing the tasks on the home board page
   */
  function getListName(taskListId: number): string {
    return taskLists.find((l) => l.id === taskListId)?.name ?? "";
  }

  /**
   * Returns the CSS stylesheet class name to apply the task card based on it's status
   * @param taskStatus : Number representing the task status the column represents
   */
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
   * Shortens the description to be displayed on the task cards if it is too long,
   * replacing a slice of the description after a certain character count with '...'
   * @param text - The text being displayed
   * @param length - The character length up to which we display description, replacing everything after with '...'
   */
  function shortenDesc(text: string | null, length: number) {
    if (!text) return "No description";
    if (text.length <= length) return text;
    return text.slice(0, length) + "...";
  }

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

<div
  {@attach sortable.attach}
  class="board-task-card {taskStatusStyling(parseInt(column))}"
  tabindex="0"
  role="button"
  onclick={() =>
    goto(resolve(`/home/task-list/${task.taskListId}/task/${task.taskId}`))}
  onkeydown={(e) => {
    if (e.key === "Enter" || e.key === " ") {
      goto(resolve(`/home/task-list/${task.taskListId}/task/${task.taskId}`));
    }
  }}
>
  <div class="task-card-header">
    <span class="task-title">{task.name}</span>
  </div>
  {#if taskLists.length > 0}
    <span class="list-badge">{getListName(task.taskListId)}</span>
  {/if}
  <p class="task-description">{shortenDesc(task.description, 30)}</p>
  <span class="due-date">
    🗓 {task.dueDate === null ? "No due date" : formatDate(task.dueDate)}
  </span>
</div>

<style>
  .board-task-card {
    display: inherit;
    background: white;
    border: 1px solid lightgrey;
    border-left: 4px solid white;
    border-radius: 8px;
    padding: 8px 12px;
    margin-bottom: 5px;
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

  .list-badge {
    font-size: 0.65rem;
    font-weight: 600;
    padding: 2px 7px;
    border-radius: 999px;
    background: #e0e7ff;
    color: #3730a3;
    display: inline-block;
    margin-bottom: 6px;
  }
</style>
