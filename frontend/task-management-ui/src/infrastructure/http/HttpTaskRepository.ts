import type { TaskRepositoryPort } from "../../domain/ports/TaskRepositoryPort";
import type { Task } from "../../domain/Task";
import { apiFetch } from "./apiClient";

/** Shape returned by the backend - kept separate from the domain Task type. */
interface TaskResponseDto {
  id: string;
  title: string;
  isCompleted: boolean;
  createdAt: string;
}

const toTask = (dto: TaskResponseDto): Task => ({
  id: dto.id,
  title: dto.title,
  isCompleted: dto.isCompleted,
  createdAt: dto.createdAt,
});

/**
 * Secondary (driven) adapter: implements the TaskRepositoryPort against the
 * real REST API. Swapping this for a different adapter (e.g. an in-memory
 * fake) requires no change anywhere else in the app.
 */
export class HttpTaskRepository implements TaskRepositoryPort {
  async getAll(): Promise<Task[]> {
    const dtos = await apiFetch<TaskResponseDto[]>("/tasks");
    return dtos.map(toTask);
  }

  async create(title: string): Promise<Task> {
    const dto = await apiFetch<TaskResponseDto>("/tasks", {
      method: "POST",
      body: JSON.stringify({ title }),
    });
    return toTask(dto);
  }

  async toggleCompleted(id: string): Promise<Task> {
    const dto = await apiFetch<TaskResponseDto>(`/tasks/${id}/toggle`, {
      method: "PATCH",
    });
    return toTask(dto);
  }

  async remove(id: string): Promise<void> {
    await apiFetch<void>(`/tasks/${id}`, { method: "DELETE" });
  }
}
