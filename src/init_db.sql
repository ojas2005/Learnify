
-- Learnify Unified Database Schema for MySQL

-- Identity Service
CREATE TABLE IF NOT EXISTS Identity_Accounts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DisplayName VARCHAR(100) NOT NULL,
    EmailAddress VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(500) NOT NULL,
    Role INT NOT NULL,
    ProfilePictureUrl VARCHAR(500),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    RegisteredOn DATETIME NOT NULL,
    LastSeenAt DATETIME
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Identity_Credentials (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AccountId INT NOT NULL,
    Provider VARCHAR(50) NOT NULL,
    ExternalId VARCHAR(255) NOT NULL,
    FOREIGN KEY (AccountId) REFERENCES Identity_Accounts(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Courses Service
CREATE TABLE IF NOT EXISTS Courses_Catalog (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Synopsis TEXT,
    Topic VARCHAR(100),
    Difficulty INT NOT NULL,
    Language VARCHAR(50),
    ListPrice DECIMAL(10,2) NOT NULL,
    AuthorId INT NOT NULL,
    CoverImageUrl VARCHAR(500),
    IsPublished BOOLEAN NOT NULL DEFAULT FALSE,
    IsApprovedByAdmin BOOLEAN,
    TotalRuntimeMinutes INT NOT NULL DEFAULT 0,
    TotalRegistrations INT NOT NULL DEFAULT 0,
    CreatedOn DATETIME NOT NULL,
    LastModifiedOn DATETIME
) ENGINE=InnoDB;

-- Curriculum Service
CREATE TABLE IF NOT EXISTS Curriculum_Lessons (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CourseId INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Body TEXT,
    Format INT NOT NULL,
    MediaUrl VARCHAR(500),
    SequencePosition INT NOT NULL,
    DurationMinutes INT NOT NULL,
    IsPublished BOOLEAN NOT NULL DEFAULT FALSE,
    IsPreviewable BOOLEAN NOT NULL DEFAULT FALSE,
    AddedOn DATETIME NOT NULL
) ENGINE=InnoDB;

-- Registration Service
CREATE TABLE IF NOT EXISTS Registration_Enrollments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    LearnerId INT NOT NULL,
    CourseId INT NOT NULL,
    EnrolledOn DATETIME NOT NULL,
    PricePaid DECIMAL(10,2) NOT NULL,
    PaymentReference VARCHAR(100),
    Status INT NOT NULL DEFAULT 0
) ENGINE=InnoDB;

-- Exams Service
CREATE TABLE IF NOT EXISTS Exams_Quizzes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CourseId INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    MaxAttempts INT NOT NULL DEFAULT 0,
    PassingScore INT NOT NULL DEFAULT 70
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Exams_Attempts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    QuizId INT NOT NULL,
    LearnerId INT NOT NULL,
    Score INT NOT NULL,
    IsPassed BOOLEAN NOT NULL,
    AttemptedOn DATETIME NOT NULL,
    FOREIGN KEY (QuizId) REFERENCES Exams_Quizzes(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Tracking Service
CREATE TABLE IF NOT EXISTS Tracking_Progress (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    LearnerId INT NOT NULL,
    LessonId INT NOT NULL,
    WatchDurationMinutes INT NOT NULL,
    IsCompleted BOOLEAN NOT NULL DEFAULT FALSE,
    LastAccessedOn DATETIME NOT NULL
) ENGINE=InnoDB;

-- Reviews Service
CREATE TABLE IF NOT EXISTS Reviews_Comments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CourseId INT NOT NULL,
    AuthorId INT NOT NULL,
    StarRating INT NOT NULL,
    ReviewText TEXT,
    IsApproved BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedOn DATETIME NOT NULL
) ENGINE=InnoDB;

-- Analytics Service
CREATE TABLE IF NOT EXISTS Analytics_AuditLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Action VARCHAR(100) NOT NULL,
    EntityName VARCHAR(100) NOT NULL,
    EntityId VARCHAR(100),
    BeforeValue TEXT,
    AfterValue TEXT,
    ActorId INT NOT NULL,
    Timestamp DATETIME NOT NULL
) ENGINE=InnoDB;
