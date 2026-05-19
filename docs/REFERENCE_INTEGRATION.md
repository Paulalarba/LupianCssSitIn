# UcCcsSitIn Reference Integration

This project now uses `Paulalarba/UcCcsSitIn` as the functional and visual foundation while preserving the current MVC app structure.

## Database Integration

The reference repository uses ASP.NET Identity and relates operational records to `ApplicationUser`. This project already uses `Student.IdNumber` as the login key, so the schema was adapted to keep that simpler authentication flow.

Added tables:

- `Announcements`: admin-published bulletins for all users or students.
- `Reservations`: lab booking requests with date, time, seat, purpose, status, and review fields.
- `SitInSessions`: active and completed sit-in records with lab, purpose, language, seat, checkout, reward, violation, and notes fields.
- `Feedback`: student feedback tied to the latest sit-in session.
- `RewardPoints`: point awards tied optionally to a sit-in session.
- `Notifications`: student-facing system messages.
- `LabRules`: ordered laboratory policy content seeded through EF Core.

Relationship mapping:

- Reference `ApplicationUser.Id` relationships were mapped to `Student.IdNumber`.
- Reservations and notifications cascade when a student is deleted.
- Sit-in sessions keep history with `NoAction` delete behavior.
- Reward points may reference a sit-in session, and each sit-in can have at most one automatic reward-point record.

The `Student` model also now tracks reference-style session metrics: `TotalSessions`, `RemainingSessions`, `TotalSessionsUsed`, `Points`, `RewardPoints`, `RegisteredAt`, `LastLoginAt`, and emergency contact fields.

## UI/UX Updates

The current login, registration, student dashboard, and admin dashboard already followed the reference repo's two-panel auth flow and sidebar dashboard layout. The update keeps that structure and improves consistency through shared color tokens.

Color changes:

- Replaced the older saturated blue/yellow scheme with OKLCH-based primary, secondary, accent, success, warning, danger, surface, border, and text roles.
- Improved focus rings and contrast for form controls and buttons.
- Removed gradient-text styling and replaced colored side stripes with full-border/tinted treatments.

Interaction updates:

- Student reservations now post to `/Student/CreateReservation`.
- Student feedback now posts to `/Student/SubmitFeedback`.
- Dashboard counts can hydrate from `/Student/GetDashboardData`.
- Existing local UI state is retained as a fallback so the dashboard remains responsive during server interruptions.

## Functionality Improvements

Implemented or enhanced:

- Login timestamps.
- Registration defaults for session allocation and welcome notifications.
- Database-backed sit-in creation with duplicate-active-session and remaining-session checks.
- Sit-in checkout with optional reward points, violations, and completion status.
- Database-backed reservations and feedback.
- Admin dashboard summary values for students, active sit-ins, pending reservations, and pending feedback.

## Maintenance Notes

When adding new student-facing features, prefer storing records in the adapted tables rather than `localStorage`. Keep `Student.IdNumber` as the relationship key unless the app is later migrated fully to ASP.NET Identity.

If the app later adopts Identity, the closest reference path is:

1. Introduce an `ApplicationUser` entity.
2. Move identity fields from `Student` to `ApplicationUser`.
3. Convert foreign keys from `Student.IdNumber` to `ApplicationUser.Id`.
4. Keep the operational tables unchanged where possible.
