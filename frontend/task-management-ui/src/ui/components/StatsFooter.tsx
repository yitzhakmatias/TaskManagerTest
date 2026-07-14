import { Box, Paper, Typography } from "@mui/material";
import type { TaskStats } from "../../application/taskStats";

interface StatsFooterProps {
  stats: TaskStats;
}

interface StatCardProps {
  value: number;
  label: string;
}

function StatCard({ value, label }: StatCardProps) {
  return (
    <Paper
      variant="outlined"
      sx={{
        flex: 1,
        py: 2.5,
        textAlign: "center",
        borderColor: "divider",
      }}
    >
      <Typography variant="h5" sx={{ fontWeight: 700 }}>
        {value}
      </Typography>
      <Typography
        variant="caption"
        color="text.secondary"
        sx={{ letterSpacing: 1, textTransform: "uppercase" }}
      >
        {label}
      </Typography>
    </Paper>
  );
}

export function StatsFooter({ stats }: StatsFooterProps) {
  return (
    <Box sx={{ display: "flex", gap: 2, mt: 3 }}>
      <StatCard value={stats.total} label="Total" />
      <StatCard value={stats.completed} label="Completed" />
      <StatCard value={stats.pending} label="Pending" />
    </Box>
  );
}
