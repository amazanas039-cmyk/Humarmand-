# 06 — Implementation Plan
**Project:** Hunarmand v2 — On-Demand Labour Hiring Platform
**For:** Cursor AI — full project build from scratch
**Stack:** ASP.NET Core 8 Razor Pages · SQL Server · SignalR · Leaflet.js · Chart.js

> Read files 00–05 before this one. This plan gives you the exact build order, file-by-file.
> Never skip a phase — each phase unlocks the next. Do not use Entity Framework, Bootstrap, or Tailwind.

---

## Phase 0 — Solution Bootstrap (do this first, nothing else works without it)

### 0.1 Create the .NET solution
```bash
dotnet new sln -n Hunarmand
dotnet new webapp -n Hunarmand --no-https false
dotnet sln add Hunarmand/Hunarmand.csproj
cd Hunarmand
```

### 0.2 Add NuGet packages
```bash
dotnet add package Microsoft.Data.SqlClient
dotnet add package BCrypt.Net-Next
dotnet add package Microsoft.AspNetCore.SignalR
```
*(No EF Core, no Bootstrap, no Tailwind — the design system is hand-crafted CSS.)*

### 0.3 `appsettings.json`
```json
{
  "ConnectionStrings": {
    "HunarmandDB": "Server=(localdb)\\MSSQLLocalDB;Database=HunarmandDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Geocoding": {
    "NominatimBaseUrl": "https://nominatim.openstreetmap.org",
    "UserAgent": "Hunarmand/1.0 (student-project)"
  },
  "Session": { "IdleTimeoutMinutes": 120 }
}
```

### 0.4 `Program.cs` (complete scaffold — fill in as services are built)
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => {
    o.IdleTimeout = TimeSpan.FromMinutes(120);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("Nominatim", c => {
    c.BaseAddress = new Uri(builder.Configuration["Geocoding:NominatimBaseUrl"]!);
    c.DefaultRequestHeaders.UserAgent.ParseAdd(builder.Configuration["Geocoding:UserAgent"]);
});

// Repositories
builder.Services.AddScoped<DatabaseConnection>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<LabourerRepository>();
builder.Services.AddScoped<JobRepository>();
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<AdminRepository>();
builder.Services.AddScoped<DisputeRepository>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<AnalyticsRepository>();
builder.Services.AddScoped<ChatRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILabourerService, LabourerService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IMatchingService, SmartRankingService>();
builder.Services.AddScoped<IReputationService, ReputationService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IDisputeService, DisputeService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IGeocodingService, GeocodingService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapRazorPages();
app.MapHub<DisputeChatHub>("/hubs/dispute");
app.MapHub<NotificationHub>("/hubs/notify");

app.Run();
```

### 0.5 Folder structure — create these empty directories now
```
Hunarmand/
├─ Data/
├─ Models/
├─ Repositories/
├─ Services/
├─ Patterns/
├─ Hubs/
├─ Helpers/
├─ Pages/
│   ├─ Auth/
│   ├─ Customer/
│   ├─ Labourer/
│   ├─ Admin/
│   └─ Shared/
├─ wwwroot/
│   ├─ css/
│   │   ├─ design-system.css
│   │   ├─ layout.css
│   │   ├─ components.css
│   │   ├─ animations.css
│   │   └─ pages/
│   ├─ js/
│   │   ├─ theme.js
│   │   ├─ map.js
│   │   ├─ charts.js
│   │   ├─ notifications.js
│   │   └─ dispute-chat.js
│   └─ lib/   (vendor — leaflet, chart.js CDN preferred; local fallback here)
└─ Database/
    └─ schema.sql
```

---

## Phase 1 — Database (create and seed before writing any C# data code)

### 1.1 `Database/schema.sql`
Copy the **full SQL** from `03_DATABASE_SCHEMA.md` exactly.
Run order: tables → indexes → views → stored procedures → triggers → seed data.

**To run:**
```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -i Database/schema.sql
```

**Verify:**
```sql
USE HunarmandDB;
SELECT name FROM sys.tables ORDER BY name;     -- should list ~10 tables
SELECT name FROM sys.procedures ORDER BY name; -- should list ~25+ SPs
SELECT * FROM Users;                            -- should show 1 admin seed
```

### 1.2 Seed data requirements (in schema.sql)
```sql
-- Admin user (BCrypt hash of 'Admin@123' — pre-computed; accept SHA256 for seed only)
INSERT INTO Users (FullName,Email,PasswordHash,Role)
VALUES ('Platform Admin','admin@Hunarmand.pk',
        '$2a$11$xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx', -- BCrypt of Admin@123
        'Admin');
INSERT INTO Admins (UserID) SELECT UserID FROM Users WHERE Email='admin@Hunarmand.pk';

-- Skill categories
INSERT INTO SkillCategories (Name,Description) VALUES
('Electrician','Wiring, installations, repairs'),
('Plumber','Pipes, drains, fixtures'),
('Carpenter','Furniture, doors, cabinetry'),
('Painter','Interior/exterior painting'),
('Mason','Brickwork, plastering, tiles'),
('HVAC Technician','AC, heating, ventilation'),
('Welder','Metal fabrication and welding'),
('Solar Panel Technician','Solar installation and maintenance');
```

---

## Phase 2 — Data Layer (Models + Repositories)

### 2.1 Models (create all before writing any repository)

**`Models/User.cs`** — abstract base
```csharp
public abstract class User
{
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; protected set; } = "";
    public string Role { get; set; } = "";
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public bool IsSuspended { get; set; }
    public int DisputeStrikes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Polymorphic display helper
    public abstract string GetDashboardUrl();
}
```

**`Models/Customer.cs`**
```csharp
public class Customer : User
{
    public int CustomerID { get; set; }
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public override string GetDashboardUrl() => "/Customer/Dashboard";
}
```

**`Models/Labourer.cs`**
```csharp
public class Labourer : User
{
    public int LabourerID { get; set; }
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = "";
    public int ExperienceYears { get; set; }
    public decimal HourlyRate { get; set; }
    public string? Bio { get; set; }
    public string? AvailabilitySchedule { get; set; } // JSON
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal ReputationScore { get; private set; }
    public decimal CompletionRate { get; private set; }
    public string VerificationStatus { get; set; } = "Pending";
    public string? VerificationNote { get; set; }
    public bool IsProfileLive { get; set; }
    // Computed — set by ranking engine
    public double? DistanceKm { get; set; }
    public double SmartScore { get; set; }

    public void RecalculateReputation(decimal newScore) => ReputationScore = newScore; // controlled update
    public override string GetDashboardUrl() => "/Labourer/Dashboard";
}
```

**`Models/Admin.cs`**
```csharp
public class Admin : User
{
    public int AdminID { get; set; }
    public override string GetDashboardUrl() => "/Admin/Dashboard";
}
```

**`Models/JobRequest.cs`** — abstract state base
```csharp
public abstract class JobRequest
{
    public int RequestID { get; set; }
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = "";
    public int LabourerID { get; set; }
    public string LabourerName { get; set; } = "";
    public string JobDescription { get; set; } = "";
    public DateTime PreferredDate { get; set; }
    public TimeSpan PreferredTime { get; set; }
    public string Location { get; set; } = "";
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string Status { get; protected set; } = "";
    public string? RejectionReason { get; set; }
    public decimal EstimatedHours { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private readonly List<INotificationObserver> _observers = new();
    public void AddObserver(INotificationObserver o) => _observers.Add(o);

    public abstract void TransitionTo(string newStatus);
    protected void NotifyObservers(string message) => _observers.ForEach(o => o.Notify(this, message));
}
```

**`Models/JobRequests/`** — concrete state classes
- `PendingRequest.cs` — can transition to Accepted or Rejected
- `AcceptedRequest.cs` — can transition to InProgress
- `InProgressRequest.cs` — can transition to Completed
- `CompletedRequest.cs` — terminal state
- `RejectedRequest.cs` — terminal state

Each overrides `TransitionTo`, throwing `InvalidOperationException` for illegal transitions.

**`Models/Review.cs`, `Models/Dispute.cs`, `Models/Notification.cs`, `Models/DisputeChatMessage.cs`, `Models/AnalyticsData.cs`** — POCOs matching the schema columns exactly.

---

### 2.2 `Data/DatabaseConnection.cs`
```csharp
public class DatabaseConnection
{
    private readonly string _connectionString;
    public DatabaseConnection(IConfiguration config)
        => _connectionString = config.GetConnectionString("HunarmandDB")!;

    public SqlConnection GetConnection() => new SqlConnection(_connectionString);
}
```

### 2.3 Repositories — implement in this order

#### `Repositories/UserRepository.cs`
Methods (each calls its named SP with `CommandType.StoredProcedure`):
- `GetByEmail(string email)` → `sp_GetUserByEmail`
- `GetById(int id)` → `sp_GetUserById`
- `CreateCustomer(Customer c, string passwordHash)` → `sp_RegisterCustomer`
- `CreateLabourer(Labourer l, string passwordHash)` → `sp_RegisterLabourer`
- `UpdateProfile(int userId, ...)` → `sp_UpdateUserProfile`
- `SuspendUser(int userId, bool suspend)` → `sp_SuspendUser`
- `GetAllForAdmin(string? search)` → `sp_GetAllUsersForAdmin`

#### `Repositories/LabourerRepository.cs`
- `GetPendingVerification()` → `sp_GetPendingLabourers`
- `Approve(int labourerId, string note)` → `sp_ApproveLabourer`
- `Reject(int labourerId, string note)` → `sp_RejectLabourer`
- `SearchNearby(double lat, double lng, double radiusKm, int? categoryId, decimal? minRating, int? minExp)` → `sp_SearchLabourersNearby`
- `GetProfile(int labourerId)` → `sp_GetLabourerProfile`
- `UpdateAvailability(int labourerId, string scheduleJson)` → `sp_UpdateLabourerAvailability`

#### `Repositories/JobRepository.cs`
- `Create(JobRequest req)` → `sp_CreateJobRequest`
- `GetById(int requestId)` → `sp_GetJobRequestById`
- `UpdateStatus(int requestId, string status, string? reason)` → `sp_UpdateJobStatus`
- `GetForCustomer(int customerId)` → `sp_GetJobsForCustomer`
- `GetForLabourer(int labourerId)` → `sp_GetJobsForLabourer`
- `CheckAvailability(int labourerId, DateTime date, TimeSpan time)` → `sp_CheckLabourerAvailability`

#### `Repositories/ReviewRepository.cs`
- `Create(int requestId, int reviewerUserId, int targetLabourerId, int rating, string comment)` → `sp_CreateReview`
- `GetForLabourer(int labourerId)` → `sp_GetReviewsForLabourer`
- `HasReviewed(int requestId, int userId)` → `sp_HasReviewed`

#### `Repositories/DisputeRepository.cs`
- `Raise(int requestId, int raisedByUserId, string reason)` → `sp_RaiseDispute`
- `GetById(int disputeId)` → `sp_GetDisputeById`
- `GetOpen()` → `sp_GetOpenDisputes`
- `Resolve(int disputeId, string resolutionType, string resolutionText)` → `sp_ResolveDispute`
- `GetForUser(int userId)` → `sp_GetDisputesForUser`

#### `Repositories/ChatRepository.cs`
- `AddMessage(int disputeId, int senderUserId, string senderRole, string text)` → `sp_AddChatMessage`
- `GetMessages(int disputeId)` → `sp_GetChatMessages`
- `GetActiveSessions()` → `sp_GetActiveChatSessions`

#### `Repositories/NotificationRepository.cs`
- `Create(int userId, string message, string type)` → `sp_CreateNotification`
- `GetUnread(int userId)` → `sp_GetUnreadNotifications`
- `MarkRead(int notificationId)` → `sp_MarkNotificationRead`
- `MarkAllRead(int userId)` → `sp_MarkAllNotificationsRead`

#### `Repositories/AnalyticsRepository.cs`
- `GetPlatformKPIs()` → `sp_GetPlatformKPIs`
- `GetUserGrowth(int days)` → `sp_GetUserGrowthTrend`
- `GetDailyDeals(int days)` → `sp_GetDailyCompletedJobs`
- `GetCategoryDistribution()` → `sp_GetCategoryDistribution`
- `GetTopRatedLabourers(int n)` → `sp_GetTopRatedLabourers`
- `GetUnmetDemand()` → `sp_GetUnmetDemand`
- `GetLabourerEarnings(int labourerId, int days)` → `sp_GetLabourerEarningsTrend`
- `GetLabourerRatingTrend(int labourerId, int days)` → `sp_GetLabourerRatingTrend`
- `GetLabourerJobFunnel(int labourerId)` → `sp_GetLabourerJobFunnel`

---

## Phase 3 — Service Layer

### Interfaces first (`Services/Interfaces/`)

```csharp
// IUserService.cs
public interface IUserService {
    Task<User?> LoginAsync(string email, string password, string expectedRole);
    Task<bool> RegisterCustomerAsync(Customer c, string password);
    Task<bool> RegisterLabourerAsync(Labourer l, string password);
    Task<User?> GetByIdAsync(int userId);
}

// IMatchingService.cs
public interface IMatchingService {
    IEnumerable<Labourer> RankAndSort(IEnumerable<Labourer> labourers, double? customerLat, double? customerLng);
}

// IJobService.cs
public interface IJobService {
    Task<bool> SendHireRequestAsync(JobRequest request);
    Task<bool> UpdateStatusAsync(int requestId, string newStatus, string? reason);
    Task<JobRequest?> GetByIdAsync(int requestId);
    Task<IEnumerable<JobRequest>> GetForCustomerAsync(int customerId);
    Task<IEnumerable<JobRequest>> GetForLabourerAsync(int labourerId);
}

// IDisputeService.cs
public interface IDisputeService {
    Task<int> RaiseDisputeAsync(int requestId, int userId, string reason);
    Task<bool> ResolveAsync(int disputeId, string resType, string resText, int adminUserId);
    Task<IEnumerable<Dispute>> GetOpenAsync();
    Task<Dispute?> GetByIdAsync(int id);
}

// IGeocodingService.cs
public interface IGeocodingService {
    Task<(double lat, double lng)?> GeocodeAsync(string address);
}

// IAnalyticsService.cs
public interface IAnalyticsService {
    Task<PlatformKPIs> GetPlatformKPIsAsync();
    Task<IEnumerable<DataPoint>> GetUserGrowthAsync(int days);
    Task<IEnumerable<DataPoint>> GetDailyDealsAsync(int days);
    Task<IEnumerable<CategoryStat>> GetCategoryDistributionAsync();
    Task<IEnumerable<Labourer>> GetTopRatedAsync(int n);
    Task<LabourerDashboardData> GetLabourerDashboardAsync(int labourerId, int days);
}
```

### Key service implementations

#### `Services/UserService.cs`
```csharp
public async Task<User?> LoginAsync(string email, string password, string expectedRole)
{
    var userRow = await _userRepo.GetByEmail(email);
    if (userRow == null) return null;
    if (userRow.Role != expectedRole) return null;  // wrong-role rejection
    if (userRow.IsSuspended) return null;
    if (!BCrypt.Net.BCrypt.Verify(password, userRow.PasswordHash)) return null;
    return userRow;
}
```

#### `Services/SmartRankingService.cs` (Strategy pattern)
```csharp
public class SmartRankingService : IMatchingService
{
    private readonly List<IMatchingStrategy> _strategies = new() {
        new RatingBasedMatcher(weight: 0.40),
        new ExperienceBasedMatcher(weight: 0.25),
        new CompletionBasedMatcher(weight: 0.25),
        new LocationBasedMatcher(weight: 0.10),
    };

    public IEnumerable<Labourer> RankAndSort(IEnumerable<Labourer> labourers, double? lat, double? lng)
    {
        foreach (var l in labourers)
        {
            l.SmartScore = _strategies.Sum(s => s.Score(l, lat, lng));
        }
        return labourers.OrderByDescending(l => l.SmartScore);
    }
}
```

#### `Services/GeocodingService.cs`
```csharp
public async Task<(double lat, double lng)?> GeocodeAsync(string address)
{
    var encoded = Uri.EscapeDataString(address);
    var response = await _httpClient.GetAsync($"/search?format=json&limit=1&q={encoded}");
    if (!response.IsSuccessStatusCode) return null;
    var json = await response.Content.ReadAsStringAsync();
    // parse first result's lat/lon
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;
    if (root.GetArrayLength() == 0) return null;
    var first = root[0];
    return (double.Parse(first.GetProperty("lat").GetString()!),
            double.Parse(first.GetProperty("lon").GetString()!));
}
```

#### `Services/DisputeService.cs` — triggers SignalR after resolution
```csharp
public async Task<bool> ResolveAsync(int disputeId, string resType, string resText, int adminUserId)
{
    await _disputeRepo.Resolve(disputeId, resType, resText);
    // Notify both parties via NotificationService
    var dispute = await _disputeRepo.GetById(disputeId);
    await _notificationService.CreateAsync(dispute!.CustomerUserId,
        $"Dispute #{disputeId} resolved: {resType}", "DisputeResolved");
    await _notificationService.CreateAsync(dispute.LabourerUserId,
        $"Dispute #{disputeId} resolved: {resType}", "DisputeResolved");
    return true;
}
```

---

## Phase 4 — SignalR Hubs

### `Hubs/DisputeChatHub.cs`
```csharp
[Authorize] // session-based; implement custom auth middleware or check in hub methods
public class DisputeChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IDisputeService _disputeService;

    public async Task JoinDispute(int disputeId)
    {
        // Validate user belongs to this dispute
        await Groups.AddToGroupAsync(Context.ConnectionId, $"dispute-{disputeId}");
        // Send message history
        var msgs = await _chatService.GetMessagesAsync(disputeId);
        await Clients.Caller.SendAsync("LoadHistory", msgs);
    }

    public async Task SendMessage(int disputeId, string text)
    {
        var userId = GetUserId();
        var role = GetUserRole();
        var name = GetUserName();
        var msg = await _chatService.SaveMessageAsync(disputeId, userId, role, text);
        await Clients.Group($"dispute-{disputeId}")
            .SendAsync("ReceiveMessage", name, role, text, msg.SentAt);
    }

    public async Task ResolveDispute(int disputeId, string resolutionType, string resolutionText)
    {
        if (GetUserRole() != "Admin") return;
        await _disputeService.ResolveAsync(disputeId, resolutionType, resolutionText, GetUserId());
        await Clients.Group($"dispute-{disputeId}").SendAsync("DisputeResolved", disputeId);
    }

    private int GetUserId() => int.Parse(Context.GetHttpContext()!.Session.GetString("UserId")!);
    private string GetUserRole() => Context.GetHttpContext()!.Session.GetString("UserRole")!;
    private string GetUserName() => Context.GetHttpContext()!.Session.GetString("UserFullName")!;
}
```

### `Hubs/NotificationHub.cs`
```csharp
public class NotificationHub : Hub
{
    public async Task RegisterUser(int userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
    }
}
```

---

## Phase 5 — Design System (CSS) — build before writing any pages

Create `wwwroot/css/design-system.css` with ALL tokens from `04_GUI_DESIGN_BRIEF.md`.
Key sections:
1. `:root` — all CSS variables (colors, spacing, radius, shadows, transitions)
2. `[data-theme="dark"]` — dark mode overrides
3. Typography scale (`.text-xs` through `.text-4xl`)
4. Spacing utilities
5. Glass mixin (`.glass-card`, `.glass-nav`)
6. Button variants (`.btn-primary`, `.btn-ghost`, `.btn-outline`)
7. Form controls (`.form-group`, `.input-field`)
8. Status badge colors

Create `wwwroot/css/animations.css`:
- `@keyframes fadeInUp`, `@keyframes slideInRight`, `@keyframes pulse-glow`
- `.animate-fade-up`, `.animate-slide-in`, `.animate-counter`
- Hover micro-interactions for cards, buttons
- Orb/blob background animations for auth pages
- `@media (prefers-reduced-motion: reduce)` — disable all

Create `wwwroot/js/theme.js`:
```javascript
const stored = localStorage.getItem('theme');
const os = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
document.documentElement.setAttribute('data-theme', stored || os);

function toggleTheme() {
    const current = document.documentElement.getAttribute('data-theme');
    const next = current === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('theme', next);
}
```

---

## Phase 6 — Shared Layout & Partials

### `Pages/Shared/_Layout.cshtml`
- Loads `design-system.css`, `layout.css`, `components.css`, `animations.css`
- Loads CDN: Leaflet CSS+JS, Chart.js, SignalR client, Font Awesome 6
- Loads `theme.js` (inline in `<head>` before body — prevents FOUC)
- Renders: navbar (role-aware) → `@RenderBody()` → notification toast container → `@RenderSection("Scripts", false)`

### `Pages/Shared/_CustomerLayout.cshtml` (extends `_Layout.cshtml`)
- Sidebar with: Dashboard, Browse, My Requests, Disputes, Notifications

### `Pages/Shared/_LabourerLayout.cshtml`
- Sidebar: Dashboard, Requests, Profile, Earnings, Disputes, Notifications

### `Pages/Shared/_AdminLayout.cshtml`
- Sidebar: Dashboard, Verification Queue, Disputes, Categories, Accounts, Analytics

### Partials needed:
- `_Navbar.cshtml` — top bar; shows role, user name, notification bell (with unread count), light/dark toggle, logout
- `_Sidebar.cshtml` — role-specific nav (uses `ViewData["ActiveNav"]`)
- `_LabourerCard.cshtml` — search result card (avatar, name, category, rating stars, distance, rate, hire button)
- `_RequestCard.cshtml` — job request item with status badge
- `_DisputeChatBox.cshtml` — floating chat panel (shows when active dispute exists for user)
- `_NotificationDropdown.cshtml`
- `_StatusBadge.cshtml`

---

## Phase 7 — Auth Pages (build first; nothing else works without login)

### Page list:
```
Pages/Auth/
├─ Index.cshtml        — Role chooser landing
├─ CustomerLogin.cshtml
├─ LabourerLogin.cshtml
├─ AdminLogin.cshtml
├─ CustomerRegister.cshtml
└─ LabourerRegister.cshtml  (multi-step: 4 steps)
```

### `Pages/Auth/Index.cshtml` — Role chooser
Three glassmorphic cards: Customer / Labourer / Admin, each linking to their login.
Background: animated violet gradient orbs.
No layout — `Layout = null;` or minimal layout.

### Login pages — all share the same structure
```cshtml
@page
@model CustomerLoginModel
Layout = "_AuthLayout";

<div class="auth-bg">
  <div class="auth-orb auth-orb--1"></div>
  <div class="auth-orb auth-orb--2"></div>
  <div class="glass-card auth-card">
    <div class="auth-card__logo">...</div>
    <h1 class="auth-card__title">Customer Login</h1>
    <form method="post">
      <div class="form-group">
        <label asp-for="Input.Email">Email</label>
        <input asp-for="Input.Email" class="input-field" type="email" />
        <span asp-validation-for="Input.Email" class="form-error"></span>
      </div>
      <div class="form-group">
        <label asp-for="Input.Password">Password</label>
        <input asp-for="Input.Password" class="input-field" type="password" />
      </div>
      <button type="submit" class="btn-primary btn--full">Sign In</button>
      @Html.AntiForgeryToken()
    </form>
    <p class="auth-card__footer">Don't have an account? <a asp-page="CustomerRegister">Register</a></p>
  </div>
</div>
```

PageModel `OnPostAsync`:
1. Call `IUserService.LoginAsync(email, password, "Customer")`
2. If null → `ModelState.AddModelError`; if success → set session keys → redirect to `/Customer/Dashboard`

### `Pages/Auth/LabourerRegister.cshtml` — multi-step
Use hidden `<input name="Step" value="1-4">` and a single `OnPostAsync` that switches on step.
Steps: 1=Personal Info, 2=Skill & Rate, 3=Availability (day checkboxes + time pickers), 4=Location (Leaflet map picker).
Store partial data in `TempData` across steps.

---

## Phase 8 — Home Page (Index)

### `Pages/Index.cshtml`
Sections (in order):
1. **Hero** — headline with gradient word, subtext, CTA buttons (Get Started / Watch Demo), social proof avatars + count, floating dashboard mockup (a styled `<div>` mimicking the Aurora dashboard — not an image)
2. **How it Works** — 3 steps: Post Job → Get Matched → Work Done. Icon + title + description per step.
3. **Categories** — 8 category cards in a grid (icon + name + "X+ labourers")
4. **Why Hunarmand** — 4 feature cards (Verified Labourers, Smart Matching, Real-Time Tracking, Dispute Protection)
5. **Stats** — 3 animated counters (2,000+ Users, 500+ Verified Labourers, 4,800+ Jobs Completed)
6. **Testimonials** — 3 cards (customer reviews; use placeholder names)
7. **CTA Banner** — full-width violet gradient, "Ready to get started?" button
8. **Footer** — logo, tagline, links, social, copyright

Animations: hero text `fadeInUp`, counters animate on scroll (IntersectionObserver), cards lift on hover.

---

## Phase 9 — Customer Pages

Build in this order:

### `Pages/Customer/Dashboard.cshtml`
- Welcome card with name and quick stats (active jobs, completed, pending)
- Quick-action buttons: Browse Labourers, My Requests, Raise Dispute
- Recent activity list

### `Pages/Customer/Browse.cshtml` — MAP PROXIMITY FILTER
```html
<div class="browse-layout">
  <!-- Left: Filters panel -->
  <aside class="filters-panel glass-card">
    <h3>Find Labourers</h3>
    <!-- Location -->
    <button id="detectLocation" class="btn-outline btn--sm">📍 Use My Location</button>
    <input id="addressInput" placeholder="Or type an address..." class="input-field"/>
    <!-- Radius slider -->
    <label>Radius: <span id="radiusDisplay">10</span> km</label>
    <input type="range" id="radiusSlider" min="1" max="50" value="10" />
    <!-- Category -->
    <select id="categoryFilter" class="input-field">...</select>
    <!-- Min Rating -->
    <input type="range" id="ratingFilter" min="1" max="5" step="0.5" />
    <!-- Experience -->
    <input type="number" id="expFilter" min="0" max="30" />
  </aside>
  <!-- Center: Map -->
  <div id="map" class="browse-map"></div>
  <!-- Right: Results -->
  <div id="results" class="browse-results">
    <!-- Populated by JS calling /Customer/Browse/Results -->
  </div>
</div>
```

`wwwroot/js/map.js`:
```javascript
const map = L.map('map').setView([30.3753, 69.3451], 6); // Pakistan center
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);

let customerMarker = null;
let labourerMarkers = [];
let currentLat = null, currentLng = null;

document.getElementById('detectLocation').addEventListener('click', () => {
    navigator.geolocation.getCurrentPosition(pos => {
        currentLat = pos.coords.latitude;
        currentLng = pos.coords.longitude;
        if (customerMarker) customerMarker.setLatLng([currentLat, currentLng]);
        else customerMarker = L.marker([currentLat, currentLng], {icon: customerIcon}).addTo(map);
        map.setView([currentLat, currentLng], 13);
        fetchResults();
    });
});

let fetchTimer = null;
function debouncedFetch() {
    clearTimeout(fetchTimer);
    fetchTimer = setTimeout(fetchResults, 300);
}

async function fetchResults() {
    const radius = document.getElementById('radiusSlider').value;
    const category = document.getElementById('categoryFilter').value;
    const response = await fetch(`/Customer/Browse/Results?lat=${currentLat}&lng=${currentLng}&radiusKm=${radius}&categoryId=${category}`);
    const html = await response.text();
    document.getElementById('results').innerHTML = html;
    // Re-render map pins from returned data
    updateMapPins();
}

document.getElementById('radiusSlider').addEventListener('input', e => {
    document.getElementById('radiusDisplay').innerText = e.target.value;
    debouncedFetch();
});
```

### `Pages/Customer/HireRequest.cshtml`
Form: job description, preferred date/time, location (map pin or text). Submit calls `IJobService.SendHireRequestAsync`.

### `Pages/Customer/MyRequests.cshtml`
List of all requests with status badges. Click → detail page.

### `Pages/Customer/RequestDetail.cshtml`
Full request info + status timeline. If Completed and not reviewed → show rate/review form. If issue → "Raise Dispute" button.

### `Pages/Customer/RaiseDispute.cshtml`
Form: dispute reason. On submit calls `IDisputeService.RaiseDisputeAsync`. Redirects to dispute detail with chat box.

### `Pages/Customer/DisputeDetail.cshtml`
Shows dispute info + `_DisputeChatBox` partial (if session active). If resolved → read-only resolved banner.

---

## Phase 10 — Labourer Pages

### `Pages/Labourer/Dashboard.cshtml` — PROFESSIONAL DASHBOARD
Layout: 2-column responsive grid.
```
Row 1: KPI cards — Reputation Score | This Month Earnings | Completion Rate | Active Requests
Row 2: Earnings over time (line chart, last 30 days) | Rating trend (line chart)
Row 3: Jobs Funnel (horizontal bar: Total → Accepted → Completed) | Upcoming jobs list
Row 4: Recent reviews | Verification status banner
```

PageModel `OnGetAsync`:
- `IAnalyticsService.GetLabourerDashboardAsync(labourerId, 30)` → returns all data in one DTO
- Serialize time-series to JSON for Chart.js: `JsonSerializer.Serialize(earningsData)`
- Pass to page via `@Model.EarningsJson` etc.

Chart.js initialization in `@section Scripts`:
```javascript
const earningsData = @Html.Raw(Model.EarningsJson);
new Chart(document.getElementById('earningsChart'), {
    type: 'line',
    data: {
        labels: earningsData.map(d => d.date),
        datasets: [{ label: 'Estimated Earnings (PKR)', data: earningsData.map(d => d.value),
            borderColor: 'var(--color-primary)', fill: true,
            backgroundColor: 'var(--color-primary-alpha)' }]
    },
    options: { responsive: true, plugins: { legend: { display: false } } }
});
```

### `Pages/Labourer/Requests.cshtml`
Incoming requests (Pending) + active jobs (Accepted/InProgress).
Each card: customer name, job description, date/time, location.
Actions: Accept | Reject (with required reason modal) | Mark In Progress | Mark Complete.

### `Pages/Labourer/Profile.cshtml`
Edit profile: bio, hourly rate, availability schedule (visual weekly grid), location map pin.

### `Pages/Labourer/Earnings.cshtml`
Full earnings breakdown: completed jobs table with date, customer, hours, estimated earn. Total at bottom.

---

## Phase 11 — Admin Pages

### `Pages/Admin/Dashboard.cshtml` — PROFESSIONAL ANALYTICS DASHBOARD
```
Row 1: KPI cards (5) — Total Users | Active Labourers | Jobs Today | Completion Rate | Avg Rating
        Each card: icon, value, delta vs last week (↑/↓ colored), mini-sparkline
Row 2: User Growth chart (area, last 30 days) | Daily Deals chart (bar, last 30 days)
Row 3: Category Distribution (donut) | Top Rated Labourers list (top 5 with stars)
Row 4: Unmet Demand table | Quick actions (Pending Verifications badge, Open Disputes badge)
```

PageModel:
```csharp
public async Task OnGetAsync()
{
    KPIs = await _analytics.GetPlatformKPIsAsync();
    UserGrowthJson = JsonSerializer.Serialize(await _analytics.GetUserGrowthAsync(30));
    DailyDealsJson = JsonSerializer.Serialize(await _analytics.GetDailyDealsAsync(30));
    CategoryDistJson = JsonSerializer.Serialize(await _analytics.GetCategoryDistributionAsync());
    TopRated = await _analytics.GetTopRatedAsync(5);
    UnmetDemand = await _analytics.GetUnmetDemandAsync();
}
```

### `Pages/Admin/VerificationQueue.cshtml`
List of pending labourers. Each row: avatar, name, category, experience, location, date applied.
Actions: Approve | Reject (requires reason text input).

### `Pages/Admin/Disputes.cshtml` — LIVE DISPUTE MANAGEMENT
```
Left panel: List of open disputes (requestId, customer name, labourer name, raised date, reason snippet)
Right panel: Active chat for selected dispute — shows _DisputeChatBox inline
Admin can switch between disputes without leaving page (click dispute → loads chat via AJAX or JS)
Each dispute card has: Resolve button → modal with resolution type (CustomerFavour/LabourerFavour/Mutual) + resolution text
```

### `Pages/Admin/DisputeDetail.cshtml`
Full dispute view + inline chat box + resolve button.

### `Pages/Admin/Categories.cshtml`
CRUD table. Add: name + description. Edit inline. Disable (soft delete, not hard).

### `Pages/Admin/Accounts.cshtml`
Search bar → table of all users (paginated). Columns: name, role, email, status, joined date, dispute strikes.
Actions: View Profile | Suspend/Unsuspend | View Activity Log.

---

## Phase 12 — Dispute Chat Box Partial

`Pages/Shared/_DisputeChatBox.cshtml`:
```html
<div class="dispute-chat glass-card" id="disputeChat-@Model.DisputeId" data-dispute="@Model.DisputeId">
  <div class="dispute-chat__header">
    <span>⚠️ Dispute #@Model.DisputeId</span>
    <span class="dispute-chat__parties">@Model.CustomerName ↔ @Model.LabourerName</span>
    @if (User is Admin) {
      <button class="btn-primary btn--sm" onclick="openResolveModal(@Model.DisputeId)">Resolve</button>
    }
  </div>
  <div class="dispute-chat__messages" id="messages-@Model.DisputeId"></div>
  <div class="dispute-chat__input">
    <input type="text" id="msgInput-@Model.DisputeId" placeholder="Type a message..." class="input-field" />
    <button class="btn-primary btn--sm" onclick="sendMsg(@Model.DisputeId)">Send</button>
  </div>
</div>
```

`wwwroot/js/dispute-chat.js`:
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/dispute")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveMessage", (sender, role, text, sentAt) => {
    appendMessage(sender, role, text, sentAt);
});

connection.on("DisputeResolved", (disputeId) => {
    const box = document.getElementById(`disputeChat-${disputeId}`);
    if (box) {
        box.innerHTML = '<div class="resolved-banner">✅ Dispute Resolved</div>';
        setTimeout(() => box.remove(), 4000); // disappears after 4 seconds
    }
});

connection.start()
    .then(() => connection.invoke("JoinDispute", currentDisputeId))
    .catch(console.error);

function sendMsg(disputeId) {
    const input = document.getElementById(`msgInput-${disputeId}`);
    const text = input.value.trim();
    if (text) {
        connection.invoke("SendMessage", disputeId, text);
        input.value = '';
    }
}
```

---

## Phase 13 — Notifications

`wwwroot/js/notifications.js`:
```javascript
const notifConn = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notify").withAutomaticReconnect().build();

notifConn.on("NewNotification", (message, type) => {
    updateBellBadge(+1);
    showToast(message, type);
});

notifConn.start().then(() => notifConn.invoke("RegisterUser", currentUserId));

function updateBellBadge(delta) {
    const badge = document.querySelector('.notif-badge');
    if (badge) badge.textContent = Math.max(0, parseInt(badge.textContent || '0') + delta);
}
```

---

## Phase 14 — Final QA Checklist

Run through every item before considering the build complete:

### Functional
- [ ] Guest can browse home page; cannot access any `/Customer`, `/Labourer`, `/Admin` route
- [ ] Customer registers → logs in → searches with radius slider → sends hire request
- [ ] Labourer registers → appears in verification queue → admin approves → profile goes live
- [ ] Full job lifecycle: Pending → Accepted → InProgress → Completed works with correct status history
- [ ] Customer rates labourer after completion; reputation score updates
- [ ] Dispute raised → 3-way chat opens → admin resolves → chat disappears for all parties
- [ ] Admin dashboard shows all 5 KPI cards + all 4 charts with real data
- [ ] Labourer dashboard shows earnings, rating trend, job funnel charts
- [ ] Availability conflict checker blocks double-booking
- [ ] Wrong-role login is blocked with an error message
- [ ] Suspend user → that user cannot log in
- [ ] All 8 skill categories visible in Browse

### Design
- [ ] Light mode works; dark mode works; toggle persists across page loads
- [ ] All 3 login pages have glassmorphism cards + animated gradient background
- [ ] Home page hero has gradient headline + floating dashboard mockup
- [ ] Cards lift on hover on all pages
- [ ] KPI counters animate on page load (Admin dashboard, Labourer dashboard, Home)
- [ ] Sidebar active state highlights correctly per `ViewData["ActiveNav"]`
- [ ] Mobile: sidebar collapses to drawer; map and results stack vertically

### Technical
- [ ] No raw SQL outside Repositories
- [ ] No business logic in cshtml or PageModels beyond binding
- [ ] All SP names match exactly between Repositories and `schema.sql`
- [ ] Anti-forgery token on all POST forms
- [ ] `prefers-reduced-motion` disables animations
- [ ] Schema.sql runs clean from scratch (DROP IF EXISTS guard on all objects)

---

## Appendix A — File Creation Order (strict)

1. `Database/schema.sql` → run it
2. `Data/DatabaseConnection.cs`
3. All `Models/*.cs`
4. All `Repositories/*.cs`
5. All `Services/Interfaces/I*.cs`
6. All `Services/*.cs`
7. `Patterns/` — strategies, factory, state classes
8. `Hubs/DisputeChatHub.cs`, `Hubs/NotificationHub.cs`
9. `Helpers/SessionKeys.cs`, `Helpers/PasswordHasher.cs`, `Helpers/Guards.cs`
10. `wwwroot/css/design-system.css` + other CSS files
11. `wwwroot/js/theme.js`
12. `Pages/Shared/_Layout.cshtml` + partial layouts
13. `Pages/Shared/` — all partials
14. `Pages/Auth/` — all auth pages
15. `Pages/Index.cshtml` — home page
16. `Pages/Customer/` — all customer pages
17. `Pages/Labourer/` — all labourer pages
18. `Pages/Admin/` — all admin pages
19. `wwwroot/js/map.js`, `charts.js`, `notifications.js`, `dispute-chat.js`
20. `Program.cs` — wire everything
21. Run, test, fix
