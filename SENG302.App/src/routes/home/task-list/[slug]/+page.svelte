<script lang="ts">
  import { goto } from "$app/navigation";
  import { resolve } from "$app/paths";
  import { fetchWithCsrf } from "$lib/csrf";
  import { onMount } from "svelte";
  import { formatDate } from "$lib/datepicker/formatDate";
  import TaskBoard from "$lib/components/task-board/task-board.svelte"

  let loading = $state(false);
  let boardView = $state(false);
  
  let listName = $state();
  let tasks = $state([]);
  let error = $state("");
  let { params } = $props();

  onMount(() => {
    GetList();
    GetTasks();
  });

  /// <Summary>
  /// Fetches tasks of the certain task list from the backend
  /// and stores them in the frontend as an array of objects
  ///
  /// <Summary>
  async function GetTasks() {
    try {
      loading = true;
      error = "";
      const response = await fetchWithCsrf(
        resolve(`/api/taskItem/${params.slug}` as any),
        {
          method: "GET",
          credentials: "include",
        },
      );
      const data = await response.json();
      if (!response.ok) {
        error = data;
        return;
      }
      tasks = data;
    } catch (err) {
      error = "Failed to get tasks: " + (err as Error).message;
    } finally {
      loading = false;
    }
  }

  /// <summary>
  /// Creates a new task list for the user with the given name. Validates the name
  /// before sending the request to the backend. If creation is successful, navigates
  /// back to the home screen. If there is an error, displays the error message.
  /// </summary>
  async function GetList() {
    try {
      loading = true;
      error = "";
      const response = await fetchWithCsrf(
        resolve(`/api/taskList/${params.slug}` as any),
        {
          method: "GET",
          credentials: "include",
        },
      );

      const data = await response.json();
      if (!response.ok) {
        error = data || "Failed to get list.";
        return;
      }
      listName = data.name;
    } catch (err) {
      error = "Failed to get list: " + (err as Error).message;
    } finally {
      loading = false;
    }
  }
  
  async function toggleBoardView() {
    if (boardView) {
      boardView = false;
    } else {
      boardView = true;
    }
  }

  /**
   * shorten the length of the displayed description to 'number' characters, add '...' onto the end of the description to indicate more.
   * @param text the description to shorten
   * @param length length of description to cut down too
   */
  function shortenDesc(text: string | null, length: number) {
    if (!text) return "No Description";
    if (text.length <= length) return text;
    return text.slice(0, length) + "...";
  }
</script>

<div class="container">
  <div style="display: flex; flex-direction: row; ">
    <h1
      class="text-break text-center mb-4"
      style="flex: 1; justify-content: center; width: 1270px"
    >
      {listName}
    </h1>
  </div>
  <div class="mb-3 card-body d-flex justify-content-between align-items-center">
    <button
      type="button"
      class="btn btn-primary"
      on:click={() =>
        goto(resolve(`/home/task-list/${params.slug}/create-task`))}
      >Add Task
    </button>
    <button type="button"
            class="btn btn-secondary"
            on:click={toggleBoardView}
    >
      See Task List
    </button>
  </div>
  {#if loading && tasks.length === 0}
    <div class="text-center text-muted py-4">Loading tasks...</div>
  {:else if tasks.length === 0}
    <div class="text-center text-muted py-4">
      No tasks yet. Create your first task above!
    </div>
  {:else}
    <div class="mb-3">
      {#if boardView}
        <TaskBoard tasks="{tasks}" />
      {:else}
        {#each tasks as task}
          <div
                  class="task-card"
                  class:status-todo={task.currentStatus === 0}
                  class:status-inprogress={task.currentStatus === 1}
                  class:status-done={task.currentStatus === 2}
                  tabindex="0"
                  role="button"
                  on:click={() =>
            goto(`/home/task-list/${params.slug}/task/${task.taskId}`)}
                  on:keydown={(e) => {
            if (e.key === "Enter" || e.key === " ") {
              goto(`/home/task-list/${params.slug}/task/${task.taskId}`);
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
              {#if task.currentStatus == 0}ToDo
              {:else if task.currentStatus === 1}In Progress
              {:else}Done{/if}
            </span>
            </div>

            <p class="task-description">{shortenDesc(task.description, 50)}</p>

            <div class="task-footer">
            <span class="due-date">
              🗓 {task.dueDate === null
                    ? "No Due Date"
                    : formatDate(task.dueDate)}
            </span>
            </div>
          </div>
        {/each}
      {/if}
    </div>
  {/if}
</div>

<style>
  .task-card {
    background: white;
    border: 1px solid lightgrey;
    border-left: 4px solid #e5e7eb;
    border-radius: 8px;
    padding: 12px 16px;
    margin-bottom: 10px;
    cursor: pointer;
    box-shadow: 0px 0px 5px lightgrey;
    transition:
      box-shadow 0.2s ease,
      transform 0.1s ease;
  }

  .task-card:hover {
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
    transform: translateY(-1px);
  }

  .task-card:focus-visible {
    box-shadow: 0 0 0 3px #4a90e2;
  }

  .status-todo {
    border-left-color: darkgray;
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
    margin-bottom: 6px;
  }

  .task-title {
    font-weight: 600;
    font-size: 1rem;
    color: black;
  }

  .task-description {
    color: grey;
    font-size: 0.875rem;
    margin-bottom: 10px;
  }

  .task-footer {
    display: flex;
    justify-content: flex-start;
  }

  .due-date {
    font-size: 0.8rem;
    color: #9ca3af;
  }

  /* Status badges */
  .status-badge {
    font-size: 0.75rem;
    font-weight: 600;
    padding: 2px 10px;
    border-radius: 999px;
  }

  .badge-todo {
    background: white;
    color: darkgray;
  }
  .badge-inprogress {
    background: white;
    color: blue;
  }
  .badge-done {
    background: white;
    color: lightgreen;
  }

  /* Board view */
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

  .board-task-card {
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

  .board-task-card:hover {
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
    transform: translateY(-1px);
  }
  
  .board-status-todo {
    border-left-color: grey;
  }
  
  .board-status-inprogress {
    border-left-color: blue;
  }
  
  .board-status-done {
    border-left-color: lightgreen;
  }
</style>
