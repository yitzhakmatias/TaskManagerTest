import { Stack, Typography } from "@mui/material";
import type { Task } from "../../domain/Task";
import { TaskListItem } from "./TaskListItem";

interface TaskListProps {
  tasks: Task[];
  onToggle: (id: string) => void;
  onDelete: (id: string) => void;
}

export function TaskList({ tasks, onToggle, onDelete }: TaskListProps) {
  if (tasks.length === 0) {
    return <Typography color="text.secondary">No tasks yet. Add one above.</Typography>;
  }

  return (
    <Stack spacing={1.5}>
      {tasks.map((task) => (
        <TaskListItem key={task.id} task={task} onToggle={onToggle} onDelete={onDelete} />
      ))}
    </Stack>
  );
}
