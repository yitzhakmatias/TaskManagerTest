import { render, screen } from "@testing-library/react";
import { TaskList } from "../TaskList";
import type { Task } from "../../../domain/Task";

describe("TaskList", () => {
  it("shows an empty state when there are no tasks", () => {
    render(<TaskList tasks={[]} onToggle={jest.fn()} onDelete={jest.fn()} />);

    expect(screen.getByText(/no tasks yet/i)).toBeInTheDocument();
  });

  it("renders one row per task", () => {
    const tasks: Task[] = [
      { id: "1", title: "First", isCompleted: false, createdAt: "2026-01-01" },
      { id: "2", title: "Second", isCompleted: true, createdAt: "2026-01-02" },
    ];

    render(<TaskList tasks={tasks} onToggle={jest.fn()} onDelete={jest.fn()} />);

    expect(screen.getByText("First")).toBeInTheDocument();
    expect(screen.getByText("Second")).toBeInTheDocument();
  });
});
