<script lang="ts">
  import { onMount } from "svelte";
  import { fetchWithCsrf } from "$lib/csrf";
  import { formatDate } from "$lib/datepicker/formatDate";
  import { resolve } from "$app/paths";
  import { goto } from "$app/navigation";

  import {
    DragDropProvider,
    PointerSensor,
    KeyboardSensor,
  } from "@dnd-kit/svelte";
  import { defaultPreset } from "@dnd-kit/dom";
  import { move } from "@dnd-kit/helpers";
  //import "../styles.css";
  //import SortableColumn from "../SortableColumn.svelte";

  let tasks = $state([]);
  let loading = $state(false);
  let error = $state("");
  
  const COLORS: Record<string, string> = {
    
  }
  
  let initialTasks: Record<string, string[]> = {
    column: tasks.map((task) => `${task.TaskId}`),
  }
  let tasksForSnapshot = $state<Record<string, string[]>>(initialTasks)
  let snapshot = $state(structuredClone(initialTasks));
  

  onMount(() => {
    fetchAllTasks();
    
  });

  const sensors = [
    PointerSensor.configure({
      activatorElements(source) {
        return [source.element, source.handle];
      },
    }),
    KeyboardSensor,
  ];

  async function fetchAllTasks() {
    try {
      loading = true;
      const response = await fetchWithCsrf(resolve(`/api/taskitem`), {
        method: "GET",
        credentials: "include",
      });
      const data = await response.json();
      if (!response.ok) {
        error = data.message || "Failed to fetch tasks";
        return;
      }
      tasks = data;
    } catch (err) {
      error = "Failed to fetch tasks: " + (err as Error).message;
    } finally {
      loading = false;
    }
  }

  function shortenDesc(text: string | null, length: number) {
    if (!text) return "No description";
    if (text.length <= length) return text;
    return text.slice(0, length) + "...";
    tasksForSnapshot = move
  }
  
  function onDragStart() {
    snapshot = structuredClone(tasksForSnapshot);
  }
  
  function onDragOver(event: any) {
    const { source } = event.operation;
    if (source && source.type==="column") return;
    tasksForSnapshot = move(tasksForSnapshot, event)
  }
  
  function onDragEnd(event: any) {
    const {source} = event.operation;
    if (event.cancelled) {
      tasksForSnapshot = snapshot;
    }
  }
  
</script>

<div class="container">
  <h1 class="mb-4">Board</h1>

  {#if loading}
    <div class="text-center text-muted py-4">Loading tasks...</div>
  {:else if error}
    <div class="text-danger">{error}</div>
  {:else if tasks.length === 0}
    <div class="text-center text-muted py-4">No tasks found.</div>
  {:else}
    <DragDropProvider
            plugins={defaultPreset.plugins}
            {sensors}
            {onDragStart}
            {onDragOver}
            {onDragEnd}
    >
      <div class="board-columns">
        <!-- todo column -->
        <div class="board-column">
          <div class="column-header status-todo-header">Todo</div>
          {#each tasks.filter((t) => t.currentStatus === 0) as task}
            <div
                    class="task-card-small status-todo"
                    tabindex="0"
                    role="button"
                    on:click={() =>
              goto(
                resolve(
                  `/home/task-list/${task.taskListId}/task/${task.taskId}`,
                ),
              )}
                    on:keydown={(e) => {
              if (e.key === "Enter" || e.key === " ")
                goto(
                  resolve(
                    `/home/task-list/${task.taskListId}/task/${task.taskId}`,
                  ),
                );
            }}
            >
              <div class="task-card-header">
                <span class="task-title">{task.name}</span>
              </div>
              <p class="task-description">{shortenDesc(task.description, 30)}</p>
              <span class="due-date"
              >🗓 {task.dueDate === null
                      ? "No due date"
                      : formatDate(task.dueDate)}</span
              >
            </div>
          {/each}
        </div>
        <!-- in progress column -->
        <div class="board-column">
          <div class="column-header status-inprogress-header">In Progress</div>
          {#each tasks.filter((t) => t.currentStatus === 1) as task}
            <div
                    class="task-card-small status-inprogress"
                    tabindex="0"
                    role="button"
                    on:click={() =>
              goto(
                resolve(
                  `/home/task-list/${task.taskListId}/task/${task.taskId}`,
                ),
              )}
                    on:keydown={(e) => {
              if (e.key === "Enter" || e.key === " ")
                goto(
                  resolve(
                    `/home/task-list/${task.taskListId}/task/${task.taskId}`,
                  ),
                );
            }}
            >
              <div class="task-card-header">
                <span class="task-title">{task.name}</span>
              </div>
              <p class="task-description">{shortenDesc(task.description, 30)}</p>
              <span class="due-date"
              >🗓 {task.dueDate === null
                      ? "No due date"
                      : formatDate(task.dueDate)}</span
              >
            </div>
          {/each}
        </div>
        <!-- done column -->
        <div class="board-column">
          <div class="column-header status-done-header">Done</div>
          {#each tasks.filter((t) => t.currentStatus === 2) as task}
            <div
                    class="task-card-small status-done"
                    tabindex="0"
                    role="button"
                    on:click={() =>
              goto(
                resolve(
                  `/home/task-list/${task.taskListId}/task/${task.taskId}`,
                ),
              )}
                    on:keydown={(e) => {
              if (e.key === "Enter" || e.key === " ")
                goto(
                  resolve(
                    `/home/task-list/${task.taskListId}/task/${task.taskId}`,
                  ),
                );
            }}
            >
              <div class="task-card-header">
                <span class="task-title">{task.name}</span>
              </div>
              <p class="task-description">{shortenDesc(task.description, 30)}</p>
              <span class="due-date"
              >🗓 {task.dueDate === null
                      ? "No due date"
                      : formatDate(task.dueDate)}</span
              >
            </div>
          {/each}
        </div>
      </div>
    </DragDropProvider>
    
  {/if}
</div>

<style>
  .board-columns {
    display: flex;
    gap: 16px;
    align-items: flex-start;
  }

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

  .task-card-small {
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

  .task-card-small:hover {
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

  .task-card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 4px;
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
