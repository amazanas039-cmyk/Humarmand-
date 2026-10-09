# 01 — Product Requirements Document (PRD)
**Project:** SkillBridge v2 — On-Demand Labour Hiring Platform
**Tagline:** *Connecting Skill to Need.*

---

## 1. Vision & Problem

In Pakistan and the wider developing world, finding a reliable skilled labourer (electrician,
plumber, carpenter, painter, mason, etc.) is opaque, unreliable, and informal — based on word of
mouth with no verification, no standard rates, no accountability, and no records. SkillBridge
replaces this with a transparent, accountable, intelligent digital marketplace where:

- Every labourer is **admin-verified** before going live.
- A **reputation engine** scores labourers using recency-weighted ratings.
- A **smart ranking engine** orders search results by rating, experience, completion rate, and **proximity**.
- An **availability conflict checker** prevents double-booking.
- Every action is **logged** in a full audit trail.

---

## 2. Personas / Roles

| Role | Goal | Key actions |
|------|------|-------------|
| **Customer** | Find & hire a trustworthy nearby labourer | search/filter, view profiles, send hire requests, track status, rate & review, raise disputes |
| **Labourer** | Get consistent, fairly-paid work + grow reputation | multi-step register, manage incoming requests, track jobs, view earnings/reputation dashboard, raise disputes |
| **Admin** | Keep the marketplace trustworthy & healthy | verify labourers, manage skill categories, view analytics, resolve disputes (via live chat), manage/suspend accounts, see unmet-demand report |

A single abstract `User` underlies all three roles (see TRD).

---

## 3. Modules (ALL must exist)

### 3.1 Customer Module
- **Registration & Login** — secure account, hashed credentials.
- **Browse by Category** — electrician, plumber, carpenter, painter, mason, etc.
- **Smart Search & Filter** — by min rating, years of experience, location, availability.
- **🆕 Map Proximity Filter** — see labourers on a live map and filter by an **adjustable distance radius** from the customer's location (see §5, Feature 2).
- **Labourer Profile View** — skills, experience, hourly rate, availability schedule, reputation, reviews, distance away.
- **Send Hire Request** — job description, preferred date/time, location (with map pin).
- **Track Request Status** — Pending → Accepted → InProgress → Completed / Rejected (real-time).
- **Rate & Review** — star rating + written review after completion.
- **Hiring History** — all past/current requests.
- **Raise Dispute** — open a dispute against a labourer on a specific job (see Feature 4).

### 3.2 Labourer Module
- **Multi-Step Registration** — personal details → skill category → experience & hourly rate → weekly availability → location (map pin).
- **Verification Queue** — profile goes live only after admin approval.
- **Incoming Requests** — full customer & job detail.
- **Accept / Reject** — reject requires a logged reason.
- **Job Progress Tracking** — mark Accepted → InProgress → Completed.
- **🆕 Professional Dashboard** — growth, earnings over time, rating trend, completion rate, jobs funnel, reputation (see Feature 5).
- **Reputation Dashboard** — current score, all ratings, individual reviews.
- **Earnings Summary** — completed jobs count + estimated total earnings over time.
- **Raise Dispute** — open a dispute against a customer on a specific job.

### 3.3 Admin Module
- **Verification Queue** — approve/reject pending labourers with a written reason.
- **Skill Category Management** — add/edit/disable categories (e.g. add "Solar Panel Technician").
- **🆕 Professional Analytics Dashboard** — charts & graphs: user growth, daily transactions/deals done, completion rate, top categories, top-rated labourers, active users (see Feature 3).
- **🆕 Dispute Management with Live Chat** — when a dispute is raised, a temporary 3-way chat box opens (customer + labourer + admin). Admin mediates in real time; on settlement the box closes/disappears and the resolution is recorded (see Feature 4).
- **Account Management** — search any account, view activity, suspend/ban.
- **Unmet Demand Report** — categories searched often but with too few verified labourers.

### 3.4 Platform Intelligence Layer (carried over, do not drop)
- **Smart Ranking Engine** — weighted: Rating 40% + Experience 25% + Completion 25% + **Proximity 10%** (Strategy pattern).
- **Reputation Engine** — recency-decay weighted, not a flat average.
- **Availability Conflict Checker** — checks weekly schedule + existing bookings before confirming.
- **Demand Tracker** — logs every search by category; surfaces chronic supply gaps.
- **Auto-Escalation Triggers** — SQL triggers: flag accounts with 3+ unresolved disputes, prompt rating 24h after completion, escalate unattended requests.

---

## 4. Notifications (cross-cutting)
Every status change (request sent/accepted/rejected/completed, dispute opened/resolved, verification
approved/rejected) creates a notification for the relevant user, shown in a bell dropdown with an
unread count. Delivered in real time via SignalR where the user is online.

---

## 5. The 8 Requested Changes (feature specs + acceptance criteria)

### Feature 1 — Full GUI redesign ("Aurora" violet theme)
**What:** Replace the entire visual design with the violet/purple "Aurora" theme from the design brief.
**Acceptance:** Every page uses the design tokens in `04_GUI_DESIGN_BRIEF.md`; light & dark modes both work; no leftover old styling.

### Feature 2 — Map proximity filter 🗺️
**What:** On the Customer Browse page, show a live map (Leaflet + OpenStreetMap). The customer sets
their location (auto-detect via browser geolocation, or drop a pin / type an address), then adjusts
a **radius slider** (e.g. 1–50 km). The labourer list and map pins update to show only **verified,
live** labourers within that radius, sorted by distance combined with smart rank.
**Data:** Labourers and request locations carry latitude/longitude; distance computed with SQL Server
`geography::STDistance` (see Schema).
**Acceptance:**
- Moving the radius slider re-filters results live (debounced).
- Each result card shows "X.X km away".
- Map shows the customer marker + a labourer pin per result; clicking a pin highlights the card.
- Proximity feeds the 10% proximity weight of the smart ranking engine.

### Feature 3 — Professional Admin analytics dashboard 📊
**What:** A dashboard that visualizes platform growth and activity with charts.
**Required widgets:**
- KPI cards: Total Users, Active Labourers, Jobs Today, Platform Completion Rate, Avg Rating (with vs-last-week deltas, like the Aurora sample).
- **User growth** line/area chart (new users per day/week, last 30/90 days).
- **Daily transactions / deals done** chart (completed jobs per day, last 30 days).
- **Category distribution** bar/donut (labourers or jobs per category).
- **Top-rated labourers** list.
- **Unmet demand** mini-table.
**Acceptance:** All charts pull from stored procedures (Schema §Analytics); render with Chart.js; responsive; theme-aware colors.

### Feature 4 — Live dispute resolution chat 💬
**What:** When a customer or labourer raises a dispute on a job, the system creates a **temporary
chat session** linking the customer, the labourer, and the admin. The admin sees it in the Disputes
panel and can chat with **both parties at once** in real time. When the admin marks the dispute
**Resolved** (records a written resolution), the chat session **closes and the chat box disappears**
for all parties.
**Tech:** SignalR hub broadcasting to a per-dispute group; messages persisted (Schema §DisputeChat).
**Acceptance:**
- Raising a dispute creates an open chat session visible to all three participants.
- Messages appear in real time for whoever is online; persisted for whoever is offline.
- Admin "Resolve" closes the session: status → Resolved, resolution text saved, chat box hidden from all three.
- A resolved dispute keeps its message history in the DB (for audit) but is not shown as an active chat.

### Feature 5 — Professional Labourer dashboard 📈
**What:** Upgrade the labourer's home dashboard into an analytics view.
**Required widgets:** reputation score (with trend), earnings-over-time chart, rating-trend chart,
completion-rate gauge/number, jobs funnel (requests → accepted → completed), incoming-requests list,
verification/live status.
**Acceptance:** Charts pull from stored procedures; theme-aware; matches design brief.

### Feature 6 — Separate glassmorphic login per role 🔐
**What:** Three distinct, modern login experiences — **Customer**, **Labourer**, **Admin** — each
with a glass (frosted, blurred) card over an animated violet gradient background. A role chooser on
the auth landing routes to the correct login. Registration similarly split (Customer vs Labourer
multi-step; Admin is seeded, not self-registered).
**Acceptance:**
- Each role has its own login route & accent treatment (see Design Brief §Login).
- Glassmorphism: blurred translucent card, soft border, depth shadow, animated gradient/orb background.
- Wrong-role login is rejected with a clear message (e.g. admin cannot log in via customer page).

### Feature 7 — Modern effects throughout ✨
**What:** Tasteful animation & glass across the app: hover micro-interactions, card lift, smooth page
transitions, animated counters on KPI cards, gradient orbs, glass navbars/panels, light/dark toggle
with persistence. (See Design Brief §Motion — keep it tasteful and performant, respect
`prefers-reduced-motion`.)

### Feature 8 — Superb home page 🏠
**What:** A premium marketing landing page matching the Aurora sample: editorial hero ("Elevate your
…" style headline with a gradient word), social-proof avatars, primary/secondary CTAs, a floating
**product-preview dashboard** mockup, and a features grid ("Everything you need to succeed"). Plus
sections that explain SkillBridge: How it works (3 steps), categories, trust/verification, testimonials,
CTA, footer.
**Acceptance:** Matches the layout & feel of the sample; fully responsive; light/dark; the hero
preview can show a real-looking SkillBridge dashboard snapshot.

---

## 6. Out of scope (v2)
- Real payments / payment gateway (earnings are *estimated* = completed jobs × hourly rate × assumed hours; "deals/transactions" = completed jobs).
- Native mobile apps.
- SMS/email delivery infrastructure (email service is interface-stubbed/logged, per original design).

---

## 7. Success criteria
- All modules in §3 work end-to-end for all three roles.
- All 8 features in §5 meet their acceptance criteria.
- The OOP + SQL academic concepts in the TRD are demonstrably present.
- The app visually matches the Aurora design brief in both light and dark mode.
