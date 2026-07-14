/**
 * Core domain type. Framework-agnostic (no React, no fetch, no MUI) - this is
 * the center of the hexagon that both the UI and the HTTP adapter depend on.
 */
export interface Task {
  id: string;
  title: string;
  isCompleted: boolean;
  createdAt: string;
}
