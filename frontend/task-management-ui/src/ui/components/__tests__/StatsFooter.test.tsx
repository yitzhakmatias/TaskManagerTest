import { render, screen } from "@testing-library/react";
import { StatsFooter } from "../StatsFooter";

describe("StatsFooter", () => {
  it("renders total, completed, and pending counts", () => {
    render(<StatsFooter stats={{ total: 5, completed: 2, pending: 3 }} />);

    expect(screen.getByText("5")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
    expect(screen.getByText("Total")).toBeInTheDocument();
    expect(screen.getByText("Completed")).toBeInTheDocument();
    expect(screen.getByText("Pending")).toBeInTheDocument();
  });
});
