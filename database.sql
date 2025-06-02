-- 1. BloodBank trước khi chạy nhớ tạo database Blood_Donation_System
CREATE TABLE BloodBank (
  BloodTypeId INT PRIMARY KEY,
  BloodTypeName NVARCHAR(50),
  Unit INT
);

-- 2. Hospital
CREATE TABLE Hospital (
  HospitalId INT PRIMARY KEY,
  Name NVARCHAR(100),
  Address NVARCHAR(200)
);

-- 3. User
CREATE TABLE [User] (
  Username NVARCHAR(50) PRIMARY KEY,
  Password NVARCHAR(100),
  Email NVARCHAR(100),
  Role NVARCHAR(50),
  FullName NVARCHAR(100),
  DateOfBirth DATE,
  Gender NVARCHAR(10),
  Phone NVARCHAR(20),
  Address NVARCHAR(200),
  ProfileStatus NVARCHAR(50),
  BloodTypeId INT FOREIGN KEY REFERENCES BloodBank(BloodTypeId)
);

-- 4. Emergency
CREATE TABLE Emergency (
  EmergencyId INT IDENTITY(1,1) PRIMARY KEY,
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  BloodTypeId INT FOREIGN KEY REFERENCES BloodBank(BloodTypeId),
  EmergencyDate DATE,
  EmergencyStatus NVARCHAR(50),
  EmergencyNote NVARCHAR(MAX),
  RequiredUnits INT,
  HospitalId INT FOREIGN KEY REFERENCES Hospital(HospitalId)
);

-- 5. Notification
CREATE TABLE Notification (
  EmergencyId INT PRIMARY KEY FOREIGN KEY REFERENCES Emergency(EmergencyId),
  NotificationStatus NVARCHAR(50),
  NotificationTitle NVARCHAR(100),
  NotificationContent NVARCHAR(MAX),
  BloodTypeId INT FOREIGN KEY REFERENCES BloodBank(BloodTypeId)
);

-- 6. DonationAppointment
CREATE TABLE DonationAppointment (
  AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
  AppointmentDate DATE,
  AppointmentTime TIME,
  AppointmentTitle NVARCHAR(100),
  AppointmentContent NVARCHAR(MAX)
);

-- 7. DonationHistory
CREATE TABLE DonationHistory (
  DonationHistoryId INT IDENTITY(1,1) PRIMARY KEY,
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  BloodTypeId INT FOREIGN KEY REFERENCES BloodBank(BloodTypeId),
  DonationDate DATE,
  DonationStatus NVARCHAR(50),
  DonationUnit INT
);

-- 8. Certificate
CREATE TABLE Certificate (
  DonationHistoryId INT PRIMARY KEY FOREIGN KEY REFERENCES DonationHistory(DonationHistoryId),
  CertificateCode NVARCHAR(50),
  IssueDate DATE
);

-- 9. Report
CREATE TABLE Report (
  ReportId INT IDENTITY(1,1) PRIMARY KEY,
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  ReportDate DATE,
  ReportType NVARCHAR(50),
  ReportContent NVARCHAR(MAX)
);

-- 10. Blog
CREATE TABLE Blog (
  BlogId INT IDENTITY(1,1) PRIMARY KEY,
  BlogTitle NVARCHAR(100),
  BlogContent NVARCHAR(MAX),
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username)
);

-- 11. BloodMove
CREATE TABLE BloodMove (
  BloodMoveId INT IDENTITY(1,1) PRIMARY KEY,
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  BloodTypeId INT FOREIGN KEY REFERENCES BloodBank(BloodTypeId),
  Unit INT,
  HospitalId INT FOREIGN KEY REFERENCES Hospital(HospitalId),
  DateMove DATE
);

-- 12. AppointmentHistory
CREATE TABLE AppointmentHistory (
  AppointmentHistoryId INT IDENTITY(1,1) PRIMARY KEY,
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  AppointmentId INT FOREIGN KEY REFERENCES DonationAppointment(AppointmentId),
  AppointmentDate DATETIME,
  Status NVARCHAR(50)
);

-- 13. NotificationRecipient
CREATE TABLE NotificationRecipient (
  NotificationRecipientId INT IDENTITY(1,1) PRIMARY KEY,
  EmergencyId INT FOREIGN KEY REFERENCES Emergency(EmergencyId),
  NotificationEmergencyId INT FOREIGN KEY REFERENCES Notification(EmergencyId),
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  ResponseStatus NVARCHAR(50), -- 'Chưa phản hồi', 'Chấp nhận', 'Từ chối'
  ResponseDate DATETIME
);
