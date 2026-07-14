import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { TaskListItem } from "../TaskListItem";
import type { Task } from "../../../domain/Task";

const task: Task = { id: "1", title: "Buy milk", isCompleted: false, createdAt: "2026-01-01" };

describe("TaskListItem", () => {
  it("calls onToggle when the checkbox is clicked", async () => {
    const user = userEvent.setup();
    const onToggle = jest.fn();

    render(<TaskListItem task={task} onToggle={onToggle} onDelete={jest.fn()} />);
    await user.click(screen.getByRole("checkbox"));

    expect(onToggle).toHaveBeenCalledWith("1");
  });

  it("calls onDelete when the delete button is clicked", async () => {
    const user = userEvent.setup();
    const onDelete = jest.fn();

    render(<TaskListItem task={task} onToggle={jest.fn()} onDelete={onDelete} />);
    await user.click(screen.getByRole("button", { name: /delete buy milk/i }));

    expect(onDelete).toHaveBeenCalledWith("1");
  });
});
