let taskListSnapshots: Array = [];

export function saveSnapshot(snapshot: Array): void {
    if (taskListSnapshots.length > 4) {
        taskListSnapshots.shift();
    }
    
    taskListSnapshots.push(snapshot);    

    console.log(`snapshot saved:`);
    console.log(snapshot);
    
    console.log(`taskListSnapshots: `);
    console.log(taskListSnapshots)
}

export function retrieveSnapshot(): Array {
    if (taskListSnapshots.length === 0) {
        return [];
    }
    
    return taskListSnapshots.pop();
}