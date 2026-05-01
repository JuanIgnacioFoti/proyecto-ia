# UX Checklist: Esports Tournament Management Platform

**Purpose**: Validates whether UX-relevant requirements in the feature spec are complete, clear, consistent, and measurable — covering all user-facing flows (registration, tournament management, team management, match recording, standings) and real-time update behavior.
**Created**: 2026-04-20
**Feature**: [spec.md](../spec.md)
**Audience**: Specification author (self-review before handoff)
**Focus areas**: Form & validation UX, navigation, role-differentiated views, real-time updates, accessibility, edge/empty states

---

## Requirement Completeness

- [x] CHK001 — Are UX requirements defined for the full Player registration form: field labels, validation feedback display, success state, and the action that follows successful registration (e.g., auto-login, redirect)? [Completeness, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Single-page form; API 400 errors mapped to inline field messages; success = auto-login + redirect to home.
- [x] CHK002 — Are UX requirements defined for the Organizer registration form, specifically how password complexity rules are communicated before and during input? [Completeness, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Password rules shown as static helper text below the field; real-time validation highlights unsatisfied rules.
- [x] CHK003 — Are loading-state requirements specified for all data-fetching interactions — tournament list, standings table, match history — when data has not yet arrived? [Completeness, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Spinner or skeleton placeholder shown during all data-fetching interactions.
- [x] CHK004 — Are UX requirements defined for the team invitation flow from the invited Player's perspective: where they receive notifications, how they accept or decline, and what they see after acting? [Completeness, Spec §FR-017–FR-018]
  > **Cross-check:** ACCEPTED (academic scope) — GET /api/players/me/invitations (contracts/teams.md) powers an invitation inbox in the Player dashboard; accept/decline buttons per invitation row.
- [x] CHK005 — Are requirements defined for the tournament detail page layout: how the registered-teams list, match history section, and standings table are organized and navigated (tabs, sections, separate pages)? [Completeness, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Three sections on one page: registered teams list, match history table, standings table (collapsible panels or tabs).
- [x] CHK006 — Are requirements defined for how each tournament lifecycle state (Draft / Open / In Progress / Completed / Suspended) is visually distinguished in listings and detail views? [Completeness, Spec §FR-012]
  > **Cross-check:** ACCEPTED (academic scope) — Each status displayed as a text badge (not color-only). Status labels: Draft, Open, In Progress, Completed, Suspended.
- [x] CHK007 — Are UX requirements specified for the Admin user management interface: listing users, role editing, and the suspend/reinstate Organizer actions? [Completeness, Spec §FR-008]
  > **Cross-check:** ACCEPTED (academic scope) — Paginated user list (contracts/users.md); per-row actions: change role, suspend, reinstate. Admin role protected from self-modification.

## Requirement Clarity

- [x] CHK008 — Is the "registration within 2 minutes" success criterion (SC-001) backed by concrete UX flow requirements — e.g., step count, single-page vs. multi-step form, minimum click count — or is it only a post-hoc outcome metric? [Clarity, Spec §SC-001]
  > **Cross-check:** ACCEPTED (academic scope) — SC-001 is an outcome metric verified by a manual walkthrough. The registration flow is a single-page form (one POST per user type) with no multi-step wizard required.
- [x] CHK009 — Is "within 5 minutes" for tournament creation (SC-002) translated into specific UX flow requirements, or does the spec leave the number of steps and screens unspecified? [Clarity, Spec §SC-002]
  > **Cross-check:** ACCEPTED (academic scope) — SC-002 is an outcome metric. Tournament creation is a single-page form (one POST to /api/tournaments) with no multi-step wizard required.
- [x] CHK010 — Is "appears publicly" (User Story 2) defined with a concrete UX location — dedicated listing page, homepage section, or search results — or is the presentation left unspecified? [Clarity, Spec §FR-015]
  > **Cross-check:** PASS — contracts/tournaments.md defines GET /api/tournaments list endpoint; Angular renders this as a dedicated public tournament listing page.
- [x] CHK011 — Is "update the standings and match list in place" (FR-028) specified with sufficient UX detail — e.g., whether a visual indicator (row highlight, animation, toast notification) must accompany the in-place update? [Clarity, Spec §FR-028]
  > **Cross-check:** RESOLVED — CHK029 already resolved this: FR-028 now requires a brief visual indication (e.g., row highlight or transient notification) on update receipt. CHK011 is fully answered by the same resolution.
- [x] CHK012 — Is "within a short time" from User Story 5 Scenario 4 fully superseded and replaced by the 5-second SLA from SC-004, or does the looser phrasing introduce ambiguity in acceptance criteria? [Clarity, Spec §US-5 vs §SC-004]
  > **Cross-check:** RESOLVED — US-5 Scenario 4 updated to read "within 5 seconds"; role also aligned to "authenticated user". Language now consistent with SC-004.

## Requirement Consistency

- [x] CHK013 — Are standings table visibility requirements consistent between User Story 5 ("Authenticated Players") and FR-039 ("any authenticated user")? Does the spec clarify whether Organizers and Admins also see standings, or is the role inconsistency unresolved? [Consistency, Spec §FR-039 vs §US-5]
  > **Cross-check:** RESOLVED — US-5 Scenario 3 changed from "authenticated Player" to "authenticated user". All three locations (US-5, FR-039, Assumptions) now agree.
- [x] CHK014 — Is the term "match history" used to describe the same data set (columns, scope) across all spec sections, or does its definition vary between User Story 5 and FR-015? [Consistency, Spec §FR-015 vs §US-5]
  > **Cross-check:** PASS — Both FR-015 and US-5 Scenario 2 define match history identically as "home team, away team, score, date/time." No inconsistency. Item closed.
- [x] CHK015 — Are validation error presentation requirements consistent across all forms — does the spec specify inline field-level error display for registration, tournament creation, and match recording forms, or are error-display patterns left to implementation? [Consistency, Spec §FR-041]
  > **Cross-check:** ACCEPTED (academic scope) — API 400 responses use `{errors: {field: [messages]}}` (FR-041/FR-042). Angular reactive forms must map server errors to the corresponding field controls and show inline messages. Same pattern applies across all forms.
- [x] CHK016 — Does the spec define consistent post-success navigation behavior (redirect destination, confirmation message) for all state-changing actions — registration, tournament creation, match recording, captaincy transfer — or are some left undefined? [Consistency, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Standard convention: redirect to the created/updated resource detail page. A success toast notification is shown for non-navigational actions (match recording, captaincy transfer).

## Acceptance Criteria Quality

- [x] CHK017 — Is the measurement method for SC-001 ("registration within 2 minutes") defined — e.g., timed usability walkthrough, step count limit — so that the criterion can be objectively verified? [Measurability, Spec §SC-001]
  > **Cross-check:** ACCEPTED (academic scope) — SC-001 verified by manual walkthrough of the registration flow. Aligns with CHK008 resolution: single-page form, no multi-step wizard.
- [x] CHK018 — Are acceptance criteria defined for what the UI must show when a server-side validation error is returned — including which field is highlighted, the error message copy source, and whether the form retains user input? [Acceptance Criteria, Spec §FR-041, §TR-001]
  > **Cross-check:** ACCEPTED (academic scope) — On server error, Angular form retains user input and maps `{errors: {field: [messages]}}` to inline field error messages. The message copy comes from the API response body.
- [x] CHK019 — Are visual acceptance criteria defined for the standings table beyond the column list in FR-039 — e.g., sort order indicator, tied-rank visual treatment, column header labels? [Acceptance Criteria, Spec §FR-039]
  > **Cross-check:** ACCEPTED (academic scope) — Columns: Position, Team, Points, MP, W, D, L (standard competition table headers). Sort is by Points DESC (server-computed). Tied teams share the same position value per CHK027/FR-031.
- [x] CHK020 — Are acceptance criteria defined for the tournament listing page — e.g., what attributes each tournament entry shows (name, status, videogame, dates) and in what order they are displayed? [Acceptance Criteria, Spec §FR-015]
  > **Cross-check:** PASS — contracts/tournaments.md GET /api/tournaments response schema defines: name, videogame, organizerName, status, startDate, estimatedEndDate, registeredTeamsCount, maxTeams.

## Scenario Coverage — Primary & Alternate Flows

- [x] CHK021 — Are UX requirements defined for the complete happy-path flows of each primary persona: Player (register → create team → join tournament) and Organizer (register → create tournament → record match result)? [Coverage, Gap]
  > **Cross-check:** PASS — User Stories 1–6 in spec.md document both complete persona flows end-to-end. Angular feature modules (auth, teams, tournaments, matches) map directly to these flows.
- [x] CHK022 — Is the UX flow for correcting a previously recorded match result specified from the Organizer's perspective — how they locate the result, enter edit mode, and confirm the correction? [Coverage, Spec §FR-027]
  > **Cross-check:** ACCEPTED (academic scope) — Match list on tournament detail page; Organizer clicks a match to open inline edit form; submits correction via PUT /api/tournaments/{id}/matches/{matchId}.
- [x] CHK023 — Are UX requirements defined for when tournament registration is rejected because the maximum slot count is reached — what message or state is shown to the Captain at the point of failure? [Coverage, Spec §FR-022]
  > **Cross-check:** ACCEPTED (academic scope) — API returns 422 with `{message}` on slot exhaustion; Angular displays the message inline at the registration action button.
- [x] CHK024 — Are UX requirements defined for the Suspended tournament state from both the public visitor's and the owning Organizer's perspectives — what is shown, what is hidden, and are restricted actions disabled or absent? [Coverage, Spec §FR-012]
  > **Cross-check:** RESOLVED — FR-012 Suspended bullet updated: previously visible tournaments remain in public listings with a "Suspended" badge; all mutating actions remain blocked.
- [x] CHK025 — Are UX requirements defined for an expired team invitation — is the expired invitation still visible to the recipient, and how is its expired status communicated? [Coverage, Spec Assumptions §Invitation model]
  > **Cross-check:** ACCEPTED (academic scope) — Expired invitations remain visible in the player invitation inbox with status "Expired" (from the `expiresAt` field in contracts/teams.md); accept/decline actions are disabled.

## Edge Case & Boundary Requirements

- [x] CHK026 — Are UX requirements specified for zero-state scenarios: empty tournament list, team with no members yet, tournament with no matches recorded, and standings table before any results exist? [Edge Case, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — All zero-state scenarios display an empty-state message (e.g., "No tournaments yet", "No matches recorded"). Standings table shows a message when no results exist.
- [x] CHK027 — Does the spec describe how tied standings positions (two teams both ranked 3rd) are presented in the UI — e.g., both show "3", the next rank is skipped, or a "T-3" notation is used? [Edge Case, Spec §FR-031]
  > **Cross-check:** RESOLVED — FR-031 updated to specify standard competition ranking (two teams tied at rank 3 → next team receives rank 5). Dense ranking and T-notation are ruled out.
- [x] CHK028 — Is there a UX requirement for what a public visitor sees when viewing a Suspended tournament — does it disappear from listings, show a "Suspended" badge, or show a placeholder message? [Edge Case, Spec §FR-012]
  > **Cross-check:** RESOLVED — FR-012 now requires a "Suspended" badge; tournament stays visible in public listings.

## Real-Time UX Requirements

- [x] CHK029 — Is there a requirement for the visual behavior when a SignalR standings update is received — e.g., row highlight, animation, or "last updated" timestamp — so users can recognize that data changed without a page reload? [Real-Time UX, Spec §FR-028, Gap]
  > **Cross-check:** RESOLVED — FR-028 now requires a brief visual indication (e.g., row highlight or transient notification) on update receipt.
- [x] CHK030 — Are UX requirements defined for the SignalR connection loss scenario — is the user notified of the disconnection, and is there a specified reconnection strategy or fallback (e.g., manual refresh prompt)? [Real-Time UX, Gap]
  > **Cross-check:** RESOLVED — FR-028 now requires a stale-data warning on connection loss and mandatory automatic reconnection by the SignalR client.
- [x] CHK031 — Is the 5-second SLA (SC-004) reflected in Angular-side UX requirements — e.g., a "connecting…" or "updating…" indicator shown during the interval between a result being recorded and the pushed update arriving? [Real-Time UX, Spec §SC-004, Gap]
  > **Cross-check:** RESOLVED — FR-028 now mandates in-place refresh and visual update indicator. A loading indicator during the push interval is not explicitly required (nor needed given the ≤5-second SLA), but the post-update visual signal is now specified.
- [x] CHK032 — Are stale-data handling requirements specified — if the SignalR hub fails to deliver an update within the 5-second SLA, must the UI show a stale-data warning or offer a manual refresh action? [Real-Time UX, Spec §SC-004, Gap]
  > **Cross-check:** RESOLVED — FR-028 now requires the frontend to inform the user that data may be outdated when the connection is interrupted, and to attempt automatic reconnection.

## Accessibility Requirements

- [x] CHK033 — Are keyboard navigation requirements specified beyond the constitution baseline for dynamic interactive elements: tournament status advancement button, match result edit controls, and team invitation action buttons? [Accessibility, Spec Plan §Principle III]
  > **Cross-check:** ACCEPTED (academic scope) — Constitution §Principle III baseline applies: all interactive elements must be keyboard-operable. No additional requirements beyond baseline for academic scope.
- [x] CHK034 — Is the "no color-only information" principle (Principle III) enforced by explicit spec requirements specifying non-chromatic cues (icons, text labels, or patterns) for tournament status badges and form validation error states? [Accessibility, Spec Plan §Principle III]
  > **Cross-check:** ACCEPTED (academic scope) — Status badges must include text labels (e.g., "Suspended", "Open") in addition to any color styling. Validation errors must include text messages alongside any red border.
- [x] CHK035 — Are ARIA label or live-region requirements specified for the standings table and match list regions, which receive content pushed via SignalR without a page reload? [Accessibility, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Standings table region must have `aria-live="polite"` to announce updates to screen readers when SignalR pushes new data.
- [x] CHK036 — Does the spec identify the primary language of the UI, and is the `lang` attribute requirement on `index.html` paired with a localization or language-selection requirement? [Accessibility, Spec Plan §Constitution]
  > **Cross-check:** RESOLVED — Primary UI language is Spanish (`es`). T002 now specifies `lang="es"` on frontend/src/index.html. No localization/language-selection required for academic scope.

## Non-Functional & SEO Requirements

- [x] CHK037 — Are responsive / mobile layout requirements defined for any page, or is the spec silent on supported viewport sizes, breakpoints, and touch interaction patterns? [Non-Functional, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Desktop-first single-viewport layout. No mobile breakpoint requirements for academic scope.
- [x] CHK038 — Are progressive enhancement requirements specified for public pages (tournament listing, tournament details) — specifically, what content must remain accessible to visitors with JavaScript disabled? [Non-Functional, Spec Plan §Principle II]
  > **Cross-check:** ACCEPTED (academic scope) — Tournament name, status, and videogame must be in the initial HTML payload. Angular renders these via server-side-compatible markup. Full progressive enhancement (SSR) is not required for academic scope.
- [x] CHK039 — Is the `meta description` requirement for the tournament listing page (SEO baseline) quantified with a content guideline or character limit, or left entirely to implementation discretion? [Non-Functional, Spec Plan §Constitution]
  > **Cross-check:** ACCEPTED (academic scope) — Meta description max 160 characters. Content must identify the platform purpose (e.g., "Plataforma de torneos de esports — encuentra y sigue torneos de tus videojuegos favoritos.").

## Ambiguities & Gaps

- [x] CHK040 — Is it specified what the invitation management UI shows to the Team Captain — pending invitations, accepted/declined status, expiry countdown — and is the invited Player's view of pending invitations equally defined? [Ambiguity, Spec §FR-017–FR-018]
  > **Cross-check:** PASS — GET /api/teams/{id}/invitations powers the Captain's dashboard (contracts/teams.md). GET /api/players/me/invitations powers the Player's inbox. Both return `status` and `expiresAt` fields.
- [x] CHK041 — Is the captaincy transfer UX flow defined — specifically, does the transfer require explicit acceptance from the receiving member before it takes effect, or is it immediate upon the Captain's action? [Ambiguity, Spec §FR-021]
  > **Cross-check:** RESOLVED — US-3 Scenario 6: immediate unilateral transfer. No acceptance step required. UX shows a confirmation dialog before submission.
- [x] CHK042 — Is there a requirement for how role-based feature restrictions are presented — are controls unavailable to the current role hidden entirely, disabled with an explanatory tooltip, or simply absent from navigation? [Ambiguity, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Role-restricted features are hidden from navigation (not shown, not disabled). Angular route guards return 403 if accessed directly.
- [x] CHK043 — Is it specified whether an unauthenticated visitor attempting to access the standings table is redirected to login, shown an inline authentication prompt, or presented with an access-denied message? [Ambiguity, Spec §FR-039 vs §FR-015]
  > **Cross-check:** ACCEPTED (academic scope) — Angular auth guard redirects to login page when standings endpoint returns 401.

## Videogame Selection & Form Dynamics

- [x] CHK044 — Is the UX for selecting a videogame at Player registration and team creation specified — is it a dropdown, typeahead, or static list, and is the behavior on a small fixed catalog documented? [Completeness, Spec §FR-001, §FR-016, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Standard `<select>` dropdown populated from GET /api/videogames. Five-item catalog does not require typeahead.
- [x] CHK045 — Is the UX for conditionally revealing Custom scoring system input fields (win/draw/loss point inputs) specified — when do they appear, and how does the form behave when the Organizer switches between scoring types? [Completeness, Spec §FR-029, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Win/draw/loss point inputs appear only when scoring type is "Custom". Switching to Standard or WinnerTakesAll hides and clears the custom fields.
- [x] CHK046 — Are requirements defined for the tournament filter/search UX on the public listing page — which filter controls exist (videogame, status), and how filtering interacts with the paginated result set? [Completeness, Spec §FR-015, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Dropdown filters for videogame and status, aligned with query params in contracts/tournaments.md. Filtering resets to page 1.

## Pagination & Destructive Action UX

- [x] CHK047 — Are pagination control requirements defined for the tournament listing page — are page navigation and page-size selector required UI elements, and do they have specific labeling or behavior requirements? [Completeness, Spec §FR-015, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Previous/Next buttons with current page number. Default page size 20 (contracts/tournaments.md). Page-size selector optional for academic scope.
- [x] CHK048 — Are UX requirements defined for destructive actions (withdraw tournament registration, remove team member, captaincy transfer) — specifically, are confirmation dialogs required before these actions take effect, and is their content specified? [Completeness, Spec §FR-020, §FR-021, §FR-024, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Confirmation dialog required for: withdraw registration, remove team member, and captaincy transfer. Dialog content: action description + Confirm/Cancel buttons.

## Account Update UX

- [x] CHK049 — Are UX requirements defined for the Organizer account update flow — which fields appear in the edit form, what success state is shown, and how is the uniqueness rejection of an organization name communicated? [Completeness, Spec §FR-037, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Edit form shows organizationName and password fields (contracts/users.md). On success, profile view reloads with updated data. On 409 Conflict, inline error on the organizationName field.
- [x] CHK050 — Are loading state and success/failure feedback requirements defined for the Player profile update flow (real name, email, password), including how an email uniqueness conflict from the server is surfaced to the user? [Completeness, Spec §FR-038, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Edit form shows realName, email, password fields. Spinner shown during submission. On 409 Conflict, inline error on the email field.

---

## Notes

- Check items off as completed: `[x]`
- Add inline comments with findings or decisions
- Items marked `[Gap]` indicate requirements not found in the spec that likely need to be added
- Items marked `[Ambiguity]` or `[Conflict]` indicate existing requirements needing clarification or alignment
- Link to relevant resources or decisions inline as items are reviewed
