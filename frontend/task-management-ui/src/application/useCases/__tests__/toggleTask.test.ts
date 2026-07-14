import { toggleTask } from "../toggleTask";
import type { TaskRepositoryPort } from "../../../domain/ports/TaskRepositoryPort";
import type { Task } from "../../../domain/Task";

describe("toggleTask", () => {
  it("delegates to the repository", async () => {
    const expected: Task = { id: "1", title: "X", isCompleted: true, createdAt: "2026-01-01" };
    const repository: TaskRepositoryPort = {
      getAll: jest.fn(),
      create: jest.fn(),
      toggleCompleted: jest.fn().mockResolvedValue(expected),
      remove: jest.fn(),
    };

    const result = await toggleTask(repository, "1");

    expect(repository.toggleCompleted).toHaveBeenCalledWith("1");
    expect(result).toEqual(expected);
  });
});
