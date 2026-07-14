import { createTask, EmptyTaskTitleError } from "../createTask";
import type { TaskRepositoryPort } from "../../../domain/ports/TaskRepositoryPort";
import type { Task } from "../../../domain/Task";

function createMockRepository(overrides: Partial<TaskRepositoryPort> = {}): TaskRepositoryPort {
  return {
    getAll: jest.fn().mockResolvedValue([]),
    create: jest.fn(),
    toggleCompleted: jest.fn(),
    remove: jest.fn(),
    ...overrides,
  };
}

describe("createTask", () => {
  it("delegates to the repository with a trimmed title", async () => {
    const expected: Task = { id: "1", title: "Buy milk", isCompleted: false, createdAt: "2026-01-01" };
    const repository = createMockRepository({ create: jest.fn().mockResolvedValue(expected) });

    const result = await createTask(repository, "  Buy milk  ");

    expect(repository.create).toHaveBeenCalledWith("Buy milk");
    expect(result).toEqual(expected);
  });

  it("throws EmptyTaskTitleError for blank titles and never calls the repository", async () => {
    const repository = createMockRepository();

    await expect(createTask(repository, "   ")).rejects.toThrow(EmptyTaskTitleError);
    expect(repository.create).not.toHaveBeenCalled();
  });
});
