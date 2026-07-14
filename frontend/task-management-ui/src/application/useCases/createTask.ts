import type { TaskRepositoryPort } from "../../domain/ports/TaskRepositoryPort";
import type { Task } from "../../domain/Task";

export class EmptyTaskTitleError extends Error {
  constructor() {
    super("Task title cannot be empty.");
    this.name = "EmptyTaskTitleError";
  }
}

export const createTask = async (repository: TaskRepositoryPort, title: string): Promise<Task> => {
  const trimmed = title.trim();
  if (!trimmed) {
    throw new EmptyTaskTitleError();
  }
  return repository.create(trimmed);
};
