Create Database Blood_Donation_System;
USE Blood_Donation_System;

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
  Address NVARCHAR(200),
  HospitalImage NVARCHAR(MAX)
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

-- 6. AppointmentList
CREATE TABLE AppointmentList (
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
  BlogImage NVARCHAR(MAX),
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
  AppointmentId INT FOREIGN KEY REFERENCES AppointmentListnt(AppointmentId),
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
INSERT INTO BloodBank (BloodTypeId, BloodTypeName, Unit) VALUES
(1, 'A+', 10),
(2, 'A-', 5),
(3, 'B+', 8),
(4, 'B-', 4),
(5, 'AB+', 6),
(6, 'AB-', 2),
(7, 'O+', 12),
(8, 'O-', 3);

INSERT INTO Hospital (HospitalId, Name, Address) VALUES
(1, N'Bệnh viện Chợ Rẫy', N'201B Nguyễn Chí Thanh, Phường 12, Quận 5, TP. Hồ Chí Minh'),
(2, N'Bệnh viện Bạch Mai', N'78 Giải Phóng, Phường Phương Mai, Quận Đống Đa, Hà Nội'),
(3, N'Bệnh viện Trung ương Huế', N'16 Lê Lợi, Phường Vĩnh Ninh, TP. Huế, Thừa Thiên Huế'),
(4, N'Bệnh viện 108', N'1 Trần Hưng Đạo, Phường Bạch Đằng, Quận Hai Bà Trưng, Hà Nội'),
(5, N'Bệnh viện Đại học Y Dược TP.HCM', N'215 Hồng Bàng, Phường 11, Quận 5, TP. Hồ Chí Minh'),
(6, N'Bệnh viện Nhi Trung ương', N'18/879 La Thành, Phường Láng Thượng, Quận Đống Đa, Hà Nội'),
(7, N'Bệnh viện Hữu nghị Việt Đức', N'40 Tràng Thi, Phường Hàng Bông, Quận Hoàn Kiếm, Hà Nội'),
(8, N'Bệnh viện K - Cơ sở Tân Triều', N'30 Cầu Bươu, Tân Triều, Thanh Trì, Hà Nội');


INSERT INTO [User] (
  Username, Password, Email, Role, FullName,
  DateOfBirth, Gender, Phone, Address, ProfileStatus, BloodTypeId
) VALUES
(N'user1', N'pass123', N'user1@example.com', N'donor', N'Nguyễn Văn A',
 '1995-05-10', N'Nam', N'0912345678', N'TP. Hồ Chí Minh', N'Đang hoạt động', 1),

(N'user2', N'pass456', N'user2@example.com', N'donor', N'Trần Thị B',
 '1998-07-20', N'Nữ', N'0987654321', N'Hà Nội', N'Đang hoạt động', 4),

(N'admin1', N'admin123', N'admin@example.com', N'admin', N'Quản trị viên',
 '1990-01-01', N'Nam', N'0909090909', N'Đà Nẵng', N'Đang hoạt động', 7);


