export type TaskSnapshot = {
  taskId: number;
  status: number;
};

type snapshotRetrieval = {
  snapshot: Array<TaskSnapshot>;
  snapshotFlag: boolean;
};

let taskListSnapshots: Array<Array<TaskSnapshot>> = [];
let hasHistoryHappened: boolean = false;

/**
 * Adds the snapshot to the taskListSnapshots and also maintains that the list is only 5 items long
 * @param snapshot number array at a point in history
 */
export function saveSnapshot(snapshot: Array<TaskSnapshot>): void {
  if (taskListSnapshots.length > 4) {
    taskListSnapshots.shift();
  }

  taskListSnapshots.push(snapshot.map((s) => ({ ...s })));
  hasHistoryHappened = true;
}
/**
 * Returns snapshot if avaliable if not returns a empty list with associated flag
 */
export function retrieveSnapshot(): snapshotRetrieval {
  let snapshot: snapshotRetrieval;
  if (taskListSnapshots.length === 0) {
    snapshot = { snapshot: [], snapshotFlag: hasHistoryHappened };
  } else {
    snapshot = { snapshot: taskListSnapshots.pop()!, snapshotFlag: true };
  }
  return snapshot;
}

/**
 * Clears snapshot history
 */
export function clearSnapshotHistory(): void {
  hasHistoryHappened = false;
  taskListSnapshots = [];
}
