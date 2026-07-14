import { createTheme } from "@mui/material/styles";

/**
 * Dark, indigo-accented theme (cards, rounded inputs/buttons, subdued
 * secondary text) matching the app's visual design reference.
 */
export const theme = createTheme({
  palette: {
    mode: "dark",
    primary: {
      main: "#7c6cf0",
      light: "#9d90f5",
      dark: "#5d4fd6",
      contrastText: "#ffffff",
    },
    error: {
      main: "#ef4444",
    },
    background: {
      default: "#0a0a12",
      paper: "#15151f",
    },
    text: {
      primary: "#e8e8ef",
      secondary: "#9497a8",
    },
    divider: "rgba(255, 255, 255, 0.08)",
  },
  shape: {
    borderRadius: 14,
  },
  typography: {
    fontFamily: [
      "Inter",
      "Roboto",
      "-apple-system",
      "BlinkMacSystemFont",
      "sans-serif",
    ].join(","),
    h4: {
      fontWeight: 700,
    },
  },
  components: {
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: "none",
        },
      },
    },
    MuiTextField: {
      defaultProps: {
        variant: "outlined",
      },
    },
    MuiOutlinedInput: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          backgroundColor: "#0f0f18",
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 10,
          textTransform: "none",
          fontWeight: 600,
        },
      },
    },
  },
});
