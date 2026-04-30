export interface Book {
	id: number;
	title: string;
	author: string;
	year: number;
}

export interface TaskItem {
	taskId: number;
	taskListId: number;
	name: string;
	description: string;
	dueDate: Date;
	currentTaskStatus: number;
	creationTime: Date;
}