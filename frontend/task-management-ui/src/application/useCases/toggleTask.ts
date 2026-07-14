import type { TaskRepositoryPort } from "../../domain/ports/TaskRepositoryPort";
import type { Task } from "../../domain/Task";

export const toggleTask = (repository: TaskRepositoryPort, id: string): Promise<Task> => {
  return repository.toggleCompleted(id);
};
