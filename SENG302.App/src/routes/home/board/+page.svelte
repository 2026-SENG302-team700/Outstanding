<script lang="ts">
  import { onMount, onDestroy } from "svelte";
  import { fetchWithCsrf } from "$lib/csrf";
  import { resolve } from "$app/paths";
  import TaskBoard from "$lib/components/task-board/task-board.svelte";
  import { clearSnapshotHistory } from "$lib/snapshot-handling/snapshot-handler";
  import type { TaskItem } from "$lib/types.js";

  let tasks = $state([]);
  let taskLists = $state([]);
  let loading = $state(false);
  let error = $state("");
  let boardUndo: (() => Promise<void>) | null = null;

  /**
   * calls the method to fetch all the tasks to populate the board with
   */
  onMount(() => {
    fetchAllTasks();
    fetchAllLists();
  });

  /**
   * calls the method within the snapshot history handler to clear the snapshot history
   */
  onDestroy(() => {
    clearSnapshotHistory();
  });

  /**
   * calls the undo method within the task-board component
   */
  async function handleUndo() {
    if (boardUndo) await boardUndo();
  }

  /**
   * Fetches all tasks from the backend
   */
  async function fetchAllTasks() {
    try {
      loading = true;
      const response = await fetchWithCsrf(`/api/taskitem`, {
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

  /**
   * Fetch all the tasklists that a user has. This is for the tasklist pill on the tasks
   */
  async function fetchAllLists() {
    try {
      const response = await fetchWithCsrf(resolve(`/api/taskList`), {
        method: "GET",
        credentials: "include",
      });
      const data = await response.json();
      if (!response.ok) return;
      taskLists = data;
    } catch (err) {
      console.error("Failed to fetch task lists", err);
    }
  }
</script>

<div class="container">
  <div class="mb-3 d-flex justify-content-end">
    <button type="button" class="btn btn-outline-warning" onclick={handleUndo}>
      Undo Last Change
    </button>
  </div>
  {#if loading}
    <div class="text-center text-muted py-4">Loading tasks...</div>
  {:else if error}
    <div class="text-danger">{error}</div>
  {:else if tasks.length === 0}
    <div class="text-center text-muted py-4">No tasks found.</div>
  {:else}
    <TaskBoard {tasks} {taskLists} onUndoReady={(fn) => (boardUndo = fn)} />
  {/if}
</div>
