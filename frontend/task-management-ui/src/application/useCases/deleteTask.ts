import type { TaskRepositoryPort } from "../../domain/ports/TaskRepositoryPort";

export const deleteTask = (repository: TaskRepositoryPort, id: string): Promise<void> => {
  return repository.remove(id);
};
