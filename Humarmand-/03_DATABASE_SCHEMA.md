# 03 — Database Schema (SQL Server)
**Database:** `HunarmandDB` · single canonical script · ADO.NET calls **these stored procedures only**.

> This file consolidates the messy multi-file SQL of the old project into ONE clean, ordered script.
> Cursor: create `Database/schema.sql` from this. Run order is top-to-bottom (tables → indexes →
> views → procedures → triggers → seed). Names here are the contract the repositories call.

---

## 0. Conventions
- PK = `IDENTITY(1,1)` int named `<Entity>ID`.
- Money = `DECIMAL(10,2)`; coordinates = `DECIMAL(9,6)`; ratings = `INT` 1–5; scores = `DECIMAL(5,2)`.
- Timestamps default `SYSUTCDATETIME()`.
- All cross-table relationships use FKs; cascade where a child cannot exist without its parent.
- 3NF: no repeating groups, no partial/transitive dependencies.

---

## 1. Tables

### Users (abstract base in C#)
```sql
CREATE TABLE Users (
    UserID        INT IDENTITY(1,1) PRIMARY KEY,
    FullName      NVARCHAR(150) NOT NULL,
    Email         NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash  NVARCHAR(255) NOT NULL,
    Role          NVARCHAR(20)  NOT NULL CHECK (Role IN ('Customer','Labourer','Admin')),
    Phone         NVARCHAR(20)  NULL,
    IsActive      BIT NOT NULL DEFAULT 1,
    IsSuspended   BIT NOT NULL DEFAULT 0,
    DisputeStrikes INT NOT NULL DEFAULT 0,   -- incremented by trigger
    CreatedAt     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

### Customers
```sql
CREATE TABLE Customers (
    CustomerID  INT IDENTITY(1,1) PRIMARY KEY,
    UserID      INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE,
    Address     NVARCHAR(250) NULL,
    Latitude    DECIMAL(9,6) NULL,
    Longitude   DECIMAL(9,6) NULL
);
```

### SkillCategories
```sql
CREATE TABLE SkillCategories (
    CategoryID  INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(80) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    IsActive    BIT NOT NULL DEFAULT 1
);
```

### Labourers
```sql
CREATE TABLE Labourers (
    LabourerID         INT IDENTITY(1,1) PRIMARY KEY,
    UserID             INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE,
    CategoryID         INT NOT NULL FOREIGN KEY REFERENCES SkillCategories(CategoryID),
    ExperienceYears    INT NOT NULL DEFAULT 0,
    HourlyRate         DECIMAL(10,2) NOT NULL DEFAULT 0,
    Bio                NVARCHAR(500) NULL,
    AvailabilitySchedule NVARCHAR(MAX) NULL,  -- JSON: weekly day->time ranges
    Address            NVARCHAR(250) NULL,
    Latitude           DECIMAL(9,6) NULL,
    Longitude          DECIMAL(9,6) NULL,
    ReputationScore    DECIMAL(5,2) NOT NULL DEFAULT 0,   -- recency-weighted; updated by trigger/SP
    CompletionRate     DECIMAL(5,2) NOT NULL DEFAULT 0,   -- %
    VerificationStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (VerificationStatus IN ('Pending','Approved','Rejected')),
    VerificationNote   NVARCHAR(500) NULL,
    IsProfileLive      BIT NOT NULL DEFAULT 0
);
```

### JobRequests (abstract in C#; status drives subtype via Factory/State)
```sql
CREATE TABLE JobRequests (
    RequestID       INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID      INT NOT NULL FOREIGN KEY REFERENCES Customers(CustomerID) ON DELETE CASCADE,
    LabourerID      INT NOT NULL FOREIGN KEY REFERENCES Labourers(LabourerID),
    JobDescription  NVARCHAR(500) NOT NULL,
    PreferredDate   DATE NOT NULL,
    PreferredTime   TIME NOT NULL,
    Location        NVARCHAR(250) NOT NULL,
    Latitude        DECIMAL(9,6) NULL,
    Longitude       DECIMAL(9,6) NULL,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'Pending'
                    CHECK (Status IN ('Pending','Accepted','InProgress','Completed','Rejected')),
    RejectionReason NVARCHAR(500) NULL,
    EstimatedHours  DECIMAL(5,2) NOT NULL DEFAULT 2,   -- used for earnings estimate
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

### RequestStatusHistory (audit trail — written by trigger)
```sql
CREATE TABLE RequestStatusHistory (
    HistoryID  INT IDENTITY(1,1) PRIMARY KEY,
    RequestID  INT NOT NULL FOREIGN KEY REFERENCES JobRequests(RequestID) ON DELETE CASCADE,
    OldStatus  NVARCHAR(20) NULL,
    NewStatus  NVARCHAR(20) NOT NULL,
    ChangedAt  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

### Reviews
```sql
CREATE TABLE Reviews (
    ReviewID    INT IDENTITY(1,1) PRIMARY KEY,
    RequestID   INT NOT NULL UNIQUE FOREIGN KEY REFERENCES JobRequests(RequestID) ON DELETE CASCADE,
    StarRating  INT NOT NULL CHECK (StarRating BETWEEN 1 AND 5),
    ReviewText  NVARCHAR(1000) NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

### Disputes
```sql
CREATE TABLE Disputes (
    DisputeID      INT IDENTITY(1,1) PRIMARY KEY,
    RequestID      INT NOT NULL FOREIGN KEY REFERENCES JobRequests(RequestID) ON DELETE CASCADE,
    RaisedByUserID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    Description    NVARCHAR(1000) NOT NULL,
    Status         NVARCHAR(20) NOT NULL DEFAULT 'Open' CHECK (Status IN ('Open','Resolved')),
    Resolution     NVARCHAR(1000) NULL,
    CreatedAt      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ResolvedAt     DATETIME2 NULL
);
```

### DisputeChatSessions (temporary 3-way chat container)
```sql
CREATE TABLE DisputeChatSessions (
    SessionID  INT IDENTITY(1,1) PRIMARY KEY,
    DisputeID  INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Disputes(DisputeID) ON DELETE CASCADE,
    IsActive   BIT NOT NULL DEFAULT 1,      -- set 0 when dispute resolved -> chat box disappears
    CreatedAt  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ClosedAt   DATETIME2 NULL
);
```

### DisputeChatMessages
```sql
CREATE TABLE DisputeChatMessages (
    MessageID    INT IDENTITY(1,1) PRIMARY KEY,
    SessionID    INT NOT NULL FOREIGN KEY REFERENCES DisputeChatSessions(SessionID) ON DELETE CASCADE,
    SenderUserID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    SenderRole   NVARCHAR(20) NOT NULL,
    MessageText  NVARCHAR(1000) NOT NULL,
    SentAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

### Notifications
```sql
CREATE TABLE Notifications (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID    INT NOT NULL FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE,
    Message   NVARCHAR(300) NOT NULL,
    IsRead    BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

### SearchLogs (demand tracker)
```sql
CREATE TABLE SearchLogs (
    SearchID   INT IDENTITY(1,1) PRIMARY KEY,
    CategoryID INT NULL FOREIGN KEY REFERENCES SkillCategories(CategoryID),
    CustomerID INT NULL FOREIGN KEY REFERENCES Customers(CustomerID),
    SearchedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

---

## 2. Indexes (performance — proposal requires these)
```sql
CREATE INDEX IX_Labourers_Category   ON Labourers(CategoryID);
CREATE INDEX IX_Labourers_Live       ON Labourers(IsProfileLive, VerificationStatus);
CREATE INDEX IX_JobRequests_Status   ON JobRequests(Status);
CREATE INDEX IX_JobRequests_Labourer ON JobRequests(LabourerID);
CREATE INDEX IX_JobRequests_Customer ON JobRequests(CustomerID);
CREATE INDEX IX_JobRequests_Created  ON JobRequests(CreatedAt);
CREATE INDEX IX_Users_Role           ON Users(Role);
CREATE INDEX IX_SearchLogs_Category  ON SearchLogs(CategoryID);
```

---

## 3. Views
```sql
CREATE VIEW vw_TopRatedLabourers AS
  SELECT TOP 50 l.LabourerID, u.FullName, u.Email, c.Name AS CategoryName,
         l.ReputationScore, l.CompletionRate, l.HourlyRate, l.ExperienceYears
  FROM Labourers l
  JOIN Users u ON u.UserID = l.UserID
  JOIN SkillCategories c ON c.CategoryID = l.CategoryID
  WHERE l.IsProfileLive = 1
  ORDER BY l.ReputationScore DESC, l.CompletionRate DESC;

CREATE VIEW vw_ActiveJobRequests AS
  SELECT r.RequestID, r.JobDescription, r.PreferredDate, r.PreferredTime, r.Location,
         r.Status, cu.FullName AS CustomerName, lu.FullName AS LabourerName,
         r.CreatedAt, r.UpdatedAt
  FROM JobRequests r
  JOIN Customers c ON c.CustomerID = r.CustomerID
  JOIN Users cu ON cu.UserID = c.UserID
  JOIN Labourers l ON l.LabourerID = r.LabourerID
  JOIN Users lu ON lu.UserID = l.UserID
  WHERE r.Status IN ('Pending','Accepted','InProgress');

CREATE VIEW vw_LabourerEarningsSummary AS
  SELECT l.LabourerID, u.FullName,
         COUNT(CASE WHEN r.Status='Completed' THEN 1 END) AS CompletedJobs,
         l.HourlyRate,
         SUM(CASE WHEN r.Status='Completed' THEN r.EstimatedHours * l.HourlyRate ELSE 0 END) AS EstimatedEarnings
  FROM Labourers l
  JOIN Users u ON u.UserID = l.UserID
  LEFT JOIN JobRequests r ON r.LabourerID = l.LabourerID
  GROUP BY l.LabourerID, u.FullName, l.HourlyRate;

CREATE VIEW vw_UnmetDemand AS
  SELECT c.CategoryID, c.Name AS CategoryName,
         COUNT(s.SearchID) AS SearchCount,
         (SELECT COUNT(*) FROM Labourers l WHERE l.CategoryID=c.CategoryID AND l.IsProfileLive=1) AS LiveLabourerCount,
         COUNT(s.SearchID) - (SELECT COUNT(*) FROM Labourers l WHERE l.CategoryID=c.CategoryID AND l.IsProfileLive=1) AS DemandGap
  FROM SkillCategories c
  LEFT JOIN SearchLogs s ON s.CategoryID = c.CategoryID
  GROUP BY c.CategoryID, c.Name;

CREATE VIEW vw_PlatformHealthDashboard AS
  SELECT
    (SELECT COUNT(*) FROM Users WHERE Role='Customer') AS TotalCustomers,
    (SELECT COUNT(*) FROM Labourers WHERE IsProfileLive=1) AS ActiveLabourers,
    (SELECT COUNT(*) FROM JobRequests) AS TotalJobs,
    (SELECT COUNT(*) FROM JobRequests WHERE Status='Completed') AS CompletedJobs,
    (SELECT AVG(CAST(StarRating AS DECIMAL(4,2))) FROM Reviews) AS AvgRating;
```

---

## 4. Stored procedures (the repository contract)

> List of names the repositories call. Implement each with parameterized logic, transactions where
> multiple rows change, and JOIN/aggregate as noted. (Signatures abbreviated; implement fully.)

**Auth & users**
- `sp_RegisterUser(@FullName,@Email,@PasswordHash,@Role,@Phone)` → returns new `UserID` (TRANSACTION when also creating Customer/Labourer row).
- `sp_RegisterCustomer(@UserID,@Address,@Lat,@Lng)`
- `sp_RegisterLabourer(@UserID,@CategoryID,@ExperienceYears,@HourlyRate,@Bio,@Availability,@Address,@Lat,@Lng)` → status Pending, not live.
- `sp_LoginUser(@Email)` → returns user row incl. `PasswordHash`, `Role`, `IsSuspended` (app verifies hash).
- `sp_GetUserById(@UserID)`, `sp_GetCustomerByUserId(@UserID)`, `sp_GetLabourerByUserId(@UserID)`
- `sp_GetAllUsers(@Search=NULL)`, `sp_SuspendUser(@UserID,@Suspend BIT)`

**Search / matching / proximity**
- `sp_GetSkillCategories(@ActiveOnly BIT=1)`
- `sp_SearchLabourers(@CategoryID=NULL,@MinRating=NULL,@MinExperience=NULL,@Location=NULL)` — verified+live only; returns ranking inputs.
- `sp_SearchLabourersNearby(@Lat,@Lng,@RadiusKm,@CategoryID=NULL,@MinRating=NULL,@MinExperience=NULL)` —
  filters by `geography::Point(@Lat,@Lng,4326).STDistance(geography::Point(Latitude,Longitude,4326)) <= @RadiusKm*1000`;
  returns each labourer with `DistanceKm = .../1000`. **Logs the search** into `SearchLogs`.
- `sp_GetLabourerProfile(@LabourerID)` → profile + reviews + completed/active counts (multiple result sets).

**Jobs**
- `sp_CreateJobRequest(@CustomerID,@LabourerID,@JobDescription,@PreferredDate,@PreferredTime,@Location,@Lat,@Lng,@EstimatedHours)` — TRANSACTION; inserts request (Status Pending).
- `sp_CheckAvailabilityConflict(@LabourerID,@PreferredDate,@PreferredTime)` → bit/result indicating conflict with weekly schedule or existing Accepted/InProgress bookings.
- `sp_UpdateRequestStatus(@RequestID,@NewStatus,@RejectionReason=NULL)` — TRANSACTION; validates transition; sets `UpdatedAt`.
- `sp_GetActiveJobRequests(@Role,@RefId)` / `sp_GetJobRequestById(@RequestID)`
- `sp_GetCustomerRequests(@CustomerID)` (history), `sp_GetIncomingRequests(@LabourerID)`
- `sp_GetCompletedJobsByLabourer(@LabourerID)`

**Reviews & reputation**
- `sp_SubmitReview(@RequestID,@StarRating,@ReviewText)` — only if job Completed; trigger recalcs reputation.
- `sp_RecalculateReputation(@LabourerID)` — **recency-weighted**: newer reviews weigh more
  (e.g. weight = `EXP(-DATEDIFF(day, CreatedAt, GETDATE())/90.0)`), score = weighted avg → scaled 0–100; also recompute `CompletionRate`.
- `sp_GetReviewsForLabourer(@LabourerID)`, `sp_GetTopRatedLabourers()`

**Labourer dashboard / earnings**
- `sp_GetLabourerDashboard(@LabourerID)` — KPIs + incoming requests.
- `sp_GetLabourerEarnings(@LabourerID)` — from `vw_LabourerEarningsSummary`.
- `sp_GetLabourerEarningsTimeSeries(@LabourerID,@Days=90)` — earnings per day/week (Chart.js).
- `sp_GetLabourerRatingTrend(@LabourerID,@Days=90)` — avg rating over time.

**Admin verification & categories**
- `sp_GetVerificationQueue()` — pending labourers + their user/category.
- `sp_ApproveLabourer(@LabourerID)` — sets Approved; trigger sets `IsProfileLive=1` + notifies.
- `sp_RejectLabourer(@LabourerID,@AdminNote)` — sets Rejected + note + notifies.
- `sp_AddSkillCategory(@Name,@Description)`, `sp_UpdateSkillCategory(@CategoryID,@Name,@Description,@IsActive)`

**Admin analytics (Feature 3)**
- `sp_GetAdminStats()` → TotalUsers, ActiveLabourers, PendingVerifications, TotalJobRequests, JobsToday, CompletionRate, AvgRating (+ last-week deltas).
- `sp_GetUserGrowth(@Days=30)` → date, NewUsers (and cumulative).
- `sp_GetDailyDeals(@Days=30)` → date, CompletedJobs, EstimatedValue.  (the "daily transactions/deals done")
- `sp_GetCategoryDistribution()` → CategoryName, LabourerCount, JobCount.
- `sp_GetUnmetDemandReport()` → from `vw_UnmetDemand` ordered by DemandGap DESC.

**Disputes & chat (Feature 4)**
- `sp_RaiseDispute(@RequestID,@RaisedByUserID,@Description)` — TRANSACTION: insert Dispute (Open) + DisputeChatSession (active) + notify the other party & admin.
- `sp_GetDisputes(@Status=NULL)` — admin list w/ customer+labourer names, job desc.
- `sp_GetDisputesForUser(@UserID)` — disputes where the user is participant (open chats to show).
- `sp_GetDisputeChat(@DisputeID)` — session + ordered messages + participant names/roles.
- `sp_AddDisputeMessage(@DisputeID,@SenderUserID,@SenderRole,@MessageText)`
- `sp_ResolveDispute(@DisputeID,@Resolution)` — TRANSACTION: Dispute→Resolved + ResolvedAt, Session IsActive=0 + ClosedAt, notify participants.

**Notifications**
- `sp_InsertNotification(@UserID,@Message)`, `sp_GetNotifications(@UserID)`,
  `sp_GetUnreadNotificationCount(@UserID)`, `sp_MarkNotificationsRead(@UserID)`

---

## 5. Triggers (proposal requires ~5; implement these)
```sql
-- (1) Recalculate reputation after a review insert
CREATE TRIGGER trg_Review_Insert ON Reviews AFTER INSERT AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @LabourerID INT =
    (SELECT TOP 1 r.LabourerID FROM JobRequests r JOIN inserted i ON i.RequestID=r.RequestID);
  EXEC sp_RecalculateReputation @LabourerID;
END;

-- (2) Log status changes into RequestStatusHistory
CREATE TRIGGER trg_JobStatus_Update ON JobRequests AFTER UPDATE AS
BEGIN
  SET NOCOUNT ON;
  INSERT INTO RequestStatusHistory(RequestID, OldStatus, NewStatus)
  SELECT i.RequestID, d.Status, i.Status
  FROM inserted i JOIN deleted d ON i.RequestID=d.RequestID
  WHERE i.Status <> d.Status;
END;

-- (3) Create notifications on accept/reject/complete (insert into Notifications for both parties)
CREATE TRIGGER trg_JobStatus_Notify ON JobRequests AFTER UPDATE AS ...;

-- (4) Flag account after 3+ unresolved disputes
CREATE TRIGGER trg_Dispute_Flag ON Disputes AFTER INSERT AS
BEGIN
  SET NOCOUNT ON;
  UPDATE u SET DisputeStrikes = (
    SELECT COUNT(*) FROM Disputes dd JOIN JobRequests rr ON rr.RequestID=dd.RequestID
    /* count disputes against this user */ WHERE dd.Status='Open' AND ... )
  FROM Users u ...;
  -- if strikes>=3, insert an admin notification
END;

-- (5) Activate profile when admin approves
CREATE TRIGGER trg_Labourer_Approve ON Labourers AFTER UPDATE AS
BEGIN
  SET NOCOUNT ON;
  UPDATE Labourers SET IsProfileLive=1
  FROM Labourers l JOIN inserted i ON l.LabourerID=i.LabourerID
  WHERE i.VerificationStatus='Approved';
END;
```
> Note: if two AFTER UPDATE triggers on `JobRequests` are undesirable, combine (2)+(3) into one.

---

## 6. Seed data
```sql
-- Skill categories
INSERT INTO SkillCategories(Name,Description) VALUES
 ('Electrician','Wiring, fixtures, fault repair'),
 ('Plumber','Pipes, leaks, fittings'),
 ('Carpenter','Furniture, woodwork, fittings'),
 ('Painter','Interior/exterior painting'),
 ('Mason','Brickwork, plaster, concrete'),
 ('AC Technician','AC install & service'),
 ('Solar Panel Technician','Solar install & maintenance');

-- Admin (replace hash with a real BCrypt/SHA-256 of 'Admin@123')
INSERT INTO Users(FullName,Email,PasswordHash,Role)
 VALUES ('Platform Admin','admin@Hunarmand.pk','<HASH_OF_Admin@123>','Admin');
```
Provide a few seed customers and **approved+live** labourers WITH latitude/longitude around Lahore
(e.g. 31.5204, 74.3587 ± small offsets) so the map filter has data on first run.

---

## 7. Setup steps (put in README)
1. Create DB `HunarmandDB`.
2. Run `Database/schema.sql` (this whole script).
3. Confirm connection string in `appsettings.json`.
4. `dotnet restore && dotnet run`.
