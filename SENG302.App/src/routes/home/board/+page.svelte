<script lang="ts">
  import { onMount } from "svelte";
  import { fetchWithCsrf } from "$lib/csrf";
  import { formatDate } from "$lib/datepicker/formatDate";
  import { resolve } from "$app/paths";
  import { goto } from "$app/navigation";

  let tasks = $state([]);
  let loading = $state(false);
  let error = $state("");

  onMount(() => {
    fetchAllTasks();
  });

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
    <div class="task-grid">
      {#each tasks as task}
        <div
          class="task-card-small"
          class:status-todo={task.currentStatus === 0}
          class:status-inprogress={task.currentStatus === 1}
          class:status-done={task.currentStatus === 2}
          tabindex="0"
          role="button"
          on:click={() =>
            goto(
              resolve(`/home/task-list/${task.taskListId}/task/${task.taskId}`),
            )}
          on:keydown={(e) => {
            if (e.key === "Enter" || e.key === " ") {
              goto(
                resolve(
                  `/home/task-list/${task.taskListId}/task/${task.taskId}`,
                ),
              );
            }
          }}
        >
          <div class="task-card-header">
            <span class="task-title">{task.name}</span>
            <span
              class="status-badge"
              class:badge-todo={task.currentStatus === 0}
              class:badge-inprogress={task.currentStatus === 1}
              class:badge-done={task.currentStatus === 2}
            >
              {#if task.currentStatus === 0}Todo
              {:else if task.currentStatus === 1}In Progress
              {:else}Done{/if}
            </span>
          </div>

          <p class="task-description">{shortenDesc(task.description, 30)}</p>

          <span class="due-date">
            🗓 {task.dueDate === null
              ? "No due date"
              : formatDate(task.dueDate)}
          </span>
        </div>
      {/each}
    </div>
  {/if}
</div>

<style>
  .task-grid {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
  }

  .task-card-small {
    background: white;
    border: 1px solid lightgrey;
    border-left: 4px solid white;
    border-radius: 8px;
    padding: 8px 12px;
    width: 250px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.07);
    transition:
      box-shadow 0.2s ease,
      transform 0.1s ease;
    cursor: default;
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
    gap: 6px;
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

  .status-badge {
    font-size: 0.65rem;
    font-weight: 600;
    padding: 2px 7px;
    border-radius: 999px;
  }

  .badge-todo {
    background: #f3f4f6;
    color: grey;
  }
  .badge-inprogress {
    background: #eff6ff;
    color: blue;
  }
  .badge-done {
    background: #f0fdf4;
    color: lightgreen;
  }
</style>
