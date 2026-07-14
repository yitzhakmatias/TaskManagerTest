import { computeTaskStats } from "../taskStats";
import type { Task } from "../../domain/Task";

const makeTask = (isCompleted: boolean): Task => ({
  id: Math.random().toString(),
  title: "task",
  isCompleted,
  createdAt: "2026-01-01T00:00:00Z",
});

describe("computeTaskStats", () => {
  it("returns zeros for an empty list", () => {
    expect(computeTaskStats([])).toEqual({ total: 0, completed: 0, pending: 0 });
  });

  it("counts total, completed, and pending correctly", () => {
    const tasks = [makeTask(true), makeTask(false), makeTask(true), makeTask(false), makeTask(false)];

    expect(computeTaskStats(tasks)).toEqual({ total: 5, completed: 2, pending: 3 });
  });
});
