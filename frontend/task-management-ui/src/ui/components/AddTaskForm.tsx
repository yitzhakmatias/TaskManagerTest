import { useState, type FormEvent } from "react";
import { Box, Button, TextField } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";

interface AddTaskFormProps {
  onAdd: (title: string) => Promise<void>;
}

export function AddTaskForm({ onAdd }: AddTaskFormProps) {
  const [title, setTitle] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    const trimmed = title.trim();
    if (!trimmed || isSubmitting) {
      return;
    }

    setIsSubmitting(true);
    try {
      await onAdd(trimmed);
      setTitle("");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ display: "flex", gap: 1.5, mb: 3 }}>
      <TextField
        placeholder="Add a new task..."
        fullWidth
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        disabled={isSubmitting}
        slotProps={{ htmlInput: { "aria-label": "New task" } }}
      />
      <Button
        type="submit"
        variant="contained"
        startIcon={<AddIcon />}
        disabled={isSubmitting || !title.trim()}
        sx={{ px: 3, whiteSpace: "nowrap" }}
      >
        Add
      </Button>
    </Box>
  );
}
