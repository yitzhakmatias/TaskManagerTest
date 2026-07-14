import type { TaskRepositoryPort } from "../../domain/ports/TaskRepositoryPort";
import type { Task } from "../../domain/Task";

export const listTasks = (repository: TaskRepositoryPort): Promise<Task[]> => {
  return repository.getAll();
};
