# 07 — Backend Service & API Contract
**Project:** SkillBridge v2
**Purpose:** Complete contract for all services, stored procedures, and AJAX endpoints.
Cursor uses this as the single source of truth when wiring repositories → services → page models.

---

## 1. Stored Procedure Master List

> Every repository method maps 1-to-1 to a stored procedure in `schema.sql`.
> Parameters listed as `@Name TYPE` exactly as they appear in the SP signature.

### 1.1 User / Auth SPs

| SP Name | Parameters | Returns | Called By |
|---------|-----------|---------|-----------|
| `sp_RegisterCustomer` | `@FullName NVARCHAR(150), @Email NVARCHAR(150), @PasswordHash NVARCHAR(255), @Phone NVARCHAR(20), @Address NVARCHAR(250), @Latitude DECIMAL(9,6), @Longitude DECIMAL(9,6)` | `UserID INT` (new) | `UserRepository.CreateCustomer` |
| `sp_RegisterLabourer` | `@FullName, @Email, @PasswordHash, @Phone, @CategoryID INT, @ExperienceYears INT, @HourlyRate DECIMAL(10,2), @Bio NVARCHAR(500), @AvailabilitySchedule NVARCHAR(MAX), @Address NVARCHAR(250), @Latitude DECIMAL(9,6), @Longitude DECIMAL(9,6)` | `UserID INT` (new) | `UserRepository.CreateLabourer` |
| `sp_GetUserByEmail` | `@Email NVARCHAR(150)` | Full `Users` row + role-specific extension row | `UserRepository.GetByEmail` |
| `sp_GetUserById` | `@UserID INT` | Same as above | `UserRepository.GetById` |
| `sp_UpdateUserProfile` | `@UserID INT, @FullName NVARCHAR(150), @Phone NVARCHAR(20), @Address NVARCHAR(250), @Latitude DECIMAL(9,6), @Longitude DECIMAL(9,6)` | `1` (rows affected) | `UserRepository.UpdateProfile` |
| `sp_SuspendUser` | `@UserID INT, @IsSuspended BIT` | — | `AdminRepository.SuspendUser` |
| `sp_GetAllUsersForAdmin` | `@Search NVARCHAR(150) = NULL` | Joined Users + role details, ordered by CreatedAt DESC | `AdminRepository.GetAllUsers` |

**sp_RegisterLabourer implementation notes:**
```sql
CREATE PROCEDURE sp_RegisterLabourer ... AS
BEGIN
    BEGIN TRANSACTION;
    INSERT INTO Users (FullName,Email,PasswordHash,Role,Phone) VALUES (@FullName,@Email,@PasswordHash,'Labourer',@Phone);
    DECLARE @uid INT = SCOPE_IDENTITY();
    INSERT INTO Labourers (UserID,CategoryID,ExperienceYears,HourlyRate,Bio,AvailabilitySchedule,Address,Latitude,Longitude)
    VALUES (@uid,@CategoryID,@ExperienceYears,@HourlyRate,@Bio,@AvailabilitySchedule,@Address,@Latitude,@Longitude);
    COMMIT;
    SELECT @uid AS UserID;
END
```

---

### 1.2 Labourer SPs

| SP Name | Parameters | Returns |
|---------|-----------|---------|
| `sp_GetPendingLabourers` | — | All Labourers WHERE VerificationStatus='Pending' JOIN Users |
| `sp_ApproveLabourer` | `@LabourerID INT, @Note NVARCHAR(500)` | — (trigger activates profile) |
| `sp_RejectLabourer` | `@LabourerID INT, @Note NVARCHAR(500)` | — |
| `sp_GetLabourerProfile` | `@LabourerID INT` | Full labourer detail + user info + category name + avg rating |
| `sp_UpdateLabourerProfile` | `@LabourerID INT, @Bio NVARCHAR(500), @HourlyRate DECIMAL(10,2), @Address NVARCHAR(250), @Latitude DECIMAL(9,6), @Longitude DECIMAL(9,6)` | — |
| `sp_UpdateLabourerAvailability` | `@LabourerID INT, @AvailabilitySchedule NVARCHAR(MAX)` | — |
| `sp_SearchLabourersNearby` | `@CustomerLat DECIMAL(9,6), @CustomerLng DECIMAL(9,6), @RadiusKm DECIMAL(5,2), @CategoryID INT = NULL, @MinRating DECIMAL(3,1) = 0, @MinExperience INT = 0` | Labourers within radius with DistanceKm, sorted by ReputationScore DESC |

**sp_SearchLabourersNearby key logic:**
```sql
CREATE PROCEDURE sp_SearchLabourersNearby
    @CustomerLat DECIMAL(9,6), @CustomerLng DECIMAL(9,6), @RadiusKm DECIMAL(5,2),
    @CategoryID INT = NULL, @MinRating DECIMAL(3,1) = 0, @MinExperience INT = 0
AS
BEGIN
    DECLARE @CustomerGeo GEOGRAPHY = geography::Point(@CustomerLat, @CustomerLng, 4326);

    SELECT
        l.LabourerID, u.FullName, u.Phone, sc.Name AS CategoryName,
        l.ExperienceYears, l.HourlyRate, l.ReputationScore, l.CompletionRate,
        l.Latitude, l.Longitude, l.Bio, l.IsProfileLive,
        ROUND(@CustomerGeo.STDistance(geography::Point(l.Latitude, l.Longitude, 4326)) / 1000.0, 2) AS DistanceKm
    FROM Labourers l
        INNER JOIN Users u ON l.UserID = u.UserID
        INNER JOIN SkillCategories sc ON l.CategoryID = sc.CategoryID
    WHERE l.IsProfileLive = 1
        AND u.IsActive = 1 AND u.IsSuspended = 0
        AND l.Latitude IS NOT NULL AND l.Longitude IS NOT NULL
        AND @CustomerGeo.STDistance(geography::Point(l.Latitude, l.Longitude, 4326)) <= @RadiusKm * 1000
        AND (@CategoryID IS NULL OR l.CategoryID = @CategoryID)
        AND l.ReputationScore >= @MinRating
        AND l.ExperienceYears >= @MinExperience
    ORDER BY DistanceKm ASC;
END
```

---

### 1.3 Job Request SPs

| SP Name | Parameters | Returns |
|---------|-----------|---------|
| `sp_CreateJobRequest` | `@CustomerID INT, @LabourerID INT, @JobDescription NVARCHAR(500), @PreferredDate DATE, @PreferredTime TIME, @Location NVARCHAR(250), @Latitude DECIMAL(9,6), @Longitude DECIMAL(9,6), @EstimatedHours DECIMAL(5,2)` | `RequestID INT` |
| `sp_GetJobRequestById` | `@RequestID INT` | Full request + customer name + labourer name |
| `sp_UpdateJobStatus` | `@RequestID INT, @NewStatus NVARCHAR(20), @RejectionReason NVARCHAR(500) = NULL` | — (trigger logs history) |
| `sp_GetJobsForCustomer` | `@CustomerID INT` | All requests ordered by CreatedAt DESC |
| `sp_GetJobsForLabourer` | `@LabourerID INT` | All requests ordered by UpdatedAt DESC |
| `sp_CheckLabourerAvailability` | `@LabourerID INT, @Date DATE, @Time TIME` | `IsAvailable BIT` |

**sp_CheckLabourerAvailability logic:**
```sql
-- Check 1: existing Pending/Accepted/InProgress request on same date
-- Check 2: day of week not in weekly AvailabilitySchedule JSON
-- Returns 1 if available, 0 if conflict
SELECT CASE WHEN COUNT(*) > 0 THEN 0 ELSE 1 END AS IsAvailable
FROM JobRequests
WHERE LabourerID = @LabourerID
  AND PreferredDate = @Date
  AND Status IN ('Pending','Accepted','InProgress');
```

---

### 1.4 Review SPs

| SP Name | Parameters | Returns |
|---------|-----------|---------|
| `sp_CreateReview` | `@RequestID INT, @ReviewerUserID INT, @TargetLabourerID INT, @Rating INT, @Comment NVARCHAR(500)` | — (trigger recalculates ReputationScore) |
| `sp_GetReviewsForLabourer` | `@LabourerID INT` | Reviews with reviewer name, date, rating, comment |
| `sp_HasReviewed` | `@RequestID INT, @UserID INT` | `HasReviewed BIT` |

**Reputation recalc trigger (fires after INSERT on Reviews):**
```sql
CREATE TRIGGER tr_RecalcReputation ON Reviews AFTER INSERT AS
BEGIN
    DECLARE @lid INT = (SELECT TargetLabourerID FROM inserted);
    DECLARE @newScore DECIMAL(5,2);

    -- Recency-weighted: more recent reviews have higher weight (1/age_months + 1)
    SELECT @newScore = SUM(r.Rating * (1.0 / (DATEDIFF(MONTH, r.CreatedAt, SYSUTCDATETIME()) + 1)))
                     / SUM(1.0 / (DATEDIFF(MONTH, r.CreatedAt, SYSUTCDATETIME()) + 1))
    FROM Reviews r WHERE r.TargetLabourerID = @lid;

    UPDATE Labourers SET ReputationScore = @newScore WHERE LabourerID = @lid;
END
```

---

### 1.5 Dispute SPs

| SP Name | Parameters | Returns |
|---------|-----------|---------|
| `sp_RaiseDispute` | `@RequestID INT, @RaisedByUserID INT, @Reason NVARCHAR(500)` | `DisputeID INT` |
| `sp_GetDisputeById` | `@DisputeID INT` | Dispute + customer/labourer user IDs and names + request summary |
| `sp_GetOpenDisputes` | — | All disputes WHERE Status='Open' with party names |
| `sp_GetDisputesForUser` | `@UserID INT` | Disputes where user is customer or labourer |
| `sp_ResolveDispute` | `@DisputeID INT, @ResolutionType NVARCHAR(30), @ResolutionText NVARCHAR(500)` | — |

**sp_RaiseDispute — also creates chat session:**
```sql
CREATE PROCEDURE sp_RaiseDispute @RequestID INT, @RaisedByUserID INT, @Reason NVARCHAR(500) AS
BEGIN
    BEGIN TRANSACTION;
    INSERT INTO Disputes (RequestID, RaisedByUserID, Reason, Status)
    VALUES (@RequestID, @RaisedByUserID, @Reason, 'Open');
    DECLARE @did INT = SCOPE_IDENTITY();
    INSERT INTO DisputeChatSessions (DisputeID, IsActive)
    VALUES (@did, 1);
    COMMIT;
    SELECT @did AS DisputeID;
END
```

---

### 1.6 Chat SPs

| SP Name | Parameters | Returns |
|---------|-----------|---------|
| `sp_AddChatMessage` | `@DisputeID INT, @SenderUserID INT, @SenderRole NVARCHAR(20), @MessageText NVARCHAR(MAX)` | `MessageID INT, SentAt DATETIME2` |
| `sp_GetChatMessages` | `@DisputeID INT` | All messages ordered by SentAt ASC with sender names |
| `sp_GetActiveChatSessions` | — | Sessions WHERE IsActive=1 with dispute + party info |

---

### 1.7 Notification SPs

| SP Name | Parameters | Returns |
|---------|-----------|---------|
| `sp_CreateNotification` | `@UserID INT, @Message NVARCHAR(300), @NotificationType NVARCHAR(50)` | `NotificationID INT` |
| `sp_GetUnreadNotifications` | `@UserID INT` | Notifications WHERE IsRead=0 ordered by CreatedAt DESC |
| `sp_MarkNotificationRead` | `@NotificationID INT` | — |
| `sp_MarkAllNotificationsRead` | `@UserID INT` | — |
| `sp_GetNotificationCount` | `@UserID INT` | `UnreadCount INT` |

---

### 1.8 Analytics SPs

#### `sp_GetPlatformKPIs` — returns single row
```sql
SELECT
    (SELECT COUNT(*) FROM Users WHERE IsActive=1) AS TotalUsers,
    (SELECT COUNT(*) FROM Labourers WHERE IsProfileLive=1) AS ActiveLabourers,
    (SELECT COUNT(*) FROM JobRequests WHERE Status='Completed' AND CAST(UpdatedAt AS DATE)=CAST(SYSUTCDATETIME() AS DATE)) AS JobsToday,
    (SELECT COUNT(*) FROM Users WHERE CreatedAt >= DATEADD(DAY,-7,SYSUTCDATETIME())) AS NewUsersThisWeek,
    (SELECT AVG(CAST(Rating AS FLOAT)) FROM Reviews) AS AvgRating,
    (SELECT CAST(COUNT(CASE WHEN Status='Completed' THEN 1 END) AS FLOAT) / NULLIF(COUNT(*),0) * 100 FROM JobRequests) AS CompletionRate,
    -- vs last week deltas
    (SELECT COUNT(*) FROM Users WHERE CreatedAt >= DATEADD(DAY,-14,SYSUTCDATETIME()) AND CreatedAt < DATEADD(DAY,-7,SYSUTCDATETIME())) AS NewUsersLastWeek,
    (SELECT COUNT(*) FROM JobRequests WHERE Status='Completed' AND UpdatedAt >= DATEADD(DAY,-7,SYSUTCDATETIME())) AS JobsThisWeek,
    (SELECT COUNT(*) FROM JobRequests WHERE Status='Completed' AND UpdatedAt >= DATEADD(DAY,-14,SYSUTCDATETIME()) AND UpdatedAt < DATEADD(DAY,-7,SYSUTCDATETIME())) AS JobsLastWeek
```

#### `sp_GetUserGrowthTrend @Days INT`
```sql
SELECT CAST(CreatedAt AS DATE) AS [Date], COUNT(*) AS NewUsers
FROM Users
WHERE CreatedAt >= DATEADD(DAY, -@Days, SYSUTCDATETIME())
GROUP BY CAST(CreatedAt AS DATE)
ORDER BY [Date] ASC
```

#### `sp_GetDailyCompletedJobs @Days INT`
```sql
SELECT CAST(UpdatedAt AS DATE) AS [Date], COUNT(*) AS CompletedJobs
FROM JobRequests
WHERE Status = 'Completed' AND UpdatedAt >= DATEADD(DAY, -@Days, SYSUTCDATETIME())
GROUP BY CAST(UpdatedAt AS DATE)
ORDER BY [Date] ASC
```

#### `sp_GetCategoryDistribution`
```sql
SELECT sc.Name, COUNT(l.LabourerID) AS LabourerCount
FROM SkillCategories sc
LEFT JOIN Labourers l ON l.CategoryID = sc.CategoryID AND l.IsProfileLive = 1
GROUP BY sc.CategoryID, sc.Name
ORDER BY LabourerCount DESC
```

#### `sp_GetTopRatedLabourers @TopN INT = 5`
```sql
SELECT TOP (@TopN) l.LabourerID, u.FullName, sc.Name AS CategoryName,
    l.ReputationScore, l.CompletionRate,
    (SELECT COUNT(*) FROM JobRequests jr WHERE jr.LabourerID = l.LabourerID AND jr.Status = 'Completed') AS CompletedJobs
FROM Labourers l
INNER JOIN Users u ON l.UserID = u.UserID
INNER JOIN SkillCategories sc ON l.CategoryID = sc.CategoryID
WHERE l.IsProfileLive = 1
ORDER BY l.ReputationScore DESC
```

#### `sp_GetUnmetDemand`
```sql
SELECT TOP 5 sc.Name AS Category,
    COUNT(DISTINCT ds.CategoryID) AS SearchCount,
    COUNT(DISTINCT l.LabourerID) AS LabourerCount,
    COUNT(DISTINCT ds.CategoryID) - COUNT(DISTINCT l.LabourerID) AS Gap
FROM DemandSearchLog ds
LEFT JOIN SkillCategories sc ON sc.CategoryID = ds.CategoryID
LEFT JOIN Labourers l ON l.CategoryID = ds.CategoryID AND l.IsProfileLive = 1
GROUP BY sc.CategoryID, sc.Name
HAVING COUNT(DISTINCT l.LabourerID) < 5
ORDER BY Gap DESC
```

#### `sp_GetLabourerEarningsTrend @LabourerID INT, @Days INT`
```sql
SELECT CAST(jr.UpdatedAt AS DATE) AS [Date],
    SUM(jr.EstimatedHours * l.HourlyRate) AS EstimatedEarnings
FROM JobRequests jr
INNER JOIN Labourers l ON l.LabourerID = jr.LabourerID
WHERE jr.LabourerID = @LabourerID
  AND jr.Status = 'Completed'
  AND jr.UpdatedAt >= DATEADD(DAY, -@Days, SYSUTCDATETIME())
GROUP BY CAST(jr.UpdatedAt AS DATE)
ORDER BY [Date] ASC
```

#### `sp_GetLabourerRatingTrend @LabourerID INT, @Days INT`
```sql
SELECT CAST(r.CreatedAt AS DATE) AS [Date], AVG(CAST(r.Rating AS FLOAT)) AS AvgRating
FROM Reviews r
WHERE r.TargetLabourerID = @LabourerID
  AND r.CreatedAt >= DATEADD(DAY, -@Days, SYSUTCDATETIME())
GROUP BY CAST(r.CreatedAt AS DATE)
ORDER BY [Date] ASC
```

#### `sp_GetLabourerJobFunnel @LabourerID INT`
```sql
SELECT
    COUNT(*) AS TotalRequests,
    COUNT(CASE WHEN Status IN ('Accepted','InProgress','Completed') THEN 1 END) AS Accepted,
    COUNT(CASE WHEN Status = 'Completed' THEN 1 END) AS Completed,
    COUNT(CASE WHEN Status = 'Rejected' THEN 1 END) AS Rejected
FROM JobRequests WHERE LabourerID = @LabourerID
```

---

## 2. C# DTOs (Data Transfer Objects)

### `Models/DTOs/PlatformKPIs.cs`
```csharp
public class PlatformKPIs
{
    public int TotalUsers { get; set; }
    public int ActiveLabourers { get; set; }
    public int JobsToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public double AvgRating { get; set; }
    public double CompletionRate { get; set; }
    // Delta helpers (calculated in service layer)
    public double UserGrowthDelta { get; set; }    // % change week-over-week
    public double JobsDelta { get; set; }
}
```

### `Models/DTOs/DataPoint.cs`
```csharp
public class DataPoint
{
    public string Date { get; set; } = "";  // "2025-06-01" format
    public double Value { get; set; }
}
```

### `Models/DTOs/CategoryStat.cs`
```csharp
public class CategoryStat
{
    public string Name { get; set; } = "";
    public int Count { get; set; }
    public string Color { get; set; } = ""; // set by service; cycles through palette
}
```

### `Models/DTOs/LabourerDashboardData.cs`
```csharp
public class LabourerDashboardData
{
    public decimal ReputationScore { get; set; }
    public double ReputationDelta { get; set; }      // vs last period
    public decimal EstimatedEarningsTotal { get; set; }
    public decimal EarningsThisMonth { get; set; }
    public double CompletionRate { get; set; }
    public int ActiveRequests { get; set; }
    public List<DataPoint> EarningsTrend { get; set; } = new();
    public List<DataPoint> RatingTrend { get; set; } = new();
    public JobFunnel Funnel { get; set; } = new();
    public List<JobRequest> RecentRequests { get; set; } = new();
}

public class JobFunnel
{
    public int TotalRequests { get; set; }
    public int Accepted { get; set; }
    public int Completed { get; set; }
    public int Rejected { get; set; }
}
```

### `Models/DTOs/DisputeViewModel.cs`
```csharp
public class DisputeViewModel
{
    public int DisputeID { get; set; }
    public int RequestID { get; set; }
    public string CustomerName { get; set; } = "";
    public int CustomerUserID { get; set; }
    public string LabourerName { get; set; } = "";
    public int LabourerUserID { get; set; }
    public string Reason { get; set; } = "";
    public string Status { get; set; } = "";
    public string? ResolutionText { get; set; }
    public DateTime RaisedAt { get; set; }
    public List<ChatMessageViewModel> Messages { get; set; } = new();
    public bool HasActiveSession { get; set; }
}

public class ChatMessageViewModel
{
    public string SenderName { get; set; } = "";
    public string SenderRole { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime SentAt { get; set; }
}
```

---

## 3. AJAX Endpoint Reference

All AJAX handlers are `OnGetXxx` methods in their page's `PageModel` returning `IActionResult`.

### `GET /Customer/Browse?handler=Results&lat=&lng=&radiusKm=&categoryId=&minRating=&minExp=`
**PageModel:** `BrowseModel.OnGetResultsAsync`
```csharp
public async Task<IActionResult> OnGetResultsAsync(double lat, double lng, double radiusKm,
    int? categoryId, decimal minRating = 0, int minExp = 0)
{
    var labourers = await _labourerRepo.SearchNearby(lat, lng, radiusKm, categoryId, minRating, minExp);
    var ranked = _matchingService.RankAndSort(labourers, lat, lng);
    return Partial("_LabourerResults", ranked);
}
```
Returns: HTML partial `_LabourerResults.cshtml` with labourer cards + JSON array of pins for JS to use.

### `GET /Auth/CheckEmail?handler=CheckEmail&email=`
```csharp
public async Task<IActionResult> OnGetCheckEmailAsync(string email)
{
    var exists = await _userRepo.EmailExistsAsync(email);
    return new JsonResult(new { available = !exists });
}
```

### `GET /Notifications/GetUnread` (or as handler on shared layout)
```csharp
public async Task<IActionResult> OnGetUnreadAsync()
{
    var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
    var count = await _notificationRepo.GetUnreadCountAsync(userId);
    return new JsonResult(new { count });
}
```

### `GET /Admin/Dashboard?handler=QuickStats`
```csharp
public async Task<IActionResult> OnGetQuickStatsAsync()
{
    var kpis = await _analytics.GetPlatformKPIsAsync();
    return new JsonResult(kpis);
}
```

---

## 4. Pattern Implementations

### `Patterns/Strategies/IMatchingStrategy.cs`
```csharp
public interface IMatchingStrategy
{
    string Name { get; }
    double Weight { get; }
    double Score(Labourer labourer, double? customerLat, double? customerLng);
}
```

### `Patterns/Strategies/RatingBasedMatcher.cs`
```csharp
public class RatingBasedMatcher : IMatchingStrategy
{
    public string Name => "Rating";
    public double Weight { get; } = 0.40;
    // Normalise: ReputationScore is 1-5, normalise to 0-1
    public double Score(Labourer l, double? lat, double? lng)
        => Weight * ((double)l.ReputationScore / 5.0);
}
```

### `Patterns/Strategies/LocationBasedMatcher.cs`
```csharp
public class LocationBasedMatcher : IMatchingStrategy
{
    public string Name => "Location";
    public double Weight { get; } = 0.10;
    // Closer = higher score; max meaningful distance = 50km → 0; 0km → Weight
    public double Score(Labourer l, double? lat, double? lng)
    {
        if (l.DistanceKm == null) return 0;
        double normalized = Math.Max(0, 1.0 - (l.DistanceKm.Value / 50.0));
        return Weight * normalized;
    }
}
```

### `Patterns/JobRequestFactory.cs`
```csharp
public static class JobRequestFactory
{
    public static JobRequest Create(string status)
        => status switch {
            "Pending"    => new PendingRequest(),
            "Accepted"   => new AcceptedRequest(),
            "InProgress" => new InProgressRequest(),
            "Completed"  => new CompletedRequest(),
            "Rejected"   => new RejectedRequest(),
            _ => throw new ArgumentException($"Unknown status: {status}")
        };

    // Hydrate from DB row
    public static JobRequest FromDataRow(SqlDataReader reader)
    {
        var status = reader.GetString(reader.GetOrdinal("Status"));
        var req = Create(status);
        req.RequestID = reader.GetInt32(reader.GetOrdinal("RequestID"));
        req.CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID"));
        req.LabourerID = reader.GetInt32(reader.GetOrdinal("LabourerID"));
        req.JobDescription = reader.GetString(reader.GetOrdinal("JobDescription"));
        // ... map all other fields
        return req;
    }
}
```

### `Patterns/States/PendingRequest.cs`
```csharp
public class PendingRequest : JobRequest
{
    public PendingRequest() { Status = "Pending"; }

    public override void TransitionTo(string newStatus)
    {
        if (newStatus != "Accepted" && newStatus != "Rejected")
            throw new InvalidOperationException($"Cannot transition from Pending to {newStatus}");
        Status = newStatus;
        NotifyObservers($"Your request is now {newStatus}");
    }
}
```

### `Patterns/Observers/INotificationObserver.cs`
```csharp
public interface INotificationObserver
{
    Task Notify(JobRequest request, string message);
}
```

### `Patterns/Observers/NotificationServiceObserver.cs`
```csharp
public class NotificationServiceObserver : INotificationObserver
{
    private readonly INotificationService _notifications;
    private readonly IHubContext<NotificationHub> _hub;

    public async Task Notify(JobRequest request, string message)
    {
        // Save to DB
        await _notifications.CreateAsync(request.CustomerUserId, message, "StatusChange");
        // Push real-time
        await _hub.Clients.Group($"user-{request.CustomerUserId}")
            .SendAsync("NewNotification", message, "StatusChange");
    }
}
```

---

## 5. Trigger Reference (SQL)

### Auto-Escalation Triggers

**`tr_FlagDisputeAccount`** — fires on INSERT to Disputes
```sql
CREATE TRIGGER tr_FlagDisputeAccount ON Disputes AFTER INSERT AS
BEGIN
    DECLARE @uid INT = (SELECT RaisedByUserID FROM inserted);
    UPDATE Users SET DisputeStrikes = DisputeStrikes + 1 WHERE UserID = @uid;
    -- If 3+ unresolved disputes, mark for admin review (IsSuspended-light)
    IF (SELECT DisputeStrikes FROM Users WHERE UserID = @uid) >= 3
        UPDATE Users SET IsActive = 0 WHERE UserID = @uid; -- soft flag
END
```

**`tr_LogStatusHistory`** — fires on UPDATE to JobRequests
```sql
CREATE TRIGGER tr_LogStatusHistory ON JobRequests AFTER UPDATE AS
BEGIN
    IF UPDATE(Status)
        INSERT INTO RequestStatusHistory (RequestID, Status, ChangedAt)
        SELECT RequestID, Status, SYSUTCDATETIME() FROM inserted;
END
```

**`tr_ActivateLabourerOnApproval`** — fires on UPDATE to Labourers
```sql
CREATE TRIGGER tr_ActivateLabourerOnApproval ON Labourers AFTER UPDATE AS
BEGIN
    IF UPDATE(VerificationStatus)
        UPDATE Labourers SET IsProfileLive = 1
        WHERE LabourerID IN (SELECT LabourerID FROM inserted WHERE VerificationStatus = 'Approved');
END
```

**`tr_CreateRatingPromptNotification`** — fires on UPDATE to JobRequests when Status → 'Completed'
```sql
CREATE TRIGGER tr_CreateRatingPromptNotification ON JobRequests AFTER UPDATE AS
BEGIN
    IF UPDATE(Status)
    BEGIN
        DECLARE @rid INT, @cid INT;
        SELECT @rid = RequestID, @cid = CustomerID FROM inserted WHERE Status = 'Completed';
        IF @rid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Reviews WHERE RequestID = @rid)
            EXEC sp_CreateNotification @UserID = (SELECT u.UserID FROM Customers c JOIN Users u ON u.UserID=c.UserID WHERE c.CustomerID=@cid),
                @Message = N'Please rate your labourer for your recent job.',
                @NotificationType = 'RatingPrompt';
    END
END
```

**`tr_LogDemandSearch`** — fires on INSERT to DemandSearchLog (called from repository when a browse search is made)
```sql
-- DemandSearchLog is populated by sp_LogDemandSearch called from LabourerRepository.SearchNearby
-- No trigger needed; demand tracking is explicit via SP
```

---

## 6. Session Key Reference (complete)

```csharp
// Helpers/SessionKeys.cs
public static class SessionKeys
{
    public const string UserId       = "UserId";       // int
    public const string UserRole     = "UserRole";     // "Customer"|"Labourer"|"Admin"
    public const string UserFullName = "UserFullName"; // string
    public const string LabourerId   = "LabourerId";   // int — set if Role == "Labourer"
    public const string CustomerId   = "CustomerId";   // int — set if Role == "Customer"
    public const string AdminId      = "AdminId";      // int — set if Role == "Admin"
}
```

Set on login:
```csharp
HttpContext.Session.SetString(SessionKeys.UserId, user.UserID.ToString());
HttpContext.Session.SetString(SessionKeys.UserRole, user.Role);
HttpContext.Session.SetString(SessionKeys.UserFullName, user.FullName);
if (user is Labourer l) HttpContext.Session.SetString(SessionKeys.LabourerId, l.LabourerID.ToString());
if (user is Customer c) HttpContext.Session.SetString(SessionKeys.CustomerId, c.CustomerID.ToString());
```

---

## 7. Authorization Guards

### `Helpers/Guards.cs`
```csharp
public class RequireRoleAttribute : ActionFilterAttribute
{
    private readonly string _role;
    public RequireRoleAttribute(string role) => _role = role;

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var role = context.HttpContext.Session.GetString(SessionKeys.UserRole);
        if (role != _role)
        {
            var redirect = _role switch {
                "Customer" => "/Auth/CustomerLogin",
                "Labourer" => "/Auth/LabourerLogin",
                "Admin"    => "/Auth/AdminLogin",
                _ => "/"
            };
            context.Result = new RedirectResult(redirect);
        }
    }
}

// Usage on PageModel:
// [RequireRole("Customer")]
// public class DashboardModel : PageModel { ... }
```

---

## 8. Email Service (stubbed)

```csharp
// Services/IEmailService.cs
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}

// Services/LoggingEmailService.cs — stub; logs to console in dev
public class LoggingEmailService : IEmailService
{
    private readonly ILogger<LoggingEmailService> _logger;
    public async Task SendAsync(string to, string subject, string body)
    {
        _logger.LogInformation("EMAIL to {To} | Subject: {Subject}\n{Body}", to, subject, body);
        await Task.CompletedTask;
    }
}
```

Register as: `builder.Services.AddScoped<IEmailService, LoggingEmailService>();`

---

## 9. Error Handling

### `Pages/Error.cshtml`
Custom error page: shows user-friendly message, no stack trace.
Links back to the relevant dashboard.

### Global exception middleware in `Program.cs` (already set above):
```csharp
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");
```

### Repository try-catch pattern:
```csharp
public async Task<Labourer?> GetProfileAsync(int labourerId)
{
    try {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_GetLabourerProfile", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@LabourerID", labourerId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync()) return MapLabourer(reader);
        return null;
    } catch (SqlException ex) {
        _logger.LogError(ex, "sp_GetLabourerProfile failed for LabourerID {Id}", labourerId);
        throw; // re-throw so service layer can handle
    }
}
```
