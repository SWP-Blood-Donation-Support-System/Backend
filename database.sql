Create Database Blood_Donation_System;
USE Blood_Donation_System;




-- 2. Hospital
CREATE TABLE Hospital (
  HospitalId INT IDENTITY(1,1) PRIMARY KEY,
  HospitalName NVARCHAR(100),
  HospitalAddress NVARCHAR(200),
  HospitalImage NVARCHAR(MAX),
  HospitalPhone NVARCHAR(20)
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
  BloodType NVARCHAR(5)
);

-- 4. Emergency
CREATE TABLE Emergency (
  EmergencyId INT IDENTITY(1,1) PRIMARY KEY,
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  EmergencyDate DATE,
  bloodType nvarchar(5),
  EmergencyStatus NVARCHAR(50),
  EmergencyNote NVARCHAR(MAX),
  RequiredUnits INT,
  HospitalId INT FOREIGN KEY REFERENCES Hospital(HospitalId)
);

-- 5. Notification
CREATE TABLE Notification (
  NotificationId int IDENTITY(1,1) PRIMARY KEY ,
  EmergencyId INT FOREIGN KEY REFERENCES Emergency(EmergencyId),
  NotificationStatus NVARCHAR(50),
  NotificationTitle NVARCHAR(100),
  NotificationContent NVARCHAR(MAX),
  NotificationDate DATE
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
  BloodType nvarchar,
  DonationDate DATE,
  DonationStatus NVARCHAR(MAX),
  DonationUnit INT
);

-- 8. Certificate
CREATE TABLE Certificate (
  DonationHistoryId INT PRIMARY KEY FOREIGN KEY REFERENCES DonationHistory(DonationHistoryId),
  CertificateCode NVARCHAR(50),
  IssueDate DATE
);

-- 1. BloodBank trước khi chạy nhớ tạo database Blood_Donation_System
CREATE TABLE BloodBank (
  BloodTypeId INT IDENTITY(1,1) PRIMARY KEY,
  BloodTypeName NVARCHAR(50),
  Unit INT,
  DonationHistoryId int FOREIGN KEY REFERENCES DonationHistory(DonationHistoryId),
  ExpiryDate date,
  [Status] nvarchar (MAX)
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
  BloodTypeId INT FOREIGN KEY REFERENCES BloodBank(BloodTypeId),
  Unit INT,
  HospitalId INT FOREIGN KEY REFERENCES Hospital(HospitalId),
  DateMove DATE,
  [Note] NVARCHAR(MAX)
);

-- 12. AppointmentHistory
CREATE TABLE AppointmentHistory (
  AppointmentHistoryId INT IDENTITY(1,1) PRIMARY KEY,
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  AppointmentId INT FOREIGN KEY REFERENCES AppointmentList(AppointmentId),
  AppointmentDate DATETIME,
  AppointmentStatus NVARCHAR(50)
);

-- 13. NotificationRecipient
CREATE TABLE NotificationRecipient (
  NotificationRecipientId INT IDENTITY(1,1) PRIMARY KEY,
  NotifivationID int FOREIGN KEY REFERENCES Notification(NotificationID),  
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  ResponseStatus NVARCHAR(50), -- 'Chưa phản hồi', 'Chấp nhận', 'Từ chối'
  ResponseDate DATETIME
);

INSERT INTO BloodBank (BloodTypeName, Unit, DonationHistoryId, ExpiryDate, [Status])
VALUES 
  ('A+', 5, 1, '2025-07-01', 'Đã kiểm tra'),
  ('O-', 3, 2, '2025-06-25', 'Chưa kiểm tra'),
  ('B+', 7, 3, '2025-08-10', 'Đang sử dụng'),
  ('AB-', 2, 4, '2025-07-15', 'Đã kiểm tra'),
  ('A-', 4, 5, '2025-06-30', 'Hết hạn'),
  ('O+', 6, 6, '2025-09-01', 'Chưa kiểm tra'),
  ('B-', 3, 7, '2025-07-20', 'Đã kiểm tra'),
  ('AB+', 5, 8, '2025-08-05', 'Đã sử dụng'),
  ('A+', 8, 9, '2025-07-10', 'Chưa kiểm tra'),
  ('O-', 2, 10, '2025-06-28', 'Hết hạn');


INSERT INTO Hospital (HospitalName, HospitalAddress, HospitalImage, HospitalPhone)
VALUES
(N'Bệnh viện Chợ Rẫy', N'201B Nguyễn Chí Thanh, Quận 5, TP.HCM', N'https://example.com/images/choray.jpg', '02838554137'),
(N'Bệnh viện Bạch Mai', N'78 Giải Phóng, Đống Đa, Hà Nội', N'https://example.com/images/bachmai.jpg', '02438693731'),
(N'Bệnh viện Trung ương Huế', N'16 Lê Lợi, TP. Huế', N'https://example.com/images/hue.jpg', '02343822231'),
(N'Bệnh viện Đại học Y Dược', N'215 Hồng Bàng, Quận 5, TP.HCM', N'https://example.com/images/ydhcm.jpg', '02839525353'),
(N'Bệnh viện 108', N'1 Trần Hưng Đạo, Hai Bà Trưng, Hà Nội', N'https://example.com/images/108.jpg', '02462784108'),
(N'Bệnh viện FV', N'6 Nguyễn Lương Bằng, Phú Mỹ Hưng, Quận 7, TP.HCM', N'https://example.com/images/fv.jpg', '02854113333'),
(N'Bệnh viện Hữu nghị Việt Đức', N'40 Tràng Thi, Hoàn Kiếm, Hà Nội', N'https://example.com/images/vietduc.jpg', '02438253531'),
(N'Bệnh viện Nhi đồng 1', N'341 Sư Vạn Hạnh, Quận 10, TP.HCM', N'https://example.com/images/nhidong1.jpg', '02839271119'),
(N'Bệnh viện Từ Dũ', N'284 Cống Quỳnh, Quận 1, TP.HCM', N'https://example.com/images/tudu.jpg', '02854042525'),
(N'Bệnh viện Phụ sản Hà Nội', N'929 La Thành, Ba Đình, Hà Nội', N'https://example.com/images/phusanhanoi.jpg', '02438343223');

INSERT INTO [Blood_Donation_System].[dbo].[AppointmentList]
    ([AppointmentDate], [AppointmentTime], [AppointmentTitle], [AppointmentContent])
VALUES
    ('2023-06-15', '09:00:00', 'Hiến máu định kỳ', 'Đợt hiến máu định kỳ quý II năm 2023 tại bệnh viện A'),
    ('2023-06-20', '13:30:00', 'Hiến máu nhân đạo', 'Chương trình hiến máu nhân đạo hỗ trợ bệnh nhân ung thư'),
    ('2023-07-05', '08:00:00', 'Ngày hội hiến máu', 'Ngày hội hiến máu "Giọt hồng trao đời" tại trường Đại học B'),
    ('2023-07-12', '10:00:00', 'Hiến máu cấp cứu', 'Kêu gọi hiến máu khẩn cấp cho bệnh nhân tai nạn giao thông'),
    ('2023-08-01', '14:00:00', 'Hiến máu hè', 'Chiến dịch hiến máu hè "Một giọt máu - Vạn niềm vui"'),
    ('2023-08-15', '09:30:00', 'Hiến máu định kỳ', 'Đợt hiến máu định kỳ quý III năm 2023 tại bệnh viện C'),
    ('2023-09-10', '08:30:00', 'Hiến máu thiện nguyện', 'Chương trình hiến máu "Trao máu - Trao sự sống"'),
    ('2023-09-25', '15:00:00', 'Hiến máu đột xuất', 'Kêu gọi hiến máu cho ca mổ tim khẩn cấp'),
    ('2023-10-05', '10:30:00', 'Ngày hội hiến máu', 'Ngày hội hiến máu "Sẻ giọt máu đào - Cứu người nguy nan"'),
    ('2023-10-20', '13:00:00', 'Hiến máu nhân đạo', 'Chương trình hiến máu hỗ trợ trẻ em bệnh viện Nhi');

INSERT INTO [User] (
  Username, Password, Email, Role, FullName,
  DateOfBirth, Gender, Phone, Address, ProfileStatus, BloodType
) VALUES
(N'user1', N'pass123', N'user1@example.com', N'Staff', N'Nguyễn Văn A',
 '1995-05-10', N'Nam', N'0912345678', N'TP. Hồ Chí Minh', N'Đang hoạt động', 1),

(N'user2', N'pass456', N'user2@example.com', N'User', N'Trần Thị B',
 '1998-07-20', N'Nữ', N'0987654321', N'Hà Nội', N'Đang hoạt động', 4),

(N'admin1', N'admin123', N'admin@example.com', N'Admin', N'Quản trị viên',
 '1990-01-01', N'Nam', N'0909090909', N'Đà Nẵng', N'Đang hoạt động', 7);


