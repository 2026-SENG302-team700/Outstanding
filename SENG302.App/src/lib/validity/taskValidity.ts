export interface TaskValidityErrors {
    isValid: boolean,
    name: string,
    description: string,
    dueDate: string,
    taskStatus: string,
    dateComparer:string,
}

/**
 * Enforces user inputs through front-end are valid with the backend.
 *
 * @param name Task Name
 * @param description Task Description
 * @param dueDate Due Date of the Task of type Date
 * @param taskStatus Task Status (0: Todo, 1: In Progress, 2: Done).
 */
export function validateTaskInput(
    name: string,
    description: string,
    dueDate: Date,
    taskStatus: number
): TaskValidityErrors {
    let validityErrors: TaskValidityErrors = {
        isValid: true,
        name: undefined,
        description: undefined,
        dueDate: undefined,
        taskStatus: undefined
    };
    
    // Check name and description length
    if (name.trim().length > 128 || name.trim().length < 3) {
        validityErrors.name =
            "Title is required and must be between 3 and 128 characters long";
        validityErrors.isValid = false;
    }
    
    if (description.trim().length > 2048) {
        validityErrors.description = "Description must be 2048 characters or less";
        validityErrors.isValid = false;
    }

    
    
    // Check date validity
    let taskDueDate = new Date(dueDate).getTime();
    let now = new Date().getTime();
    let nullDate = new Date("9999-99-99").getTime();
    if (now > taskDueDate && taskDueDate != nullDate) {
        validityErrors.dueDate = "Invalid due date, date must be in the future";
        validityErrors.isValid = false;
    }
    
    // Check task status -- Shouldn't occur without user modifying code.
    if (![0, 1, 2].includes(taskStatus)) {
        console.log(taskStatus);
        validityErrors.taskStatus = "Invalid task status. Refresh Webpage";
        validityErrors.isValid = false;
    }

    return validityErrors;
}