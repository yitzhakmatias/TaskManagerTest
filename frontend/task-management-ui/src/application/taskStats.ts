import type { Task } from "../domain/Task";

export interface TaskStats {
  total: number;
  completed: number;
  pending: number;
}

/**
 * Pure derivation over the current task list - no port, no side effects, so
 * it's trivial to unit test and doesn't need a use case of its own.
 */
export function computeTaskStats(tasks: Task[]): TaskStats {
  const completed = tasks.filter((t) => t.isCompleted).length;

  return {
    total: tasks.length,
    completed,
    pending: tasks.length - completed,
  };
}
