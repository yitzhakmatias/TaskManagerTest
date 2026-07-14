import { Box, Checkbox, IconButton, Paper, Typography } from "@mui/material";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutlineOutlined";
import type { Task } from "../../domain/Task";

interface TaskListItemProps {
  task: Task;
  onToggle: (id: string) => void;
  onDelete: (id: string) => void;
}

export function TaskListItem({ task, onToggle, onDelete }: TaskListItemProps) {
  return (
    <Paper
      variant="outlined"
      sx={{
        display: "flex",
        alignItems: "center",
        px: 1.5,
        py: 0.5,
        borderColor: "divider",
      }}
    >
      <Checkbox
        checked={task.isCompleted}
        onChange={() => onToggle(task.id)}
        slotProps={{ input: { "aria-label": `Toggle ${task.title}` } }}
      />
      <Box
        onClick={() => onToggle(task.id)}
        sx={{ flexGrow: 1, cursor: "pointer", py: 1 }}
      >
        <Typography
          sx={{
            textDecoration: task.isCompleted ? "line-through" : "none",
            color: task.isCompleted ? "text.secondary" : "text.primary",
          }}
        >
          {task.title}
        </Typography>
      </Box>
      <IconButton
        edge="end"
        aria-label={`Delete ${task.title}`}
        onClick={() => onDelete(task.id)}
        sx={{ color: "error.main" }}
      >
        <DeleteOutlineIcon />
      </IconButton>
    </Paper>
  );
}
