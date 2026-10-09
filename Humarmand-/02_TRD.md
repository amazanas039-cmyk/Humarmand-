# 02 — Technical Requirements Document (TRD)
**Project:** Hunarmand v2

---

## 1. Technology stack (fixed)

| Layer | Technology |
|-------|-----------|
| Language / framework | C# / **ASP.NET Core 8 Razor Pages** |
| Data access | **ADO.NET** (`Microsoft.Data.SqlClient`) calling **stored procedures only** — NO Entity Framework |
| Database | **SQL Server** (LocalDB for dev: `(localdb)\MSSQLLocalDB`), with **spatial `geography` type** for proximity |
| Real-time | **ASP.NET Core SignalR** (dispute chat + live notifications) |
| Auth | Session-based, role-aware; passwords hashed (see §7) |
| Maps | **Leaflet.js** + OpenStreetMap tiles + **Nominatim** geocoding (free, no API key) |
| Charts | **Chart.js** (CDN) |
| Fonts/icons | Plus Jakarta Sans (headings) + Inter (body) + Font Awesome 6 |
| Styling | Custom CSS design system (CSS variables) — no Bootstrap/Tailwind build |

> **Why these:** they're free and key-less (important for a student project), they keep the
> ADO.NET + stored-procedure architecture the proposal requires, and SignalR/`geography` are the
> minimal correct tools for the live-chat and proximity features.

---

## 2. Solution architecture

Strict layering — **dependencies point downward only**:

```
┌───────────────────────────────────────────────┐
│  Pages (Razor .cshtml + .cshtml.cs PageModels)  │  presentation, no business logic
├───────────────────────────────────────────────┤
│  Services  (IXxxService + XxxService)           │  business logic, patterns, orchestration
├───────────────────────────────────────────────┤
│  Repositories (XxxRepository)                   │  ALL ADO.NET / SQL lives here
├───────────────────────────────────────────────┤
│  DatabaseConnection                             │  opens SqlConnection from config
├───────────────────────────────────────────────┤
│  SQL Server  (tables, SPs, views, triggers)     │
└───────────────────────────────────────────────┘
        ▲ SignalR Hubs sit beside Pages, call Services
```

**Hard rules**
- No raw SQL outside `/Repositories`. Repositories call stored procedures by name with parameters.
- No business logic in `.cshtml` or `.cshtml.cs` beyond binding & calling a service.
- Every service has an **interface** registered in DI (`Program.cs`), enabling polymorphism/testing.

### Folder structure
```
Hunarmand/
├─ Program.cs
├─ appsettings.json
├─ Data/                DatabaseConnection.cs
├─ Models/              POCOs + domain class hierarchy (abstract User, etc.)
├─ Repositories/        UserRepository, LabourerRepository, JobRepository,
│                       ReviewRepository, AdminRepository, DisputeRepository,
│                       NotificationRepository, AnalyticsRepository, ChatRepository
├─ Services/            IUserService/UserService, IMatchingService/SmartRankingService,
│                       IReputationService, IAvailabilityService, IJobService,
│                       IReviewService, IAdminService, IDisputeService,
│                       INotificationService, IEmailService, IGeocodingService, IAnalyticsService
├─ Patterns/            JobRequestFactory, JobRequest state classes, matching strategies, observers
├─ Hubs/                DisputeChatHub.cs, NotificationHub.cs   (SignalR)
├─ Helpers/             SessionKeys, PasswordHasher, Guards
├─ Pages/               Auth/, Customer/, Labourer/, Admin/, Shared/, Index
├─ wwwroot/             css/ (design system), js/, lib/ (leaflet, chartjs if vendored)
└─ Database/            schema.sql  (single canonical script — see file 03)
```

---

## 3. Academic Concept Map (MUST be implemented — graded)

> The old project had these; the rebuild must keep them. Implement as real engineering, not decoration.

### 3.1 C# OOP
| Concept | Concrete implementation |
|---------|--------------------------|
| **Abstract class** | `abstract class User` (cannot be instantiated). Holds `UserID`, `FullName`, `Email`, `Role`, `IsActive`, `IsSuspended`, `CreatedAt`. |
| **Inheritance** | `Customer : User`, `Labourer : User`, `Admin : User`. `JobRequest` is abstract with `PendingRequest`, `AcceptedRequest`, `InProgressRequest`, `CompletedRequest`, `RejectedRequest`. |
| **Interface** | `IMatchingStrategy`, `IReputationCalculator`, `INotificationService`, `IAvailabilityChecker`, `IReportGenerator`, `IGeocodingService`. |
| **Polymorphism** | `SmartRankingEngine` holds `IMatchingStrategy` and calls `Score()`/`Match()`; the concrete strategy is injected at runtime. |
| **Encapsulation** | Private fields; controlled properties. e.g. `Labourer.ReputationScore` has no public setter — only `RecalculateReputation()` updates it. |

### 3.2 Design patterns
| Pattern | Implementation |
|---------|----------------|
| **Strategy** | `IMatchingStrategy` → `RatingBasedMatcher`, `ExperienceBasedMatcher`, `CompletionBasedMatcher`, `LocationBasedMatcher`. `SmartRankingEngine` composes them with weights 40/25/25/10. |
| **Factory** | `JobRequestFactory.Create(status)` returns the correct `JobRequest` subtype from a status string. |
| **Observer** | `JobRequest` keeps a list of `INotificationService` observers; on status change it notifies all (alerts customer + labourer). |
| **State** | `JobRequest.TransitionTo(newStatus)` enforces legal transitions; illegal ones (e.g. Completed → Pending) throw `InvalidOperationException`. |
| **Repository** | Each repository wraps all ADO.NET calls; no SQL leaks elsewhere. |

### 3.3 SQL (see file 03 for the actual code)
Stored procedures; triggers (reputation recalc, status-history logging, notification creation,
3-dispute account flag, profile activation on approval); views (`vw_TopRatedLabourers`,
`vw_ActiveJobRequests`, `vw_UnmetDemand`, `vw_LabourerEarningsSummary`, `vw_PlatformHealthDashboard`);
foreign keys; indexes on hot columns; transactions for multi-row writes; 3NF normalization; cascading
FKs; aggregate functions (AVG/COUNT/SUM/MAX); INNER/LEFT/multi-table JOINs.

---

## 4. Feature: Map proximity filter (Feature 2)

### 4.1 Data
- Add `Latitude DECIMAL(9,6)`, `Longitude DECIMAL(9,6)` to **Labourers** and to **JobRequests**
  (request location) and optionally to **Customers** (home location).
- Add a persisted computed/`geography` column or compute on the fly with
  `geography::Point(@lat, @lng, 4326)`.

### 4.2 Flow
1. Customer location obtained via browser `navigator.geolocation` (fallback: address typed → geocoded by `IGeocodingService` → Nominatim → lat/lng).
2. Browse page posts `{lat, lng, radiusKm, filters}` to a handler.
3. `sp_SearchLabourersNearby` filters verified+live labourers where
   `geography::Point(@lat,@lng,4326).STDistance(LabourerGeo) <= @radiusKm * 1000`, returns each with `DistanceKm`.
4. `SmartRankingService` blends `DistanceKm` into the proximity component (closer = higher score) and returns sorted results.
5. Frontend renders Leaflet map: customer marker + one pin per result; cards show distance; radius slider re-queries (debounced ~300ms).

### 4.3 `IGeocodingService`
- `Task<(double lat, double lng)?> GeocodeAsync(string address)` → calls Nominatim
  (`https://nominatim.openstreetmap.org/search?format=json&q=...`), set a proper User-Agent header,
  cache results. Used at registration (labourer pins their location) and for typed customer addresses.

---

## 5. Feature: Live dispute chat (Feature 4) — SignalR

### 5.1 Tables (see file 03)
`Disputes`, `DisputeChatSessions`, `DisputeChatMessages`.

### 5.2 Hub — `Hubs/DisputeChatHub.cs`
- Group name = `dispute-{disputeId}`. On connect, a participant (customer/labourer/admin, verified by
  session + a check they belong to that dispute) joins the group.
- `SendMessage(disputeId, text)` → persist via `ChatRepository.AddMessage` → broadcast
  `ReceiveMessage(senderName, senderRole, text, sentAt)` to the group.
- `ResolveDispute(disputeId, resolutionText)` (admin only) → `DisputeService.Resolve(...)` sets
  status `Resolved`, saves resolution, then broadcasts `DisputeResolved(disputeId)` → all clients hide
  the chat box.

### 5.3 Lifecycle
- Raising a dispute (`sp_RaiseDispute`) creates the `Disputes` row + an **open** `DisputeChatSessions` row.
- "Open dispute chats" appear: for the customer & labourer on their dispute/job screens; for the admin in the Disputes panel (admin can have multiple open at once).
- On resolve: session `IsActive = 0`, `Disputes.Status='Resolved'`, `Resolution` saved; UI removes the box. History stays in DB for audit.
- Auto-escalation trigger: an account with 3+ unresolved disputes is flagged for admin attention.

### 5.4 Also use SignalR for notifications
`NotificationHub` pushes new notifications to a user's group `user-{userId}` so the bell updates live.

---

## 6. Feature: Analytics (Features 3 & 5)
- `IAnalyticsService` + `AnalyticsRepository` call analytics stored procedures returning time-series
  and aggregates (see Schema §Analytics): user growth, daily completed jobs (deals), category
  distribution, completion rate, rating trend, earnings over time.
- PageModels expose data as JSON (via a `JsonResult` handler or serialized into a `data-*`/script
  block) for Chart.js to consume. Theme-aware colors come from CSS variables read in JS.

---

## 7. Security & auth
- **Roles:** Customer, Labourer, Admin stored on `Users.Role`.
- **Sessions:** `SessionKeys.UserId`, `.Role`, `.FullName`. 2-hour idle timeout, HttpOnly cookie.
- **Password hashing:** use a salted hash. **Recommended:** `BCrypt.Net-Next` (add NuGet). If you
  must mirror the legacy seed, SHA-256 is acceptable for the admin seed only; prefer BCrypt for all
  new accounts and note it. Never store plaintext.
- **Authorization guard:** a helper/filter that redirects to the correct login if the session role
  doesn't match the page area (`/Admin/*` requires Admin, etc.). Wrong-role login is rejected.
- **Anti-forgery:** all POST forms include the ASP.NET anti-forgery token; AJAX/SignalR posts send it via header.
- **Input validation:** server-side on every PageModel; parameterized SP calls everywhere (no string concatenation into SQL).

---

## 8. Configuration
`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "HunarmandDB": "Server=(localdb)\\MSSQLLocalDB;Database=HunarmandDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Geocoding": { "NominatimBaseUrl": "https://nominatim.openstreetmap.org", "UserAgent": "Hunarmand/1.0 (student-project)" }
}
```
`Program.cs` registers: Razor Pages, Session, HttpContextAccessor, `AddSignalR()`, `IHttpClientFactory`
(for geocoding), and all repositories + services + hubs. Map hubs: `app.MapHub<DisputeChatHub>("/hubs/dispute")`,
`app.MapHub<NotificationHub>("/hubs/notify")`.

---

## 9. Non-functional
- **Responsive** down to 360px; mobile drawer nav.
- **Light/Dark** via CSS variables + a toggle persisted in `localStorage`, defaulting to OS preference.
- **Accessibility:** semantic HTML, focus states, `prefers-reduced-motion` honored, AA contrast.
- **Performance:** indexed hot columns; debounced map queries; charts lazy-init on view.
- **Error handling:** friendly `/Error` page; no stack traces to users in production.
