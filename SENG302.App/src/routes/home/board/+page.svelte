<script lang="ts">
  import { onMount } from "svelte";
  import { fetchWithCsrf } from "$lib/csrf";
  import { resolve } from "$app/paths";
  import TaskBoard from "$lib/components/task-board/task-board.svelte"
  import type { TaskItem } from "$lib/types.js";

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
    <TaskBoard tasks={tasks} />
  {/if}
</div>
