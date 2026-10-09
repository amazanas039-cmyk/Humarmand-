# MASTER CURSOR PROMPT — paste this whole file into Cursor (Agent / Composer mode)

You are the lead engineer building **Hunarmand v2**, an on-demand labour-hiring web platform,
**from scratch**. Assume you know nothing about it yet — everything you need is in the `/docs`
folder of this workspace. Read those files before writing any code.

## Source-of-truth documents (read in this order, all in this workspace)
1. `docs/01_PRD.md` — what to build and why (product requirements + acceptance criteria)
2. `docs/02_TRD.md` — how to build it (stack, architecture, patterns, integrations, security)
3. `docs/03_DATABASE_SCHEMA.md` — the complete SQL Server schema (run this first when coding)
4. `docs/04_GUI_DESIGN_BRIEF.md` — the exact visual design system ("Aurora" violet theme)
5. `docs/05_USER_FLOWS_WEBFLOW.md` — every page, route, and navigation flow
6. `docs/06_IMPLEMENTATION_PLAN.md` — the phased build order you MUST follow

## Non-negotiable rules
1. **Follow the phases in `06_IMPLEMENTATION_PLAN.md` strictly.** Build Phase 1, stop, and summarize what you did and how to test it. Wait for me to say "continue" before the next phase. Never try to build the whole app in one response.
2. **Tech stack is fixed:** ASP.NET Core 8 Razor Pages (C#), ADO.NET calling **SQL Server stored procedures only** (no Entity Framework, no inline SQL outside repositories). Real-time via **SignalR**. Frontend = Razor + a custom CSS design system (no Bootstrap, no Tailwind build step), **Leaflet.js** for maps, **Chart.js** for charts.
3. **Architecture layering is mandatory:** `Pages → Services (interface + impl) → Repositories → DatabaseConnection → SQL stored procedures`. No raw SQL may exist outside `/Repositories`. No business logic in Razor pages.
4. **This is a graded OOP + SQL semester project.** You MUST implement the design patterns and SQL features exactly as listed in the TRD §"Academic Concept Map" and the Schema doc (abstract `User` class, inheritance, interfaces, polymorphism, encapsulation, Strategy/Factory/Observer/State/Repository patterns; stored procedures, triggers, views, transactions, indexes, 3NF normalization, cascading FKs, aggregate functions, joins). Do not skip or fake these.
5. **Apply the design system from `04_GUI_DESIGN_BRIEF.md` to every page** — use the CSS variables/tokens defined there, the glassmorphism rules, the light/dark toggle, and the animation guidance. Match the "Aurora" violet aesthetic. Do not output generic/un-styled pages.
6. **Preserve all original modules** AND add the 8 new requirements listed in the PRD. Do not drop existing features when adding new ones.
7. When a detail is genuinely unspecified, choose the option most consistent with the docs, state your assumption in your summary, and keep going. Do not stop to ask trivial questions.

## Definition of done for each page
- Server-side logic in the `.cshtml.cs` PageModel, talking only to a Service interface.
- All data access through a Repository that calls a stored procedure.
- Styled with the Aurora design tokens; responsive (mobile drawer nav); light/dark aware.
- No unhandled exceptions on the happy path; validation messages shown inline.

## Start now
Begin with **Phase 0 + Phase 1** from `06_IMPLEMENTATION_PLAN.md` (project scaffold, database
schema script, core layout, and the design-system CSS). Create the SQL schema file exactly as
specified in `03_DATABASE_SCHEMA.md`. Then build the global `_Layout`, the design-system CSS, and
the home page per `04_GUI_DESIGN_BRIEF.md` and `05_USER_FLOWS_WEBFLOW.md`.

When Phase 1 is complete, stop and give me:
- a list of files created,
- the exact commands to run it (`dotnet` + how to apply the SQL),
- what I should see in the browser,
then wait for me to say "continue" before Phase 2.
