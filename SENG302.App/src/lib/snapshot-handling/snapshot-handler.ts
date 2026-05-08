let taskListSnapshots: Array<Array<number>> = [];
let hasHistoryHappened: boolean = false;
type snapshotRetrieval = {
  snapshot: Array<number>;
  snapshotFlag: boolean;
};
/**
 * Adds the snapshot to the taskListSnapshots and also maintains that the list is only 5 items long
 * @param snapshot number array at a point in history
 */
export function saveSnapshot(snapshot: Array<number>): void {
  if (taskListSnapshots.length > 4) {
    taskListSnapshots.shift();
  }

  taskListSnapshots.push(snapshot);
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