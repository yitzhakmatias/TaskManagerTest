import { Alert, Box, CircularProgress, Container, Typography } from "@mui/material";
import { useTasks } from "./hooks/useTasks";
import { AddTaskForm } from "./components/AddTaskForm";
import { TaskList } from "./components/TaskList";
import { StatsFooter } from "./components/StatsFooter";
import { computeTaskStats } from "../application/taskStats";

export function TaskBoard() {
  const { tasks, isLoading, error, addTask, toggle, remove } = useTasks();
  const stats = computeTaskStats(tasks);

  return (
    <Container maxWidth="sm" sx={{ py: 6 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" color="primary.light" sx={{ fontWeight: 700 }}>
          Task Manager
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Organize your tasks with React and TypeScript
        </Typography>
      </Box>

      <AddTaskForm onAdd={addTask} />

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {isLoading ? (
        <Box sx={{ display: "flex", justifyContent: "center", py: 4 }}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          <TaskList tasks={tasks} onToggle={toggle} onDelete={remove} />
          <StatsFooter stats={stats} />
        </>
      )}
    </Container>
  );
}
