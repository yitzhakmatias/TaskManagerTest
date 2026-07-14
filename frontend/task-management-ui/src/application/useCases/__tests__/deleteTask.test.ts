import { deleteTask } from "../deleteTask";
import type { TaskRepositoryPort } from "../../../domain/ports/TaskRepositoryPort";

describe("deleteTask", () => {
  it("delegates to the repository", async () => {
    const repository: TaskRepositoryPort = {
      getAll: jest.fn(),
      create: jest.fn(),
      toggleCompleted: jest.fn(),
      remove: jest.fn().mockResolvedValue(undefined),
    };

    await deleteTask(repository, "42");

    expect(repository.remove).toHaveBeenCalledWith("42");
  });
});
