import type { Task } from "../Task";

/**
 * Secondary (driven) port. The application core depends on this interface
 * only - it has no idea whether tasks are fetched over HTTP, from local
 * storage, or from an in-memory fake used in tests.
 */
export interface TaskRepositoryPort {
  getAll(): Promise<Task[]>;
  create(title: string): Promise<Task>;
  toggleCompleted(id: string): Promise<Task>;
  remove(id: string): Promise<void>;
}
