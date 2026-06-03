# 05 — Frontend Webflow & Page Navigation Map
**Project:** SkillBridge v2 (Hunarmand)
**Framework:** ASP.NET Core 8 Razor Pages

---

## 1. Overview

This document defines every page in the application, its URL, who can access it, what it renders, what data it needs, and how it connects to other pages. Use this as the complete frontend contract.

---

## 2. Global Layout Files

| File | Purpose |
|------|---------|
| `Pages/Shared/_Layout.cshtml` | Public layout — navbar + footer, used by home & auth pages |
| `Pages/Shared/_CustomerLayout.cshtml` | Sidebar + topbar for logged-in Customer pages |
| `Pages/Shared/_LabourerLayout.cshtml` | Sidebar + topbar for logged-in Labourer pages |
| `Pages/Shared/_AdminLayout.cshtml` | Sidebar + topbar for Admin pages |
| `Pages/Shared/_ViewImports.cshtml` | Global `@using`, `@addTagHelper` |
| `Pages/Shared/_ViewStart.cshtml` | Sets default layout |
| `Pages/Shared/_Notifications.cshtml` | Partial: toast container + SignalR notification script |
| `Pages/Shared/_ValidationScripts.cshtml` | jQuery validation partial |

### CSS File Structure
```
wwwroot/css/
├── design-system.css      ← ALL CSS variables, reset, typography, components
├── layout.css             ← sidebar, topbar, grid structures
├── home.css               ← landing page specific
├── auth.css               ← login/register glass cards
├── dashboard.css          ← shared dashboard styles
├── map.css                ← leaflet overrides + custom map UI
├── chat.css               ← dispute chat panel
└── animations.css         ← all @keyframes
```

### JS File Structure
```
wwwroot/js/
├── app.js                 ← global init: theme toggle, toasts, sidebar toggle
├── map-browse.js          ← customer browse map + radius slider
├── map-register.js        ← labourer registration map pin
├── charts-admin.js        ← all Chart.js instances for admin dashboard
├── charts-labourer.js     ← Chart.js for labourer dashboard
├── dispute-chat.js        ← SignalR client for dispute chat
├── notifications.js       ← SignalR client for real-time notifications
└── counter-animation.js   ← animated number count-up for stat cards
```

---

## 3. Page Inventory

### 3.1 Public Pages (No Auth Required)

---

#### `GET /` — Home / Landing
**File:** `Pages/Index.cshtml`
**Layout:** `_Layout`
**Data needed:**
- `IEnumerable<LabourerSummary>` TopLabourers (top 4 by rating)
- `IEnumerable<SkillCategory>` Categories (all active)

**Sections rendered:**
1. Hero (animated aurora background + blobs)
2. How It Works (3 static steps)
3. Skill Categories grid (from DB)
4. Featured Labourers (4 cards from DB)
5. Footer

**Links out:**
- Hire a Professional → `/Customer/Browse`
- Register as Labourer → `/Auth/LabourerRegister`
- Category pill → `/Customer/Browse?category={slug}`
- Labourer card → `/Customer/LabourerProfile/{id}`
- Get Started (navbar) → `/Auth/CustomerLogin`

---

#### `GET /Auth/CustomerLogin` — Customer Login
**File:** `Pages/Auth/CustomerLogin.cshtml`
**Model:** `CustomerLoginModel`
**Layout:** `_Layout` (no sidebar)

**Form fields:** Email, Password
**POST `/Auth/CustomerLogin`:**
- Validate credentials via `IUserService.AuthenticateAsync()`
- Set session: `HttpContext.Session.SetInt32(SessionKeys.UserId, id)` + role
- Redirect → `/Customer/Dashboard`
- On failure: show inline error, re-render page

**Links out:** Register → `/Auth/CustomerRegister`, Labourer login → `/Auth/LabourerLogin`

---

#### `GET /Auth/LabourerLogin` — Labourer Login
Same pattern as CustomerLogin. Redirects → `/Labourer/Dashboard`.

---

#### `GET /Auth/AdminLogin` — Admin Login
Same pattern. No registration link. Redirects → `/Admin/Dashboard`.

---

#### `GET /Auth/CustomerRegister` — Customer Registration
**File:** `Pages/Auth/CustomerRegister.cshtml`
**Form fields:** FullName, Email, Phone, Password, ConfirmPassword
**POST:** Create user → redirect to `/Auth/CustomerLogin?registered=1`
**Validation:** Client-side + server-side. Email uniqueness checked via AJAX `/Auth/CheckEmail?email=`.

---

#### `GET /Auth/LabourerRegister` — Multi-Step Labourer Registration
**File:** `Pages/Auth/LabourerRegister.cshtml`

Five-step wizard — all steps in ONE Razor Page, step state managed via JavaScript (show/hide div sections). Data accumulated in hidden fields and submitted as one POST.

| Step | Fields |
|------|--------|
| 1 — Personal | FullName, Email, Phone, CNIC, Password, ConfirmPassword |
| 2 — Skills | CategoryId (dropdown), SubSkills (multi-checkbox) |
| 3 — Experience | YearsExperience, HourlyRate, Bio |
| 4 — Availability | 21 checkboxes (Mon–Sun × Morning/Afternoon/Evening) — serialised to JSON |
| 5 — Location | Leaflet map → hidden Latitude, Longitude, AddressText |

Progress bar updates on each JS step transition.
**POST `/Auth/LabourerRegister`:** Creates User + Labourer records, status = `PendingVerification` → redirects to `/Auth/LabourerLogin?pending=1`.

---

#### `GET /Auth/Logout`
Clears session, redirects to `/`.

---

#### `GET /Customer/LabourerProfile/{id}` — Public Labourer Profile
Accessible to logged-in customers. Shows full profile: bio, skills, rating breakdown, reviews list, availability, hourly rate. "Hire" button → `/Customer/HireRequest/{id}`.

---

### 3.2 Customer Pages (Role: Customer)

All protected by `[CustomerOnly]` page filter (redirect to `/Auth/CustomerLogin` if not logged in with Customer role).

---

#### `GET /Customer/Dashboard` — Customer Home
**File:** `Pages/Customer/Dashboard.cshtml`
**Data:**
- Welcome message + active request count
- List of 5 most recent requests (status, labourer name, date)
- Unrated completed jobs (prompt for review)

**Widgets:**
- 3 stat cards: Active, Completed, Pending Review
- Active requests mini-table
- "Quick Hire" category picker → Browse page

---

#### `GET /Customer/Browse` — Search & Map
**File:** `Pages/Customer/Browse.cshtml`
**Query params:** `?category=&minRating=&minExperience=&radius=&lat=&lng=&availability=`

**Data flow:**
1. On page load: JS requests `navigator.geolocation` → writes to hidden `CustomerLat`, `CustomerLng` fields.
2. Filters panel submits GET request with params.
3. Razor page calls `IMatchingService.GetFilteredLabourers(filterParams)` → returns `IEnumerable<LabourerSearchResult>`.
4. Results rendered in both list and map JSON (serialised to JS variable for Leaflet).

**Map JS (`map-browse.js`):**
```javascript
// Initialise map centred on customer's location
const map = L.map('browse-map').setView([customerLat, customerLng], 13);
L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png').addTo(map);

const radiusCircle = L.circle([customerLat, customerLng], {
  radius: currentRadiusMetres,
  color: '#7C3AED',
  fillOpacity: 0.08
}).addTo(map);

// Radius slider updates circle in real-time:
radiusSlider.addEventListener('input', () => {
  radiusCircle.setRadius(radiusSlider.value * 1000); // km to metres
  radiusDisplay.textContent = radiusSlider.value + ' km';
});

// Labour markers
labourers.forEach(l => {
  const marker = L.marker([l.lat, l.lng], { icon: purplePinIcon });
  marker.bindPopup(buildPopupHtml(l));
  marker.addTo(map);
});
```

---

#### `GET /Customer/HireRequest/{labourerId}` — Submit Hire Request
Form: Job Description (textarea), Preferred Date, Preferred Time, Location (map pin or text), Estimated Hours.
**POST:** Calls `IJobService.CreateJobRequest()` → status = Pending → redirect to `/Customer/Requests`.

---

#### `GET /Customer/Requests` — My Requests List
Table of all requests. Status badges. Click row → `/Customer/RequestDetail/{id}`.

---

#### `GET /Customer/RequestDetail/{id}` — Request Detail
Full job info + status timeline. If Completed and not yet rated → show rating form.
**POST `/Customer/SubmitReview/{jobId}`:** Star rating + review text → `IReviewService.SubmitReview()`.

---

#### `GET /Customer/Disputes` — My Disputes
List of disputes raised by this customer.
**POST `/Customer/RaiseDispute`:** JobId, Reason text → `IDisputeService.OpenDispute()` → redirects to dispute detail.

---

#### `GET /Customer/DisputeDetail/{id}` — Dispute Chat View (Customer side)
Read-only metadata + live chat window. Customer can send messages, sees admin replies.
Uses SignalR: joins group `dispute-{id}`.

---

#### `GET /Customer/Profile` — View/Edit Profile
Name, Email, Phone fields. Change password form.

---

### 3.3 Labourer Pages (Role: Labourer)

All protected by `[LabourerOnly]` page filter.

---

#### `GET /Labourer/Dashboard` — Labourer Home
**Data needed:**
- 4 stat cards: TotalJobsCompleted, AverageRating, MonthlyEarnings, ActiveJobs
- Last 6 months earnings array → Chart.js line chart
- Upcoming jobs list (next 5 by date)
- Recent reviews (last 3)
- Jobs funnel counts (Requested / Accepted / InProgress / Completed)
- Rating distribution (count per 1–5 stars)

All data from `IAnalyticsService.GetLabourerDashboard(labourerId)`.

**Charts rendered via `charts-labourer.js`:**
- Earnings line chart with gradient fill
- Jobs funnel horizontal bar chart
- Rating distribution doughnut chart

---

#### `GET /Labourer/Jobs` — Incoming & Current Jobs
Tab bar: Incoming (Pending) | Active | Completed | Rejected.
Each tab is filtered by status. Cards show customer name, job description, date, actions.

---

#### `GET /Labourer/JobDetail/{id}` — Job Detail
Full info card. Action button depends on current status:
- Pending → "Accept" (POST) / "Reject" (POST, opens reason modal)
- Accepted → "Start Job" (POST, → InProgress)
- InProgress → "Mark Completed" (POST)
- Completed/Rejected → read-only

Reject modal: textarea for reason (required), sends POST with `RejectReason`.

---

#### `GET /Labourer/Earnings` — Earnings Summary
Monthly breakdown table (Month, # Jobs, Total PKR estimate).
Line chart (same as dashboard but expanded with more data).

---

#### `GET /Labourer/Profile` — Edit Profile
Edit bio, hourly rate, sub-skills, availability grid, location pin update.

---

#### `GET /Labourer/Disputes` — My Disputes
List. Raise dispute against a customer → `/Labourer/RaiseDispute` (requires selecting a completed job).

---

#### `GET /Labourer/DisputeDetail/{id}`
Same SignalR chat view as customer side, from labourer's perspective.

---

### 3.4 Admin Pages (Role: Admin)

All protected by `[AdminOnly]` page filter.

---

#### `GET /Admin/Dashboard` — Admin Control Centre
**Data from `IAnalyticsService.GetAdminDashboard()`:**
- 6 stat cards (see GUI brief §5.7)
- UserGrowth: `List<MonthlyCount>` for customers + labourers (12 months)
- DailyTransactions: `List<DailyCount>` (last 30 days)
- JobStatusDistribution: counts per status enum value
- TopCategories: `List<CategoryDemand>` (name + request count)
- RecentActivity: `List<AuditLog>` (last 20 rows)

**Charts rendered via `charts-admin.js`:** 4 Chart.js instances as described in GUI brief.

---

#### `GET /Admin/LabourerVerification` — Verification Queue
Filterable table: Pending | Approved | Rejected tabs.
Row → expandable detail panel (all profile info).
**POST `/Admin/ApproveLabourer/{id}`:** Updates status to Verified, triggers notification to labourer.
**POST `/Admin/RejectLabourer/{id}`:** Requires reason text in modal → updates status + logs reason.

---

#### `GET /Admin/Users` — All Users
Searchable/sortable table of all customers and labourers.
Actions: View Profile, Suspend, Reinstate, Delete.

---

#### `GET /Admin/Disputes` — All Disputes
Table: Open | Resolved | Escalated tabs.
Click → `/Admin/DisputeDetail/{id}`.

---

#### `GET /Admin/DisputeDetail/{id}` — Live Dispute Chat + Resolution

**This is the most complex admin page.**

**Layout:**
```
┌──────────────────────────────────────────────────────────┐
│  DISPUTE #123 — Job #456 — [Customer Name] vs [Labourer] │
├──────────────────────────────────────────────────────────┤
│  Dispute Metadata: Reason, Date Opened, Raised By        │
├──────────────────────────────────────────────────────────┤
│                  CHAT PANEL                               │
│  ┌──────────────┬─────────────────┬────────────────┐    │
│  │ Customer     │   Chat Thread   │   Labourer     │    │
│  │ [Name]       │ [bubbles]       │ [Name]         │    │
│  │ [Avatar]     │ timestamped     │ [Avatar]       │    │
│  │ [Info]       │ labelled        │ [Info]         │    │
│  └──────────────┴─────────────────┴────────────────┘    │
│  [Admin message input ____________________] [Send]       │
├──────────────────────────────────────────────────────────┤
│  [Resolve → Customer Favour] [Resolve → Labourer Favour] │
│  [Escalate]                                               │
└──────────────────────────────────────────────────────────┘
```

**SignalR behaviour (`dispute-chat.js`):**
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/dispute")
  .withAutomaticReconnect()
  .build();

// Join the dispute group
connection.start().then(() => {
  connection.invoke("JoinDisputeRoom", disputeId);
});

// Receive messages from any party
connection.on("ReceiveMessage", (senderId, senderRole, message, timestamp) => {
  appendMessageBubble(senderId, senderRole, message, timestamp);
});

// Admin sends message
document.getElementById('sendBtn').addEventListener('click', () => {
  const msg = document.getElementById('adminMsgInput').value.trim();
  if (msg) connection.invoke("SendMessage", disputeId, msg);
});
```

**On resolution (POST `/Admin/ResolveDispute/{id}`):**
- Sets dispute status to Resolved
- Records resolution type (CustomerFavour / LabourerFavour)
- Triggers SignalR broadcast `DisputeResolved` → all clients hide chat, show read-only resolved banner
- Sends notification to both parties

---

#### `GET /Admin/Categories` — Skill Category Management
CRUD table for SkillCategories. Add/Edit/Delete (soft delete).

---

#### `GET /Admin/Analytics` — Extended Analytics (optional full page)
All charts from dashboard + download CSV buttons.

---

## 4. AJAX / Partial Endpoints

| Route | Method | Purpose |
|-------|--------|---------|
| `/Auth/CheckEmail` | GET | Returns `{available: bool}` for real-time email uniqueness check |
| `/Customer/Browse/Results` | GET | Returns partial `_LabourerResults.cshtml` for AJAX filter refresh |
| `/Notifications/GetUnread` | GET | Returns unread notification count for badge |
| `/Admin/QuickStats` | GET | Returns JSON for dashboard stat card refresh |

---

## 5. Navigation Access Matrix

| Page | Guest | Customer | Labourer | Admin |
|------|-------|----------|----------|-------|
| `/` | ✓ | ✓ | ✓ | ✓ |
| `/Auth/*` | ✓ | redirect | redirect | redirect |
| `/Customer/*` | ✗ | ✓ | ✗ | ✗ |
| `/Labourer/*` | ✗ | ✗ | ✓ | ✗ |
| `/Admin/*` | ✗ | ✗ | ✗ | ✓ |

Redirect rules:
- Unauthenticated → role login page
- Wrong role → own dashboard

---

## 6. Session Keys

```csharp
// Helpers/SessionKeys.cs
public static class SessionKeys
{
    public const string UserId       = "UserId";
    public const string UserRole     = "UserRole";     // "Customer" | "Labourer" | "Admin"
    public const string UserFullName = "UserFullName";
    public const string LabourerId   = "LabourerId";   // if role is Labourer
}
```

---

## 7. Page Authorization Filters

```csharp
// Helpers/Guards.cs — page filter attributes
public class CustomerOnlyAttribute : PageFilterAttribute { ... }
public class LabourerOnlyAttribute : PageFilterAttribute { ... }
public class AdminOnlyAttribute    : PageFilterAttribute { ... }
```

Each filter checks `HttpContext.Session.GetString(SessionKeys.UserRole)` and redirects if wrong.

---

## 8. Razor Page ViewData Conventions

| Key | Type | Used in |
|-----|------|---------|
| `ViewData["Title"]` | string | `<title>` tag in layout |
| `ViewData["ActiveNav"]` | string | Sidebar active state |
| `ViewData["BodyClass"]` | string | Extra CSS class on `<body>` |

---

## 9. Real-Time Features Summary

| Feature | Hub | Groups | Events |
|---------|-----|--------|--------|
| Dispute Chat | `DisputeChatHub` | `dispute-{id}` | `ReceiveMessage`, `DisputeResolved` |
| Notifications | `NotificationHub` | `user-{userId}` | `NewNotification`, `RequestStatusChanged` |

**Notification triggers** (send to user's group):
- Customer: request accepted/rejected/completed, dispute message
- Labourer: new hire request, dispute message, verification approved/rejected
- Admin: new dispute opened (badge count update)
