/**
 * Mirrors backend/TaskManagement.Api/appsettings.json's DemoUser section.
 * There is no user-facing sign-in screen - the frontend authenticates
 * automatically on startup with this single demo account, purely so the
 * JWT-protected API can still be exercised. See README trade-offs.
 */
export const DEMO_USERNAME = "admin";
export const DEMO_PASSWORD = "admin123!";
