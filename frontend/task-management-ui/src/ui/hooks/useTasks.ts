import { useCallback, useEffect, useMemo, useState } from "react";
import type { Task } from "../../domain/Task";
import type { TaskRepositoryPort } from "../../domain/ports/TaskRepositoryPort";
import { HttpTaskRepository } from "../../infrastructure/http/HttpTaskRepository";
import { listTasks } from "../../application/useCases/listTasks";
import { createTask } from "../../application/useCases/createTask";
import { toggleTask } from "../../application/useCases/toggleTask";
import { deleteTask } from "../../application/useCases/deleteTask";

// Module-level singleton so the default adapter has a stable identity across
// renders (tests can still inject their own mock repository per call).
const defaultRepository: TaskRepositoryPort = new HttpTaskRepository();

/**
 * Primary (driving) adapter, in the hexagonal sense: this hook is what
 * translates React's world (state, renders, effects) into calls against the
 * application's use cases, and use cases into React state updates.
 */
export function useTasks(repository: TaskRepositoryPort = defaultRepository) {
  const repo = useMemo(() => repository, [repository]);
  const [tasks, setTasks] = useState<Task[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await listTasks(repo);
      setTasks(result);
    } catch {
      setError("Failed to load tasks. Is the API running?");
    } finally {
      setIsLoading(false);
    }
  }, [repo]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const addTask = useCallback(
    async (title: string) => {
      const created = await createTask(repo, title);
      setTasks((current) => [created, ...current]);
    },
    [repo]
  );

  const toggle = useCallback(
    async (id: string) => {
      const updated = await toggleTask(repo, id);
      setTasks((current) => current.map((t) => (t.id === id ? updated : t)));
    },
    [repo]
  );

  const remove = useCallback(
    async (id: string) => {
      await deleteTask(repo, id);
      setTasks((current) => current.filter((t) => t.id !== id));
    },
    [repo]
  );

  return { tasks, isLoading, error, addTask, toggle, remove, refresh };
}
