import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AddTaskForm } from "../AddTaskForm";

describe("AddTaskForm", () => {
  it("calls onAdd with the trimmed title and clears the input", async () => {
    const user = userEvent.setup();
    const onAdd = jest.fn().mockResolvedValue(undefined);

    render(<AddTaskForm onAdd={onAdd} />);

    const input = screen.getByLabelText(/new task/i);
    await user.type(input, "  Buy milk  ");
    await user.click(screen.getByRole("button", { name: /add/i }));

    expect(onAdd).toHaveBeenCalledWith("Buy milk");
  });

  it("disables the submit button when the input is empty", () => {
    render(<AddTaskForm onAdd={jest.fn()} />);

    expect(screen.getByRole("button", { name: /add/i })).toBeDisabled();
  });
});
