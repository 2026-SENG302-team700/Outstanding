<script lang="ts">
  import { goto } from "$app/navigation";
  import { resolve } from "$app/paths";
  import { fetchWithCsrf } from "$lib/csrf";
  import { onMount, onDestroy } from "svelte";
  import { formatDate } from "$lib/datepicker/formatDate";
  import TaskBoard from "$lib/components/task-board/task-board.svelte";
  import TaskItemComponent from "$lib/components/task-item.svelte";
  import { move } from "@dnd-kit/helpers";
  import { DragDropProvider } from "@dnd-kit/svelte";
  import type { TaskItem } from "$lib/types.js";
  import {
    saveSnapshot,
    retrieveSnapshot,
    clearSnapshotHistory,
  } from "$lib/snapshot-handling/snapshot-handler";
  import { addToast, toasts } from "$lib/toast/toast.js";
  import { page } from "$app/state";
  import { flip } from "svelte/animate";

  let loading = $state(false);
  let boardView = $state(false);
  let inUndoAnimation = $state(false);

  let listName = $state();
  let taskRefs: number[] = $state([]);
  let tasks: TaskItem[] = $state([]);
  let error = $state("");
  let { params } = $props();

  let snapshot: number[] = [];

  onMount(() => {
    GetList();
    GetTasks();
  });

  onDestroy(() => {
    clearSnapshotHistory();
  });

  /**
   * Trigger when url changes, checks params to see what page view needs to be loaded
   */
  $effect(() => {
    let query = page.url.searchParams.get("mode");
    if (query != "board-view") {
      boardView = false;
    } else {
      boardView = true;
    }
    GetTasks();
  });

  /**
   * Creates a snapshot of the original ordering of list of items before the items are dragged
   */
  function onDragStart() {
    snapshot = taskRefs.slice();
  }

  /**
   * Changes the task-ref list ordering based on where the task has been moved to
   * @param event
   */
  function onDragOver(event: any) {
    taskRefs = move(taskRefs, event);
  }

  /**
   * If the drag event is cancelled, resets the taskRefs to how the task board looked before the drag and drop
   * @param event
   */
  async function onDragEnd(event: any) {
    if (event.canceled) {
      taskRefs = snapshot;
    }
    saveSnapshot(snapshot);
    await reorderReloadTasks();
  }

  /**
   * get the past history and show the corrosponding toast depending on the situation
   */
  async function handleUndo() {
    let retrievedSnapshot = retrieveSnapshot();
    if (retrievedSnapshot.snapshot.length === 0) {
      if (retrievedSnapshot.snapshotFlag === false) {
        addToast("No sorting to undo", "error");
        return;
      }
      if (retrievedSnapshot.snapshotFlag === true) {
        addToast("Cannot undo more than 5 sorting", "error");
        return;
      }
    }

    inUndoAnimation = true;
    taskRefs = retrievedSnapshot.snapshot;
    await reorderReloadTasks();
    inUndoAnimation = false;
  }

  /**
   * Fetches tasks of the certain task list from the backend
   *  and stores them in the frontend as an array of objects
   */
  async function GetTasks() {
    try {
      loading = true;
      error = "";
      const response = await fetchWithCsrf(
        resolve(`/api/taskItem/${params.slug}`),
        {
          method: "GET",
          credentials: "include",
          headers: {
            "Cache-Control": "no-cache",
          },
        },
      );
      const data = await response.json();
      if (!response.ok) {
        error = data;
        return;
      }
      tasks = data;
      taskRefs = data.map((task) => task.taskId);
    } catch (err) {
      error = "Failed to get tasks: " + (err as Error).message;
    } finally {
      loading = false;
    }
  }

  /**
   * Creates a new task list for the user with the given name. Validates the name
   * before sending the request to the backend. If creation is successful, navigates
   * back to the home screen. If there is an error, displays the error message.
   */
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

  /**
   * Sends ordering information to backend to persist dnd changes.
   */
  async function reorderReloadTasks(): void {
    try {
      await fetchWithCsrf(resolve("/api/taskItem/order"), {
        method: "PATCH",
        credentials: "include",
        headers: { "content-type": "application/json" },
        body: JSON.stringify(taskRefs),
      });
    } catch (err) {
      console.error(err);
      addToast("An error occurred.", "error");
    }
  }

  /**
   * Changes the url to add board view as a param
   * Gets updated task list (for task ordering) then toggles boardview on or off depending on its previous state.
   */
  async function toggleBoardView() {
    clearSnapshotHistory();
    GetTasks();
    let query = page.url.searchParams.get("mode");
    if (query == "board-view") {
      goto(resolve(`/home/task-list/${params.slug}`));
    } else {
      goto(resolve(`/home/task-list/${params.slug}/?mode=board-view`));
    }
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
    <div>
      <button
        type="button"
        class="btn btn-outline-info"
        on:click={toggleBoardView}
      >
        See Task List
      </button>
      <button
        type="button"
        class="btn btn-outline-warning"
        on:click={handleUndo}
      >
        Undo Last Sorting
      </button>
    </div>
  </div>
  {#if loading && Object.keys(tasks).length === 0}
    <div class="text-center text-muted py-4">Loading tasks...</div>
  {:else if Object.keys(tasks).length === 0}
    <div class="text-center text-muted py-4">
      No tasks yet. Create your first task above!
    </div>
  {:else}
    <div class="mb-3">
      {#if boardView}
        <TaskBoard {tasks} />
      {:else}
        <DragDropProvider {onDragStart} {onDragOver} {onDragEnd}>
          <ul class="list">
            {#each taskRefs as taskRef, index (taskRef)}
              <div
                animate:flip={inUndoAnimation
                  ? { duration: 200 }
                  : { duration: 0 }}
              >
                <TaskItemComponent
                  id={taskRef}
                  task={tasks.find((u) => u.taskId === taskRef)}
                  {index}
                />
              </div>
            {/each}
          </ul>
        </DragDropProvider>
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
