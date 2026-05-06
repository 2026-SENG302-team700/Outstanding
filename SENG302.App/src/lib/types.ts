export interface Book {
	id: number;
	title: string;
	author: string;
	year: number;
}

export type TaskItem = {
	taskIndex: number;
	taskId: number;
	taskListId: number;
	name: string;
	description: string;
	dueDate: Date;
	currentStatus: number;
	creationTime: Date;
}