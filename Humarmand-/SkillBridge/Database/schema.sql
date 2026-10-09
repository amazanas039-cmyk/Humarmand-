-- Single canonical database schema script for SkillBridgeDB
-- Run order: tables -> indexes -> views -> procedures -> triggers -> seed data

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SkillBridgeDB')
BEGIN
    CREATE DATABASE SkillBridgeDB;
END
GO

USE SkillBridgeDB;
GO

-- Cleanup existing triggers
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Review_Insert') DROP TRIGGER trg_Review_Insert;
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_JobStatus_Update') DROP TRIGGER trg_JobStatus_Update;
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_JobStatus_Notify') DROP TRIGGER trg_JobStatus_Notify;
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Dispute_Flag') DROP TRIGGER trg_Dispute_Flag;
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Labourer_Approve') DROP TRIGGER trg_Labourer_Approve;
GO

-- Cleanup existing stored procedures
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_RegisterCustomer') DROP PROCEDURE sp_RegisterCustomer;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_RegisterLabourer') DROP PROCEDURE sp_RegisterLabourer;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetUserByEmail') DROP PROCEDURE sp_GetUserByEmail;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetUserById') DROP PROCEDURE sp_GetUserById;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_UpdateUserProfile') DROP PROCEDURE sp_UpdateUserProfile;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_SuspendUser') DROP PROCEDURE sp_SuspendUser;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetAllUsersForAdmin') DROP PROCEDURE sp_GetAllUsersForAdmin;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetPendingLabourers') DROP PROCEDURE sp_GetPendingLabourers;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_ApproveLabourer') DROP PROCEDURE sp_ApproveLabourer;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_RejectLabourer') DROP PROCEDURE sp_RejectLabourer;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetLabourerProfile') DROP PROCEDURE sp_GetLabourerProfile;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_UpdateLabourerProfile') DROP PROCEDURE sp_UpdateLabourerProfile;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_UpdateLabourerAvailability') DROP PROCEDURE sp_UpdateLabourerAvailability;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_SearchLabourersNearby') DROP PROCEDURE sp_SearchLabourersNearby;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_SearchLabourers') DROP PROCEDURE sp_SearchLabourers;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_CreateJobRequest') DROP PROCEDURE sp_CreateJobRequest;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetJobRequestById') DROP PROCEDURE sp_GetJobRequestById;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_UpdateJobStatus') DROP PROCEDURE sp_UpdateJobStatus;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetJobsForCustomer') DROP PROCEDURE sp_GetJobsForCustomer;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetJobsForLabourer') DROP PROCEDURE sp_GetJobsForLabourer;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_CheckLabourerAvailability') DROP PROCEDURE sp_CheckLabourerAvailability;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_CreateReview') DROP PROCEDURE sp_CreateReview;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetReviewsForLabourer') DROP PROCEDURE sp_GetReviewsForLabourer;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_HasReviewed') DROP PROCEDURE sp_HasReviewed;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_RaiseDispute') DROP PROCEDURE sp_RaiseDispute;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetDisputeById') DROP PROCEDURE sp_GetDisputeById;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetOpenDisputes') DROP PROCEDURE sp_GetOpenDisputes;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetDisputesForUser') DROP PROCEDURE sp_GetDisputesForUser;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_ResolveDispute') DROP PROCEDURE sp_ResolveDispute;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_AddChatMessage') DROP PROCEDURE sp_AddChatMessage;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetChatMessages') DROP PROCEDURE sp_GetChatMessages;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetActiveChatSessions') DROP PROCEDURE sp_GetActiveChatSessions;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_CreateNotification') DROP PROCEDURE sp_CreateNotification;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetUnreadNotifications') DROP PROCEDURE sp_GetUnreadNotifications;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_MarkNotificationRead') DROP PROCEDURE sp_MarkNotificationRead;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_MarkAllNotificationsRead') DROP PROCEDURE sp_MarkAllNotificationsRead;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetNotificationCount') DROP PROCEDURE sp_GetNotificationCount;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetPlatformKPIs') DROP PROCEDURE sp_GetPlatformKPIs;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetUserGrowthTrend') DROP PROCEDURE sp_GetUserGrowthTrend;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetDailyCompletedJobs') DROP PROCEDURE sp_GetDailyCompletedJobs;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetCategoryDistribution') DROP PROCEDURE sp_GetCategoryDistribution;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetTopRatedLabourers') DROP PROCEDURE sp_GetTopRatedLabourers;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetUnmetDemand') DROP PROCEDURE sp_GetUnmetDemand;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetLabourerEarningsTrend') DROP PROCEDURE sp_GetLabourerEarningsTrend;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetLabourerRatingTrend') DROP PROCEDURE sp_GetLabourerRatingTrend;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetLabourerJobFunnel') DROP PROCEDURE sp_GetLabourerJobFunnel;
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_RecalculateReputation') DROP PROCEDURE sp_RecalculateReputation;
GO

-- Cleanup existing views
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_TopRatedLabourers') DROP VIEW vw_TopRatedLabourers;
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_ActiveJobRequests') DROP VIEW vw_ActiveJobRequests;
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_LabourerEarningsSummary') DROP VIEW vw_LabourerEarningsSummary;
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_UnmetDemand') DROP VIEW vw_UnmetDemand;
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_PlatformHealthDashboard') DROP VIEW vw_PlatformHealthDashboard;
GO

-- Cleanup existing tables in reverse dependency order
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SearchLogs_Customers') ALTER TABLE SearchLogs DROP CONSTRAINT FK_SearchLogs_Customers;
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SearchLogs_Categories') ALTER TABLE SearchLogs DROP CONSTRAINT FK_SearchLogs_Categories;

DROP TABLE IF EXISTS DemandSearchLog;
DROP TABLE IF EXISTS SearchLogs;
DROP TABLE IF EXISTS Notifications;
DROP TABLE IF EXISTS DisputeChatMessages;
DROP TABLE IF EXISTS DisputeChatSessions;
DROP TABLE IF EXISTS Disputes;
DROP TABLE IF EXISTS Reviews;
DROP TABLE IF EXISTS RequestStatusHistory;
DROP TABLE IF EXISTS JobRequests;
DROP TABLE IF EXISTS Labourers;
DROP TABLE IF EXISTS SkillCategories;
DROP TABLE IF EXISTS Customers;
DROP TABLE IF EXISTS Admins;
DROP TABLE IF EXISTS Users;
GO

-- ==========================================
-- 1. TABLES
-- ==========================================

CREATE TABLE Users (
    UserID        INT IDENTITY(1,1) PRIMARY KEY,
    FullName      NVARCHAR(150) NOT NULL,
    Email         NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash  NVARCHAR(255) NOT NULL,
    Role          NVARCHAR(20)  NOT NULL CHECK (Role IN ('Customer','Labourer','Admin')),
    Phone         NVARCHAR(20)  NULL,
    IsActive      BIT NOT NULL DEFAULT 1,
    IsSuspended   BIT NOT NULL DEFAULT 0,
    DisputeStrikes INT NOT NULL DEFAULT 0,
    CreatedAt     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Admins (
    AdminID INT IDENTITY(1,1) PRIMARY KEY,
    UserID  INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE
);

CREATE TABLE Customers (
    CustomerID  INT IDENTITY(1,1) PRIMARY KEY,
    UserID      INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE,
    Address     NVARCHAR(250) NULL,
    Latitude    DECIMAL(9,6) NULL,
    Longitude   DECIMAL(9,6) NULL
);

CREATE TABLE SkillCategories (
    CategoryID  INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(80) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    IsActive    BIT NOT NULL DEFAULT 1
);

CREATE TABLE Labourers (
    LabourerID         INT IDENTITY(1,1) PRIMARY KEY,
    UserID             INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE,
    CategoryID         INT NOT NULL FOREIGN KEY REFERENCES SkillCategories(CategoryID),
    ExperienceYears    INT NOT NULL DEFAULT 0,
    HourlyRate         DECIMAL(10,2) NOT NULL DEFAULT 0,
    Bio                NVARCHAR(500) NULL,
    AvailabilitySchedule NVARCHAR(MAX) NULL,  -- JSON schedule
    Address            NVARCHAR(250) NULL,
    Latitude           DECIMAL(9,6) NULL,
    Longitude          DECIMAL(9,6) NULL,
    ReputationScore    DECIMAL(5,2) NOT NULL DEFAULT 0,
    CompletionRate     DECIMAL(5,2) NOT NULL DEFAULT 0,
    VerificationStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (VerificationStatus IN ('Pending','Approved','Rejected')),
    VerificationNote   NVARCHAR(500) NULL,
    IsProfileLive      BIT NOT NULL DEFAULT 0
);

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
    Status          NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending','Accepted','InProgress','Completed','Rejected')),
    RejectionReason NVARCHAR(500) NULL,
    EstimatedHours  DECIMAL(5,2) NOT NULL DEFAULT 2.00,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE RequestStatusHistory (
    HistoryID  INT IDENTITY(1,1) PRIMARY KEY,
    RequestID  INT NOT NULL FOREIGN KEY REFERENCES JobRequests(RequestID) ON DELETE CASCADE,
    OldStatus  NVARCHAR(20) NULL,
    NewStatus  NVARCHAR(20) NOT NULL,
    ChangedAt  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Reviews (
    ReviewID    INT IDENTITY(1,1) PRIMARY KEY,
    RequestID   INT NOT NULL UNIQUE FOREIGN KEY REFERENCES JobRequests(RequestID) ON DELETE CASCADE,
    TargetLabourerID INT NOT NULL FOREIGN KEY REFERENCES Labourers(LabourerID),
    ReviewerUserID   INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    StarRating  INT NOT NULL CHECK (StarRating BETWEEN 1 AND 5),
    ReviewText  NVARCHAR(1000) NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Disputes (
    DisputeID      INT IDENTITY(1,1) PRIMARY KEY,
    RequestID      INT NOT NULL FOREIGN KEY REFERENCES JobRequests(RequestID) ON DELETE CASCADE,
    RaisedByUserID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    Reason         NVARCHAR(1000) NOT NULL,
    Status         NVARCHAR(20) NOT NULL DEFAULT 'Open' CHECK (Status IN ('Open','Resolved')),
    Resolution     NVARCHAR(1000) NULL,
    CreatedAt      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ResolvedAt     DATETIME2 NULL
);

CREATE TABLE DisputeChatSessions (
    SessionID  INT IDENTITY(1,1) PRIMARY KEY,
    DisputeID  INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Disputes(DisputeID) ON DELETE CASCADE,
    IsActive   BIT NOT NULL DEFAULT 1,
    CreatedAt  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ClosedAt   DATETIME2 NULL
);

CREATE TABLE DisputeChatMessages (
    MessageID    INT IDENTITY(1,1) PRIMARY KEY,
    SessionID    INT NOT NULL FOREIGN KEY REFERENCES DisputeChatSessions(SessionID) ON DELETE CASCADE,
    SenderUserID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    SenderRole   NVARCHAR(20) NOT NULL,
    MessageText  NVARCHAR(MAX) NOT NULL,
    SentAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Notifications (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID         INT NOT NULL FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE,
    Message        NVARCHAR(300) NOT NULL,
    NotificationType NVARCHAR(50) NULL,
    IsRead         BIT NOT NULL DEFAULT 0,
    CreatedAt      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE DemandSearchLog (
    SearchID   INT IDENTITY(1,1) PRIMARY KEY,
    CategoryID INT NULL FOREIGN KEY REFERENCES SkillCategories(CategoryID),
    CustomerID INT NULL FOREIGN KEY REFERENCES Customers(CustomerID),
    SearchedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ==========================================
-- 2. INDEXES
-- ==========================================

CREATE INDEX IX_Labourers_Category   ON Labourers(CategoryID);
CREATE INDEX IX_Labourers_Live       ON Labourers(IsProfileLive, VerificationStatus);
CREATE INDEX IX_JobRequests_Status   ON JobRequests(Status);
CREATE INDEX IX_JobRequests_Labourer ON JobRequests(LabourerID);
CREATE INDEX IX_JobRequests_Customer ON JobRequests(CustomerID);
CREATE INDEX IX_JobRequests_Created  ON JobRequests(CreatedAt);
CREATE INDEX IX_Users_Role           ON Users(Role);
CREATE INDEX IX_DemandSearchLog_Category ON DemandSearchLog(CategoryID);
GO

-- ==========================================
-- 3. VIEWS
-- ==========================================

CREATE VIEW vw_TopRatedLabourers AS
  SELECT TOP 50 l.LabourerID, u.FullName, u.Email, c.Name AS CategoryName,
         l.ReputationScore, l.CompletionRate, l.HourlyRate, l.ExperienceYears
  FROM Labourers l
  JOIN Users u ON u.UserID = l.UserID
  JOIN SkillCategories c ON c.CategoryID = l.CategoryID
  WHERE l.IsProfileLive = 1
  ORDER BY l.ReputationScore DESC, l.CompletionRate DESC;
GO

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
GO

CREATE VIEW vw_LabourerEarningsSummary AS
  SELECT l.LabourerID, u.FullName,
         COUNT(CASE WHEN r.Status='Completed' THEN 1 END) AS CompletedJobs,
         l.HourlyRate,
         SUM(CASE WHEN r.Status='Completed' THEN r.EstimatedHours * l.HourlyRate ELSE 0 END) AS EstimatedEarnings
  FROM Labourers l
  JOIN Users u ON u.UserID = l.UserID
  LEFT JOIN JobRequests r ON r.LabourerID = l.LabourerID
  GROUP BY l.LabourerID, u.FullName, l.HourlyRate;
GO

CREATE VIEW vw_UnmetDemand AS
  SELECT c.CategoryID, c.Name AS CategoryName,
         COUNT(s.SearchID) AS SearchCount,
         (SELECT COUNT(*) FROM Labourers l WHERE l.CategoryID=c.CategoryID AND l.IsProfileLive=1) AS LiveLabourerCount,
         COUNT(s.SearchID) - (SELECT COUNT(*) FROM Labourers l WHERE l.CategoryID=c.CategoryID AND l.IsProfileLive=1) AS DemandGap
  FROM SkillCategories c
  LEFT JOIN DemandSearchLog s ON s.CategoryID = c.CategoryID
  GROUP BY c.CategoryID, c.Name;
GO

CREATE VIEW vw_PlatformHealthDashboard AS
  SELECT
    (SELECT COUNT(*) FROM Users WHERE Role='Customer') AS TotalCustomers,
    (SELECT COUNT(*) FROM Labourers WHERE IsProfileLive=1) AS ActiveLabourers,
    (SELECT COUNT(*) FROM JobRequests) AS TotalJobs,
    (SELECT COUNT(*) FROM JobRequests WHERE Status='Completed') AS CompletedJobs,
    (SELECT AVG(CAST(StarRating AS DECIMAL(4,2))) FROM Reviews) AS AvgRating;
GO

-- ==========================================
-- 4. PROCEDURES
-- ==========================================

-- Auth & Registration
CREATE PROCEDURE sp_RegisterCustomer
    @FullName NVARCHAR(150),
    @Email NVARCHAR(150),
    @PasswordHash NVARCHAR(255),
    @Phone NVARCHAR(20),
    @Address NVARCHAR(250),
    @Latitude DECIMAL(9,6),
    @Longitude DECIMAL(9,6)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO Users (FullName, Email, PasswordHash, Role, Phone)
        VALUES (@FullName, @Email, @PasswordHash, 'Customer', @Phone);
        
        DECLARE @uid INT = SCOPE_IDENTITY();
        
        INSERT INTO Customers (UserID, Address, Latitude, Longitude)
        VALUES (@uid, @Address, @Latitude, @Longitude);
        
        COMMIT;
        SELECT @uid AS UserID;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END;
GO

CREATE PROCEDURE sp_RegisterLabourer
    @FullName NVARCHAR(150),
    @Email NVARCHAR(150),
    @PasswordHash NVARCHAR(255),
    @Phone NVARCHAR(20),
    @CategoryID INT,
    @ExperienceYears INT,
    @HourlyRate DECIMAL(10,2),
    @Bio NVARCHAR(500),
    @AvailabilitySchedule NVARCHAR(MAX),
    @Address NVARCHAR(250),
    @Latitude DECIMAL(9,6),
    @Longitude DECIMAL(9,6)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO Users (FullName, Email, PasswordHash, Role, Phone)
        VALUES (@FullName, @Email, @PasswordHash, 'Labourer', @Phone);
        
        DECLARE @uid INT = SCOPE_IDENTITY();
        
        INSERT INTO Labourers (UserID, CategoryID, ExperienceYears, HourlyRate, Bio, AvailabilitySchedule, Address, Latitude, Longitude, VerificationStatus, IsProfileLive)
        VALUES (@uid, @CategoryID, @ExperienceYears, @HourlyRate, @Bio, @AvailabilitySchedule, @Address, @Latitude, @Longitude, 'Pending', 0);
        
        COMMIT;
        SELECT @uid AS UserID;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END;
GO

CREATE PROCEDURE sp_GetUserByEmail
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserID, FullName, Email, PasswordHash, Role, Phone, IsActive, IsSuspended, DisputeStrikes, CreatedAt
    FROM Users
    WHERE Email = @Email;
END;
GO

CREATE PROCEDURE sp_GetUserById
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserID, FullName, Email, PasswordHash, Role, Phone, IsActive, IsSuspended, DisputeStrikes, CreatedAt
    FROM Users
    WHERE UserID = @UserID;
END;
GO

CREATE PROCEDURE sp_UpdateUserProfile
    @UserID INT,
    @FullName NVARCHAR(150),
    @Phone NVARCHAR(20),
    @Address NVARCHAR(250),
    @Latitude DECIMAL(9,6),
    @Longitude DECIMAL(9,6)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE Users 
        SET FullName = @FullName, Phone = @Phone 
        WHERE UserID = @UserID;
        
        IF EXISTS (SELECT 1 FROM Customers WHERE UserID = @UserID)
        BEGIN
            UPDATE Customers 
            SET Address = @Address, Latitude = @Latitude, Longitude = @Longitude 
            WHERE UserID = @UserID;
        END
        ELSE IF EXISTS (SELECT 1 FROM Labourers WHERE UserID = @UserID)
        BEGIN
            UPDATE Labourers 
            SET Address = @Address, Latitude = @Latitude, Longitude = @Longitude 
            WHERE UserID = @UserID;
        END
        
        COMMIT;
        SELECT 1 AS Affected;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END;
GO

CREATE PROCEDURE sp_SuspendUser
    @UserID INT,
    @IsSuspended BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users 
    SET IsSuspended = @IsSuspended, IsActive = CASE WHEN @IsSuspended = 1 THEN 0 ELSE 1 END
    WHERE UserID = @UserID;
END;
GO

CREATE PROCEDURE sp_GetAllUsersForAdmin
    @Search NVARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserID, u.FullName, u.Email, u.Role, u.Phone, u.IsActive, u.IsSuspended, u.DisputeStrikes, u.CreatedAt
    FROM Users u
    WHERE (@Search IS NULL OR u.FullName LIKE '%' + @Search + '%' OR u.Email LIKE '%' + @Search + '%')
    ORDER BY u.CreatedAt DESC;
END;
GO

-- Labourer specific procedures
CREATE PROCEDURE sp_GetPendingLabourers
AS
BEGIN
    SET NOCOUNT ON;
    SELECT l.LabourerID, u.UserID, u.FullName, u.Email, u.Phone, sc.Name AS CategoryName, l.ExperienceYears, l.HourlyRate, l.Bio, l.VerificationStatus, u.CreatedAt
    FROM Labourers l
    JOIN Users u ON l.UserID = u.UserID
    JOIN SkillCategories sc ON l.CategoryID = sc.CategoryID
    WHERE l.VerificationStatus = 'Pending'
    ORDER BY u.CreatedAt DESC;
END;
GO

CREATE PROCEDURE sp_ApproveLabourer
    @LabourerID INT,
    @Note NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Labourers
    SET VerificationStatus = 'Approved', VerificationNote = @Note, IsProfileLive = 1
    WHERE LabourerID = @LabourerID;
END;
GO

CREATE PROCEDURE sp_RejectLabourer
    @LabourerID INT,
    @Note NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Labourers
    SET VerificationStatus = 'Rejected', VerificationNote = @Note, IsProfileLive = 0
    WHERE LabourerID = @LabourerID;
    
    -- Notify the Labourer
    DECLARE @uid INT = (SELECT UserID FROM Labourers WHERE LabourerID = @LabourerID);
    IF @uid IS NOT NULL
    BEGIN
        INSERT INTO Notifications (UserID, Message, NotificationType)
        VALUES (@uid, 'Your verification profile was rejected: ' + @Note, 'VerificationRejected');
    END
END;
GO

CREATE PROCEDURE sp_GetLabourerProfile
    @LabourerID INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Select 1: Profile Details
    SELECT l.LabourerID, u.UserID, u.FullName, u.Email, u.Phone, l.CategoryID, sc.Name AS CategoryName,
           l.ExperienceYears, l.HourlyRate, l.Bio, l.AvailabilitySchedule, l.Address, l.Latitude, l.Longitude,
           l.ReputationScore, l.CompletionRate, l.VerificationStatus, l.IsProfileLive
    FROM Labourers l
    JOIN Users u ON l.UserID = u.UserID
    JOIN SkillCategories sc ON l.CategoryID = sc.CategoryID
    WHERE l.LabourerID = @LabourerID;

    -- Select 2: Reviews
    SELECT r.ReviewID, r.StarRating, r.ReviewText, r.CreatedAt, ru.FullName AS ReviewerName
    FROM Reviews r
    JOIN Users ru ON r.ReviewerUserID = ru.UserID
    WHERE r.TargetLabourerID = @LabourerID
    ORDER BY r.CreatedAt DESC;
END;
GO

CREATE PROCEDURE sp_UpdateLabourerProfile
    @LabourerID INT,
    @Bio NVARCHAR(500),
    @HourlyRate DECIMAL(10,2),
    @Address NVARCHAR(250),
    @Latitude DECIMAL(9,6),
    @Longitude DECIMAL(9,6)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Labourers
    SET Bio = @Bio, HourlyRate = @HourlyRate, Address = @Address, Latitude = @Latitude, Longitude = @Longitude
    WHERE LabourerID = @LabourerID;
END;
GO

CREATE PROCEDURE sp_UpdateLabourerAvailability
    @LabourerID INT,
    @AvailabilitySchedule NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Labourers
    SET AvailabilitySchedule = @AvailabilitySchedule
    WHERE LabourerID = @LabourerID;
END;
GO

CREATE PROCEDURE sp_SearchLabourersNearby
    @CustomerLat DECIMAL(9,6),
    @CustomerLng DECIMAL(9,6),
    @RadiusKm DECIMAL(5,2),
    @CategoryID INT = NULL,
    @MinRating DECIMAL(3,1) = 0,
    @MinExperience INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Log the search for demand tracking
    IF @CategoryID IS NOT NULL
    BEGIN
        INSERT INTO DemandSearchLog (CategoryID, CustomerID)
        VALUES (@CategoryID, NULL);
    END

    DECLARE @CustomerGeo GEOGRAPHY = geography::Point(@CustomerLat, @CustomerLng, 4326);

    SELECT 
        l.LabourerID, u.UserID, u.FullName, u.Phone, l.CategoryID, sc.Name AS CategoryName,
        l.ExperienceYears, l.HourlyRate, l.ReputationScore, l.CompletionRate, l.Bio, l.Address, l.Latitude, l.Longitude,
        ROUND(@CustomerGeo.STDistance(geography::Point(l.Latitude, l.Longitude, 4326)) / 1000.0, 2) AS DistanceKm
    FROM Labourers l
    JOIN Users u ON l.UserID = u.UserID
    JOIN SkillCategories sc ON l.CategoryID = sc.CategoryID
    WHERE l.IsProfileLive = 1
      AND u.IsActive = 1
      AND u.IsSuspended = 0
      AND l.Latitude IS NOT NULL
      AND l.Longitude IS NOT NULL
      AND @CustomerGeo.STDistance(geography::Point(l.Latitude, l.Longitude, 4326)) <= @RadiusKm * 1000
      AND (@CategoryID IS NULL OR l.CategoryID = @CategoryID)
      AND l.ReputationScore >= @MinRating
      AND l.ExperienceYears >= @MinExperience
    ORDER BY DistanceKm ASC;
END;
GO

CREATE PROCEDURE sp_SearchLabourers
    @CategoryID INT = NULL,
    @MinRating DECIMAL(3,1) = 0,
    @MinExperience INT = 0,
    @Location NVARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        l.LabourerID, u.UserID, u.FullName, u.Phone, l.CategoryID, sc.Name AS CategoryName,
        l.ExperienceYears, l.HourlyRate, l.ReputationScore, l.CompletionRate, l.Bio, l.Address, l.Latitude, l.Longitude,
        CAST(NULL AS FLOAT) AS DistanceKm
    FROM Labourers l
    JOIN Users u ON l.UserID = u.UserID
    JOIN SkillCategories sc ON l.CategoryID = sc.CategoryID
    WHERE l.IsProfileLive = 1
      AND u.IsActive = 1
      AND u.IsSuspended = 0
      AND (@CategoryID IS NULL OR l.CategoryID = @CategoryID)
      AND l.ReputationScore >= @MinRating
      AND l.ExperienceYears >= @MinExperience
      AND (@Location IS NULL OR l.Address LIKE '%' + @Location + '%');
END;
GO

-- Job Requests
CREATE PROCEDURE sp_CreateJobRequest
    @CustomerID INT,
    @LabourerID INT,
    @JobDescription NVARCHAR(500),
    @PreferredDate DATE,
    @PreferredTime TIME,
    @Location NVARCHAR(250),
    @Latitude DECIMAL(9,6),
    @Longitude DECIMAL(9,6),
    @EstimatedHours DECIMAL(5,2)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO JobRequests (CustomerID, LabourerID, JobDescription, PreferredDate, PreferredTime, Location, Latitude, Longitude, Status, EstimatedHours)
        VALUES (@CustomerID, @LabourerID, @JobDescription, @PreferredDate, @PreferredTime, @Location, @Latitude, @Longitude, 'Pending', @EstimatedHours);
        
        DECLARE @rid INT = SCOPE_IDENTITY();
        
        COMMIT;
        SELECT @rid AS RequestID;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END;
GO

CREATE PROCEDURE sp_GetJobRequestById
    @RequestID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT jr.RequestID, jr.CustomerID, cu.FullName AS CustomerName, jr.LabourerID, lu.FullName AS LabourerName,
           jr.JobDescription, jr.PreferredDate, jr.PreferredTime, jr.Location, jr.Latitude, jr.Longitude,
           jr.Status, jr.RejectionReason, jr.EstimatedHours, jr.CreatedAt, jr.UpdatedAt
    FROM JobRequests jr
    JOIN Customers c ON jr.CustomerID = c.CustomerID
    JOIN Users cu ON c.UserID = cu.UserID
    JOIN Labourers l ON jr.LabourerID = l.LabourerID
    JOIN Users lu ON l.UserID = lu.UserID
    WHERE jr.RequestID = @RequestID;
END;
GO

CREATE PROCEDURE sp_UpdateJobStatus
    @RequestID INT,
    @NewStatus NVARCHAR(20),
    @RejectionReason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE JobRequests
    SET Status = @NewStatus, RejectionReason = @RejectionReason, UpdatedAt = SYSUTCDATETIME()
    WHERE RequestID = @RequestID;
    
    -- If status completed, update CompletionRate for labourer
    IF @NewStatus = 'Completed' OR @NewStatus = 'Rejected'
    BEGIN
        DECLARE @lid INT = (SELECT LabourerID FROM JobRequests WHERE RequestID = @RequestID);
        IF @lid IS NOT NULL
        BEGIN
            DECLARE @total INT = (SELECT COUNT(*) FROM JobRequests WHERE LabourerID = @lid);
            DECLARE @completed INT = (SELECT COUNT(*) FROM JobRequests WHERE LabourerID = @lid AND Status = 'Completed');
            IF @total > 0
            BEGIN
                UPDATE Labourers 
                SET CompletionRate = CAST(@completed AS DECIMAL(5,2)) / CAST(@total AS DECIMAL(5,2)) * 100 
                WHERE LabourerID = @lid;
            END
        END
    END
END;
GO

CREATE PROCEDURE sp_GetJobsForCustomer
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT jr.RequestID, jr.CustomerID, jr.LabourerID, lu.FullName AS LabourerName,
           jr.JobDescription, jr.PreferredDate, jr.PreferredTime, jr.Location, jr.Latitude, jr.Longitude,
           jr.Status, jr.RejectionReason, jr.EstimatedHours, jr.CreatedAt, jr.UpdatedAt
    FROM JobRequests jr
    JOIN Labourers l ON jr.LabourerID = l.LabourerID
    JOIN Users lu ON l.UserID = lu.UserID
    WHERE jr.CustomerID = @CustomerID
    ORDER BY jr.CreatedAt DESC;
END;
GO

CREATE PROCEDURE sp_GetJobsForLabourer
    @LabourerID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT jr.RequestID, jr.CustomerID, cu.FullName AS CustomerName, jr.LabourerID,
           jr.JobDescription, jr.PreferredDate, jr.PreferredTime, jr.Location, jr.Latitude, jr.Longitude,
           jr.Status, jr.RejectionReason, jr.EstimatedHours, jr.CreatedAt, jr.UpdatedAt
    FROM JobRequests jr
    JOIN Customers c ON jr.CustomerID = c.CustomerID
    JOIN Users cu ON c.UserID = cu.UserID
    WHERE jr.LabourerID = @LabourerID
    ORDER BY jr.UpdatedAt DESC;
END;
GO

CREATE PROCEDURE sp_CheckLabourerAvailability
    @LabourerID INT,
    @Date DATE,
    @Time TIME
AS
BEGIN
    SET NOCOUNT ON;
    -- Returns 1 if available (no conflict), 0 if there is conflict
    DECLARE @ConflictCount INT;
    
    SELECT @ConflictCount = COUNT(*)
    FROM JobRequests
    WHERE LabourerID = @LabourerID
      AND PreferredDate = @Date
      AND Status IN ('Pending', 'Accepted', 'InProgress');
      
    SELECT CASE WHEN @ConflictCount > 0 THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS IsAvailable;
END;
GO

-- Reviews
CREATE PROCEDURE sp_CreateReview
    @RequestID INT,
    @ReviewerUserID INT,
    @TargetLabourerID INT,
    @Rating INT,
    @Comment NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Reviews (RequestID, TargetLabourerID, ReviewerUserID, StarRating, ReviewText)
    VALUES (@RequestID, @TargetLabourerID, @ReviewerUserID, @Rating, @Comment);
END;
GO

CREATE PROCEDURE sp_GetReviewsForLabourer
    @LabourerID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.ReviewID, r.RequestID, r.StarRating, r.ReviewText, r.CreatedAt, ru.FullName AS ReviewerName
    FROM Reviews r
    JOIN Users ru ON r.ReviewerUserID = ru.UserID
    WHERE r.TargetLabourerID = @LabourerID
    ORDER BY r.CreatedAt DESC;
END;
GO

CREATE PROCEDURE sp_HasReviewed
    @RequestID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Reviews WHERE RequestID = @RequestID AND ReviewerUserID = @UserID)
        SELECT CAST(1 AS BIT) AS HasReviewed;
    ELSE
        SELECT CAST(0 AS BIT) AS HasReviewed;
END;
GO

-- Disputes
CREATE PROCEDURE sp_RaiseDispute
    @RequestID INT,
    @RaisedByUserID INT,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO Disputes (RequestID, RaisedByUserID, Reason, Status)
        VALUES (@RequestID, @RaisedByUserID, @Reason, 'Open');
        
        DECLARE @did INT = SCOPE_IDENTITY();
        
        INSERT INTO DisputeChatSessions (DisputeID, IsActive)
        VALUES (@did, 1);
        
        COMMIT;
        SELECT @did AS DisputeID;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END;
GO

CREATE PROCEDURE sp_GetDisputeById
    @DisputeID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.DisputeID, d.RequestID, d.RaisedByUserID, ru.FullName AS RaisedByName, ru.Role AS RaisedByRole,
           d.Reason, d.Status, d.Resolution, d.CreatedAt, d.ResolvedAt,
           c.UserID AS CustomerUserID, cu.FullName AS CustomerName,
           l.UserID AS LabourerUserID, lu.FullName AS LabourerName,
           jr.JobDescription
    FROM Disputes d
    JOIN JobRequests jr ON d.RequestID = jr.RequestID
    JOIN Customers c ON jr.CustomerID = c.CustomerID
    JOIN Users cu ON c.UserID = cu.UserID
    JOIN Labourers l ON jr.LabourerID = l.LabourerID
    JOIN Users lu ON l.UserID = lu.UserID
    JOIN Users ru ON d.RaisedByUserID = ru.UserID
    WHERE d.DisputeID = @DisputeID;
END;
GO

CREATE PROCEDURE sp_GetOpenDisputes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.DisputeID, d.RequestID, d.RaisedByUserID, ru.FullName AS RaisedByName,
           d.Reason, d.Status, d.CreatedAt,
           cu.FullName AS CustomerName, lu.FullName AS LabourerName
    FROM Disputes d
    JOIN JobRequests jr ON d.RequestID = jr.RequestID
    JOIN Customers c ON jr.CustomerID = c.CustomerID
    JOIN Users cu ON c.UserID = cu.UserID
    JOIN Labourers l ON jr.LabourerID = l.LabourerID
    JOIN Users lu ON l.UserID = lu.UserID
    JOIN Users ru ON d.RaisedByUserID = ru.UserID
    WHERE d.Status = 'Open'
    ORDER BY d.CreatedAt DESC;
END;
GO

CREATE PROCEDURE sp_GetDisputesForUser
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.DisputeID, d.RequestID, d.RaisedByUserID, d.Reason, d.Status, d.CreatedAt,
           cu.FullName AS CustomerName, lu.FullName AS LabourerName
    FROM Disputes d
    JOIN JobRequests jr ON d.RequestID = jr.RequestID
    JOIN Customers c ON jr.CustomerID = c.CustomerID
    JOIN Users cu ON c.UserID = cu.UserID
    JOIN Labourers l ON jr.LabourerID = l.LabourerID
    JOIN Users lu ON l.UserID = lu.UserID
    WHERE c.UserID = @UserID OR l.UserID = @UserID
    ORDER BY d.CreatedAt DESC;
END;
GO

CREATE PROCEDURE sp_ResolveDispute
    @DisputeID INT,
    @ResolutionType NVARCHAR(30),
    @ResolutionText NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE Disputes
        SET Status = 'Resolved', Resolution = @ResolutionType + ': ' + @ResolutionText, ResolvedAt = SYSUTCDATETIME()
        WHERE DisputeID = @DisputeID;
        
        UPDATE DisputeChatSessions
        SET IsActive = 0, ClosedAt = SYSUTCDATETIME()
        WHERE DisputeID = @DisputeID;
        
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END;
GO

-- Chat SPs
CREATE PROCEDURE sp_AddChatMessage
    @DisputeID INT,
    @SenderUserID INT,
    @SenderRole NVARCHAR(20),
    @MessageText NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sid INT = (SELECT SessionID FROM DisputeChatSessions WHERE DisputeID = @DisputeID AND IsActive = 1);
    
    IF @sid IS NOT NULL
    BEGIN
        INSERT INTO DisputeChatMessages (SessionID, SenderUserID, SenderRole, MessageText)
        VALUES (@sid, @SenderUserID, @SenderRole, @MessageText);
        
        SELECT SCOPE_IDENTITY() AS MessageID, SYSUTCDATETIME() AS SentAt;
    END
END;
GO

CREATE PROCEDURE sp_GetChatMessages
    @DisputeID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.MessageID, m.SenderUserID, u.FullName AS SenderName, m.SenderRole, m.MessageText, m.SentAt
    FROM DisputeChatMessages m
    JOIN DisputeChatSessions s ON m.SessionID = s.SessionID
    JOIN Users u ON m.SenderUserID = u.UserID
    WHERE s.DisputeID = @DisputeID
    ORDER BY m.SentAt ASC;
END;
GO

CREATE PROCEDURE sp_GetActiveChatSessions
AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.SessionID, s.DisputeID, d.Reason, cu.FullName AS CustomerName, lu.FullName AS LabourerName
    FROM DisputeChatSessions s
    JOIN Disputes d ON s.DisputeID = d.DisputeID
    JOIN JobRequests jr ON d.RequestID = jr.RequestID
    JOIN Customers c ON jr.CustomerID = c.CustomerID
    JOIN Users cu ON c.UserID = cu.UserID
    JOIN Labourers l ON jr.LabourerID = l.LabourerID
    JOIN Users lu ON l.UserID = lu.UserID
    WHERE s.IsActive = 1;
END;
GO

-- Notifications
CREATE PROCEDURE sp_CreateNotification
    @UserID INT,
    @Message NVARCHAR(300),
    @NotificationType NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Notifications (UserID, Message, NotificationType, IsRead)
    VALUES (@UserID, @Message, @NotificationType, 0);
    
    SELECT SCOPE_IDENTITY() AS NotificationID;
END;
GO

CREATE PROCEDURE sp_GetUnreadNotifications
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT NotificationID, UserID, Message, NotificationType, IsRead, CreatedAt
    FROM Notifications
    WHERE UserID = @UserID AND IsRead = 0
    ORDER BY CreatedAt DESC;
END;
GO

CREATE PROCEDURE sp_MarkNotificationRead
    @NotificationID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Notifications
    SET IsRead = 1
    WHERE NotificationID = @NotificationID;
END;
GO

CREATE PROCEDURE sp_MarkAllNotificationsRead
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Notifications
    SET IsRead = 1
    WHERE UserID = @UserID;
END;
GO

CREATE PROCEDURE sp_GetNotificationCount
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS UnreadCount
    FROM Notifications
    WHERE UserID = @UserID AND IsRead = 0;
END;
GO

-- Analytics
CREATE PROCEDURE sp_GetPlatformKPIs
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        (SELECT COUNT(*) FROM Users WHERE IsActive=1) AS TotalUsers,
        (SELECT COUNT(*) FROM Labourers WHERE IsProfileLive=1) AS ActiveLabourers,
        (SELECT COUNT(*) FROM JobRequests WHERE Status='Completed' AND CAST(UpdatedAt AS DATE)=CAST(SYSUTCDATETIME() AS DATE)) AS JobsToday,
        (SELECT COUNT(*) FROM Users WHERE CreatedAt >= DATEADD(DAY,-7,SYSUTCDATETIME())) AS NewUsersThisWeek,
        COALESCE((SELECT AVG(CAST(StarRating AS FLOAT)) FROM Reviews), 0.0) AS AvgRating,
        COALESCE((SELECT CAST(COUNT(CASE WHEN Status='Completed' THEN 1 END) AS FLOAT) / NULLIF(COUNT(*),0) * 100 FROM JobRequests), 0.0) AS CompletionRate,
        
        -- vs last week deltas
        (SELECT COUNT(*) FROM Users WHERE CreatedAt >= DATEADD(DAY,-14,SYSUTCDATETIME()) AND CreatedAt < DATEADD(DAY,-7,SYSUTCDATETIME())) AS NewUsersLastWeek,
        (SELECT COUNT(*) FROM JobRequests WHERE Status='Completed' AND UpdatedAt >= DATEADD(DAY,-7,SYSUTCDATETIME())) AS JobsThisWeek,
        (SELECT COUNT(*) FROM JobRequests WHERE Status='Completed' AND UpdatedAt >= DATEADD(DAY,-14,SYSUTCDATETIME()) AND UpdatedAt < DATEADD(DAY,-7,SYSUTCDATETIME())) AS JobsLastWeek;
END;
GO

CREATE PROCEDURE sp_GetUserGrowthTrend
    @Days INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FORMAT(CreatedAt, 'yyyy-MM-dd') AS [Date], COUNT(*) AS NewUsers
    FROM Users
    WHERE CreatedAt >= DATEADD(DAY, -@Days, SYSUTCDATETIME())
    GROUP BY FORMAT(CreatedAt, 'yyyy-MM-dd')
    ORDER BY [Date] ASC;
END;
GO

CREATE PROCEDURE sp_GetDailyCompletedJobs
    @Days INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FORMAT(UpdatedAt, 'yyyy-MM-dd') AS [Date], COUNT(*) AS CompletedJobs
    FROM JobRequests
    WHERE Status = 'Completed' AND UpdatedAt >= DATEADD(DAY, -@Days, SYSUTCDATETIME())
    GROUP BY FORMAT(UpdatedAt, 'yyyy-MM-dd')
    ORDER BY [Date] ASC;
END;
GO

CREATE PROCEDURE sp_GetCategoryDistribution
AS
BEGIN
    SET NOCOUNT ON;
    SELECT sc.Name, COUNT(l.LabourerID) AS LabourerCount
    FROM SkillCategories sc
    LEFT JOIN Labourers l ON l.CategoryID = sc.CategoryID AND l.IsProfileLive = 1
    GROUP BY sc.CategoryID, sc.Name
    ORDER BY LabourerCount DESC;
END;
GO

CREATE PROCEDURE sp_GetTopRatedLabourers
    @TopN INT = 5
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@TopN) l.LabourerID, u.FullName, sc.Name AS CategoryName,
        l.ReputationScore, l.CompletionRate,
        (SELECT COUNT(*) FROM JobRequests jr WHERE jr.LabourerID = l.LabourerID AND jr.Status = 'Completed') AS CompletedJobs
    FROM Labourers l
    INNER JOIN Users u ON l.UserID = u.UserID
    INNER JOIN SkillCategories sc ON l.CategoryID = sc.CategoryID
    WHERE l.IsProfileLive = 1
    ORDER BY l.ReputationScore DESC;
END;
GO

CREATE PROCEDURE sp_GetUnmetDemand
AS
BEGIN
    SET NOCOUNT ON;
    -- Demand is calculated by grouping searches on category
    SELECT TOP 5 sc.Name AS CategoryName,
        COUNT(ds.SearchID) AS SearchCount,
        (SELECT COUNT(*) FROM Labourers l WHERE l.CategoryID = sc.CategoryID AND l.IsProfileLive = 1) AS LiveLabourerCount,
        COUNT(ds.SearchID) - (SELECT COUNT(*) FROM Labourers l WHERE l.CategoryID = sc.CategoryID AND l.IsProfileLive = 1) AS DemandGap
    FROM SkillCategories sc
    LEFT JOIN DemandSearchLog ds ON ds.CategoryID = sc.CategoryID
    GROUP BY sc.CategoryID, sc.Name
    ORDER BY DemandGap DESC;
END;
GO

CREATE PROCEDURE sp_GetLabourerEarningsTrend
    @LabourerID INT,
    @Days INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FORMAT(jr.UpdatedAt, 'yyyy-MM-dd') AS [Date],
        SUM(jr.EstimatedHours * l.HourlyRate) AS EstimatedEarnings
    FROM JobRequests jr
    INNER JOIN Labourers l ON l.LabourerID = jr.LabourerID
    WHERE jr.LabourerID = @LabourerID
      AND jr.Status = 'Completed'
      AND jr.UpdatedAt >= DATEADD(DAY, -@Days, SYSUTCDATETIME())
    GROUP BY FORMAT(jr.UpdatedAt, 'yyyy-MM-dd')
    ORDER BY [Date] ASC;
END;
GO

CREATE PROCEDURE sp_GetLabourerRatingTrend
    @LabourerID INT,
    @Days INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FORMAT(r.CreatedAt, 'yyyy-MM-dd') AS [Date], AVG(CAST(r.StarRating AS FLOAT)) AS AvgRating
    FROM Reviews r
    WHERE r.TargetLabourerID = @LabourerID
      AND r.CreatedAt >= DATEADD(DAY, -@Days, SYSUTCDATETIME())
    GROUP BY FORMAT(r.CreatedAt, 'yyyy-MM-dd')
    ORDER BY [Date] ASC;
END;
GO

CREATE PROCEDURE sp_GetLabourerJobFunnel
    @LabourerID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        COUNT(*) AS TotalRequests,
        COUNT(CASE WHEN Status IN ('Accepted','InProgress','Completed') THEN 1 END) AS Accepted,
        COUNT(CASE WHEN Status = 'Completed' THEN 1 END) AS Completed,
        COUNT(CASE WHEN Status = 'Rejected' THEN 1 END) AS Rejected
    FROM JobRequests WHERE LabourerID = @LabourerID;
END;
GO

CREATE PROCEDURE sp_RecalculateReputation
    @LabourerID INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @newScore DECIMAL(5,2);

    -- Recency-weighted rating recalculation
    SELECT @newScore = COALESCE(SUM(r.StarRating * (1.0 / (DATEDIFF(MONTH, r.CreatedAt, SYSUTCDATETIME()) + 1)))
                     / SUM(1.0 / (DATEDIFF(MONTH, r.CreatedAt, SYSUTCDATETIME()) + 1)), 0.0)
    FROM Reviews r WHERE r.TargetLabourerID = @LabourerID;

    UPDATE Labourers SET ReputationScore = @newScore WHERE LabourerID = @LabourerID;
END;
GO


-- ==========================================
-- 5. TRIGGERS
-- ==========================================

-- Trigger 1: Recalculate reputation after review insert
CREATE TRIGGER trg_Review_Insert ON Reviews AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @LabourerID INT = (SELECT TargetLabourerID FROM inserted);
    EXEC sp_RecalculateReputation @LabourerID;
END;
GO

-- Trigger 2: Log status changes into RequestStatusHistory
CREATE TRIGGER trg_JobStatus_Update ON JobRequests AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(Status)
    BEGIN
        INSERT INTO RequestStatusHistory(RequestID, OldStatus, NewStatus, ChangedAt)
        SELECT i.RequestID, d.Status, i.Status, SYSUTCDATETIME()
        FROM inserted i 
        JOIN deleted d ON i.RequestID=d.RequestID
        WHERE i.Status <> d.Status;
    END
END;
GO

-- Trigger 3: Create notifications on job request update
CREATE TRIGGER trg_JobStatus_Notify ON JobRequests AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(Status)
    BEGIN
        -- Notify Customer
        INSERT INTO Notifications (UserID, Message, NotificationType, IsRead, CreatedAt)
        SELECT cu.UserID, 'Your job request #' + CAST(i.RequestID AS NVARCHAR(10)) + ' status was updated to ' + i.Status + '.', 'StatusChange', 0, SYSUTCDATETIME()
        FROM inserted i
        JOIN Customers c ON i.CustomerID = c.CustomerID
        JOIN Users cu ON c.UserID = cu.UserID
        JOIN deleted d ON i.RequestID = d.RequestID
        WHERE i.Status <> d.Status;

        -- Notify Labourer
        INSERT INTO Notifications (UserID, Message, NotificationType, IsRead, CreatedAt)
        SELECT lu.UserID, 'Incoming job request #' + CAST(i.RequestID AS NVARCHAR(10)) + ' status was updated to ' + i.Status + '.', 'StatusChange', 0, SYSUTCDATETIME()
        FROM inserted i
        JOIN Labourers l ON i.LabourerID = l.LabourerID
        JOIN Users lu ON l.UserID = lu.UserID
        JOIN deleted d ON i.RequestID = d.RequestID
        WHERE i.Status <> d.Status;
    END
END;
GO

-- Trigger 4: Flag account after 3+ unresolved disputes
CREATE TRIGGER trg_Dispute_Flag ON Disputes AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @disputeID INT, @reqID INT, @raisedBy INT;
    SELECT @disputeID = DisputeID, @reqID = RequestID, @raisedBy = RaisedByUserID FROM inserted;
    
    DECLARE @customerUserID INT, @labourerUserID INT;
    SELECT @customerUserID = cu.UserID, @labourerUserID = lu.UserID
    FROM JobRequests jr
    JOIN Customers c ON jr.CustomerID = c.CustomerID
    JOIN Users cu ON c.UserID = cu.UserID
    JOIN Labourers l ON jr.LabourerID = l.LabourerID
    JOIN Users lu ON l.UserID = lu.UserID
    WHERE jr.RequestID = @reqID;
    
    DECLARE @targetUserID INT = CASE WHEN @raisedBy = @customerUserID THEN @labourerUserID ELSE @customerUserID END;
    
    -- Increment dispute strikes
    UPDATE Users SET DisputeStrikes = DisputeStrikes + 1 WHERE UserID = @targetUserID;
    
    -- If strikes >= 3, flag account
    IF (SELECT DisputeStrikes FROM Users WHERE UserID = @targetUserID) >= 3
    BEGIN
        UPDATE Users SET IsActive = 0, IsSuspended = 1 WHERE UserID = @targetUserID;
        
        -- Admin notification
        DECLARE @adminUID INT = (SELECT TOP 1 UserID FROM Users WHERE Role = 'Admin');
        IF @adminUID IS NOT NULL
        BEGIN
            INSERT INTO Notifications (UserID, Message, NotificationType, IsRead, CreatedAt)
            VALUES (@adminUID, 'User ' + (SELECT FullName FROM Users WHERE UserID = @targetUserID) + ' was suspended automatically due to 3+ dispute strikes.', 'AccountSuspended', 0, SYSUTCDATETIME());
        END
    END
END;
GO

-- Trigger 5: Activate profile when admin approves
CREATE TRIGGER trg_Labourer_Approve ON Labourers AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(VerificationStatus)
    BEGIN
        UPDATE Labourers 
        SET IsProfileLive = 1
        FROM Labourers l
        JOIN inserted i ON l.LabourerID = i.LabourerID
        WHERE i.VerificationStatus = 'Approved';
    END
END;
GO


-- ==========================================
-- 6. SEED DATA
-- ==========================================

-- Skill Categories
INSERT INTO SkillCategories (Name, Description, IsActive) VALUES
('Electrician', 'Wiring, installations, repairs', 1),
('Plumber', 'Pipes, drains, fixtures', 1),
('Carpenter', 'Furniture, doors, cabinetry', 1),
('Painter', 'Interior/exterior painting', 1),
('Mason', 'Brickwork, plastering, tiles', 1),
('AC Technician', 'AC, heating, ventilation', 1),
('Welder', 'Metal fabrication and welding', 1),
('Solar Panel Technician', 'Solar installation and maintenance', 1);

-- Admin Seed User (Password: Admin@123)
-- Precomputed BCrypt hash: $2a$11$Rbj4WQmYcgHGG36eFdkAyurtufzhSdbgI1cT7uouid3ZQneGleJZ.
INSERT INTO Users (FullName, Email, PasswordHash, Role, Phone, IsActive, IsSuspended) VALUES
('Platform Admin', 'admin@skillbridge.pk', '$2a$11$Rbj4WQmYcgHGG36eFdkAyurtufzhSdbgI1cT7uouid3ZQneGleJZ.', 'Admin', '+923001234567', 1, 0);

DECLARE @adminUID INT = SCOPE_IDENTITY();
INSERT INTO Admins (UserID) VALUES (@adminUID);

-- Seed Customers
INSERT INTO Users (FullName, Email, PasswordHash, Role, Phone, IsActive, IsSuspended) VALUES
('Imran Khan', 'customer1@skillbridge.pk', '$2a$11$Rbj4WQmYcgHGG36eFdkAyurtufzhSdbgI1cT7uouid3ZQneGleJZ.', 'Customer', '+923007654321', 1, 0),
('Ayesha Bibi', 'customer2@skillbridge.pk', '$2a$11$Rbj4WQmYcgHGG36eFdkAyurtufzhSdbgI1cT7uouid3ZQneGleJZ.', 'Customer', '+923009988776', 1, 0);

INSERT INTO Customers (UserID, Address, Latitude, Longitude) VALUES
((SELECT UserID FROM Users WHERE Email='customer1@skillbridge.pk'), 'Gulberg III, Lahore, Pakistan', 31.5204, 74.3587),
((SELECT UserID FROM Users WHERE Email='customer2@skillbridge.pk'), 'DHA Phase 5, Lahore, Pakistan', 31.4697, 74.4087);

-- Seed Labourers (Lahore Area Offsets)
-- Seed 1: Approved Electrician (Live)
INSERT INTO Users (FullName, Email, PasswordHash, Role, Phone, IsActive, IsSuspended) VALUES
('Muhammad Ali', 'labourer1@skillbridge.pk', '$2a$11$Rbj4WQmYcgHGG36eFdkAyurtufzhSdbgI1cT7uouid3ZQneGleJZ.', 'Labourer', '+923011234567', 1, 0);

INSERT INTO Labourers (UserID, CategoryID, ExperienceYears, HourlyRate, Bio, AvailabilitySchedule, Address, Latitude, Longitude, ReputationScore, CompletionRate, VerificationStatus, VerificationNote, IsProfileLive) VALUES
((SELECT UserID FROM Users WHERE Email='labourer1@skillbridge.pk'), 
 (SELECT CategoryID FROM SkillCategories WHERE Name='Electrician'), 
 8, 450.00, 'Experienced commercial and residential electrician. Specializing in fault diagnostic and lighting installation.', 
 '{"Monday":["09:00-17:00"],"Tuesday":["09:00-17:00"],"Wednesday":["09:00-17:00"],"Thursday":["09:00-17:00"],"Friday":["09:00-17:00"],"Saturday":["09:00-13:00"]}',
 'Gulberg Town, Lahore, Pakistan', 31.5220, 74.3520, 4.80, 95.00, 'Approved', 'Initial seed approval', 1);

-- Seed 2: Approved Plumber (Live)
INSERT INTO Users (FullName, Email, PasswordHash, Role, Phone, IsActive, IsSuspended) VALUES
('Sajid Mahmood', 'labourer2@skillbridge.pk', '$2a$11$Rbj4WQmYcgHGG36eFdkAyurtufzhSdbgI1cT7uouid3ZQneGleJZ.', 'Labourer', '+923021234567', 1, 0);

INSERT INTO Labourers (UserID, CategoryID, ExperienceYears, HourlyRate, Bio, AvailabilitySchedule, Address, Latitude, Longitude, ReputationScore, CompletionRate, VerificationStatus, VerificationNote, IsProfileLive) VALUES
((SELECT UserID FROM Users WHERE Email='labourer2@skillbridge.pk'), 
 (SELECT CategoryID FROM SkillCategories WHERE Name='Plumber'), 
 5, 350.00, 'Professional plumbing services. Expert in leak repairs, geyser service, and pipeline installations.', 
 '{"Monday":["08:00-18:00"],"Tuesday":["08:00-18:00"],"Wednesday":["08:00-18:00"],"Thursday":["08:00-18:00"],"Friday":["08:00-18:00"]}',
 'Model Town, Lahore, Pakistan', 31.4800, 74.3200, 4.50, 90.00, 'Approved', 'Initial seed approval', 1);

-- Seed 3: Pending Carpenter (Not Live)
INSERT INTO Users (FullName, Email, PasswordHash, Role, Phone, IsActive, IsSuspended) VALUES
('Asif Bhatti', 'labourer3@skillbridge.pk', '$2a$11$Rbj4WQmYcgHGG36eFdkAyurtufzhSdbgI1cT7uouid3ZQneGleJZ.', 'Labourer', '+923031234567', 1, 0);

INSERT INTO Labourers (UserID, CategoryID, ExperienceYears, HourlyRate, Bio, AvailabilitySchedule, Address, Latitude, Longitude, ReputationScore, CompletionRate, VerificationStatus, VerificationNote, IsProfileLive) VALUES
((SELECT UserID FROM Users WHERE Email='labourer3@skillbridge.pk'), 
 (SELECT CategoryID FROM SkillCategories WHERE Name='Carpenter'), 
 12, 600.00, 'Master woodworker with 12 years of experience building custom cabinets, doors, and furniture repairs.', 
 '{"Monday":["09:00-18:00"],"Tuesday":["09:00-18:00"],"Wednesday":["09:00-18:00"],"Thursday":["09:00-18:00"],"Friday":["09:00-18:00"],"Saturday":["09:00-18:00"]}',
 'Johar Town, Lahore, Pakistan', 31.4700, 74.2700, 0.00, 0.00, 'Pending', NULL, 0);

-- Seed 4: Approved Painter (Live)
INSERT INTO Users (FullName, Email, PasswordHash, Role, Phone, IsActive, IsSuspended) VALUES
('Zafar Iqbal', 'labourer4@skillbridge.pk', '$2a$11$Rbj4WQmYcgHGG36eFdkAyurtufzhSdbgI1cT7uouid3ZQneGleJZ.', 'Labourer', '+923041234567', 1, 0);

INSERT INTO Labourers (UserID, CategoryID, ExperienceYears, HourlyRate, Bio, AvailabilitySchedule, Address, Latitude, Longitude, ReputationScore, CompletionRate, VerificationStatus, VerificationNote, IsProfileLive) VALUES
((SELECT UserID FROM Users WHERE Email='labourer4@skillbridge.pk'), 
 (SELECT CategoryID FROM SkillCategories WHERE Name='Painter'), 
 3, 300.00, 'Expert interior and exterior painting services. Wall putty, color mixing, and wallpaper installation.', 
 '{"Monday":["09:00-17:00"],"Tuesday":["09:00-17:00"],"Wednesday":["09:00-17:00"],"Thursday":["09:00-17:00"],"Friday":["09:00-17:00"]}',
 'Faisal Town, Lahore, Pakistan', 31.4900, 74.3000, 4.20, 85.00, 'Approved', 'Initial seed approval', 1);

-- Seed Demand Search Logs for Unmet Demand View
INSERT INTO DemandSearchLog (CategoryID, CustomerID, SearchedAt) VALUES
((SELECT CategoryID FROM SkillCategories WHERE Name='AC Technician'), NULL, SYSUTCDATETIME()),
((SELECT CategoryID FROM SkillCategories WHERE Name='AC Technician'), NULL, SYSUTCDATETIME()),
((SELECT CategoryID FROM SkillCategories WHERE Name='AC Technician'), NULL, SYSUTCDATETIME()),
((SELECT CategoryID FROM SkillCategories WHERE Name='Solar Panel Technician'), NULL, SYSUTCDATETIME()),
((SELECT CategoryID FROM SkillCategories WHERE Name='Solar Panel Technician'), NULL, SYSUTCDATETIME());
GO

PRINT 'SkillBridgeDB database setup completed successfully.';
