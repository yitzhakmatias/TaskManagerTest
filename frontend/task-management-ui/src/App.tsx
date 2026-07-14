import { Alert, Box, Button, CircularProgress, CssBaseline, ThemeProvider, Typography } from "@mui/material";
import { theme } from "./theme";
import { AuthProvider, useAuth } from "./ui/auth/AuthContext";
import { TaskBoard } from "./ui/TaskBoard";

function AppContent() {
  const { status, errorMessage, retry } = useAuth();

  if (status === "authenticating") {
    return (
      <Box
        sx={{
          minHeight: "100vh",
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          gap: 2,
        }}
      >
        <CircularProgress />
        <Typography color="text.secondary">Connecting...</Typography>
      </Box>
    );
  }

  if (status === "error") {
    return (
      <Box sx={{ minHeight: "100vh", display: "flex", alignItems: "center", justifyContent: "center", px: 2 }}>
        <Box sx={{ maxWidth: 380, width: "100%", textAlign: "center" }}>
          <Alert severity="error" sx={{ mb: 2 }}>
            {errorMessage ?? "Something went wrong."}
          </Alert>
          <Button variant="contained" onClick={retry}>
            Retry
          </Button>
        </Box>
      </Box>
    );
  }

  return <TaskBoard />;
}

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
