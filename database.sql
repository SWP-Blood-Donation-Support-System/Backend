-- Lưu ý: Trước khi chạy đoạn code này hãy xóa tất bảng database Blood_Donation_System nếu đã tồn tại nếu ko Id sẽ tăng sai
-- Lưu ý: ko chạy 1 mạch
-- Chạy theo thứ tự tạo database -> tạo bảng -> add dữ liệu theo thứ tự 1 -> 2 -> 3 -> 4 -> 5
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
  BloodType nvarchar(5),
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
  NotificationID int FOREIGN KEY REFERENCES Notification(NotificationID),  
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  ResponseStatus NVARCHAR(50), -- 'Chưa phản hồi', 'Chấp nhận', 'Từ chối'
  ResponseDate DATETIME
);


--1
INSERT INTO [User] (Username, Password, Email, Role, FullName,DateOfBirth, Gender, Phone, Address, ProfileStatus, BloodType) VALUES
(N'user1', N'pass1', N'user1@example.com', N'Staff', N'Nguyễn Văn A',
 '1995-05-10', N'Nam', N'0912345678', N'TP. Hồ Chí Minh', N'Active', 'A-'),

(N'user2', N'pass2', N'user2@example.com', N'User', N'Trần Thị B',
 '1998-07-20', N'Nữ', N'0987654321', N'Hà Nội', N'Active', 'B-'),

(N'admin1', N'admin1', N'admin@example.com', N'Admin', N'Quản trị viên',
 '1990-01-01', N'Nam', N'0909090909', N'Đà Nẵng', N'Active', 'AB-'),

(N'user3', N'pass3', N'user1@email.com', N'User', N'Nguyễn Văn A', 
 N'1990-05-15', N'Nam', N'0912345678', N'123 Đường Lê Lợi, Q1, TP.HCM', N'Active', N'A+'),

(N'user4', N'pass4', N'user2@email.com', N'User', N'Trần Thị B', 
  N'1995-08-20', N'Nữ', N'0987654321', N'456 Đường Nguyễn Huệ, Q1, TP.HCM', N'Active', N'B+'),

(N'user5', N'pass5', N'user3@email.com', N'User', N'Lê Văn C', 
  N'1985-03-10', 'Nam', N'0909123456', N'789 Đường CMT8, Q3, TP.HCM', N'Active', N'O+'),

(N'user6', N'pass6', 'Nuser4@email.com', N'Staff', N'Phạm Thị D', 
  N'1992-11-25', N'Nữ', '0978123456', N'321 Đường Lý Tự Trọng, Q1, TP.HCM', N'Inactive', N'AB+');

--2
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

--2
INSERT INTO AppointmentList(AppointmentDate, AppointmentTime, AppointmentTitle, AppointmentContent)
VALUES
    ('2025-06-15', '09:00:00', N'Hiến máu định kỳ', N'Đợt hiến máu định kỳ quý II năm 2023 tại bệnh viện A'),
    ('2025-06-20', '13:30:00', N'Hiến máu nhân đạo', N'Chương trình hiến máu nhân đạo hỗ trợ bệnh nhân ung thư'),
    ('2025-07-05', '08:00:00', N'Ngày hội hiến máu', N'Ngày hội hiến máu "Giọt hồng trao đời" tại trường Đại học B'),
    ('2025-07-12', '10:00:00', N'Hiến máu cấp cứu', N'Kêu gọi hiến máu khẩn cấp cho bệnh nhân tai nạn giao thông'),
    ('2025-08-01', '14:00:00', N'Hiến máu hè', N'Chiến dịch hiến máu hè "Một giọt máu - Vạn niềm vui"'),
    ('2025-08-15', '09:30:00', N'Hiến máu định kỳ', N'Đợt hiến máu định kỳ quý III năm 2023 tại bệnh viện C'),
    ('2025-09-10', '08:30:00', N'Hiến máu thiện nguyện', N'Chương trình hiến máu "Trao máu - Trao sự sống"'),
    ('2025-09-25', '15:00:00', N'Hiến máu đột xuất', N'Kêu gọi hiến máu cho ca mổ tim khẩn cấp'),
    ('2025-10-05', '10:30:00', N'Ngày hội hiến máu', N'Ngày hội hiến máu "Sẻ giọt máu đào - Cứu người nguy nan"'),
    ('2025-10-20', '13:00:00', N'Hiến máu nhân đạo', N'Chương trình hiến máu hỗ trợ trẻ em bệnh viện Nhi');

--2
INSERT INTO Report (Username, ReportDate, ReportType, ReportContent)
VALUES 
(N'user2', '2025-03-15', N'Khiếu nại', N'Thái độ nhân viên chưa tốt khi đi hiến máu'),
(N'user3', '2025-04-20', N'Góp ý', N'Đề nghị cải thiện cơ sở vật chất tại điểm hiến máu'),
(N'user4', '2025-07-05', N'Báo cáo', N'Báo cáo hoạt động hiến máu quý 2 năm 2023');

--2
INSERT INTO Blog (BlogTitle, BlogContent, BlogImage, Username)
VALUES 
(N'Lợi ích của việc hiến máu', N'Hiến máu không chỉ cứu người mà còn có lợi cho sức khỏe của bạn...', N'https://example.com/hienmau1.jpg', N'user1'),
(N'Chuẩn bị gì trước khi hiến máu?', N'Để có một lần hiến máu thành công, bạn cần chuẩn bị...', N'https://example.com/hienmau2.jpg', N'user6'),
(N'Những điều cần biết sau khi hiến máu', N'Sau khi hiến máu, bạn cần lưu ý những điều sau để đảm bảo sức khỏe...', N'https://example.com/hienmau3.jpg', N'user1');

--3
INSERT INTO AppointmentHistory (Username, AppointmentId, AppointmentDate, AppointmentStatus)
VALUES 
(N'user2', 1, '2023-10-02 08:30:00', N'Đã hoàn thành'),
(N'user3', 2, '2023-10-06 09:00:00', N'Đã hoàn thành'),
(N'user4', 3, '2023-10-11 10:30:00', N'Đã hoàn thành'),
(N'user5', 4, '2023-11-15 14:00:00', N'Đã đặt lịch'),
(N'user4', 5, '2023-11-20 15:30:00', N'Đã hủy'),
(N'user2', 6, '2023-12-05 08:00:00', N'Đã đặt lịch'),
(N'user3', 7, '2023-12-10 10:00:00', N'Đã đặt lịch'),
(N'user5', 8, '2024-01-08 13:30:00', N'Đã đặt lịch'),
(N'user4', 9, '2024-01-15 09:00:00', N'Đã đặt lịch'),
(N'user2', 10, '2024-02-01 11:00:00', N'Đã đặt lịch');

--3
INSERT INTO DonationHistory (Username, BloodType, DonationDate, DonationStatus, DonationUnit)
VALUES 
(N'user2', N'A+', '2023-01-15', N'Hoàn thành', 1),
(N'user3', N'B+', '2023-02-20', N'Hoàn thành', 1),
(N'user4', N'A+', '2023-05-10', N'Hoàn thành', 1),
(N'user5', N'A-', '2023-06-25', N'Hoàn thành', 1),
(N'user2', N'B+', '2023-08-12', N'Hoàn thành', 1);

--3
INSERT INTO Emergency (Username, EmergencyDate, bloodType, EmergencyStatus, EmergencyNote, RequiredUnits, HospitalId)
VALUES 
(N'user2', '2025-10-01', N'A+', N'Chưa xét duyệt', N'Cần gấp máu cho ca phẫu thuật tim', 5, 1),
(N'user3', '2025-10-05', N'O+', N'Đã xét duyệt', N'Cần máu cho bệnh nhân tai nạn giao thông', 3, 2),
(N'user4', '2025-10-10', N'B+', N'Đã đáp ứng', N'Cần gấp máu cho sản phụ', 4, 3);

--4
INSERT INTO Notification (EmergencyId, NotificationStatus, NotificationTitle, NotificationContent, NotificationDate)
VALUES 
(1, N'Đã gửi', N'Yêu cầu hiến máu khẩn cấp', N'Cần gấp 5 đơn vị máu A+ cho bệnh nhân phẫu thuật tim tại Bệnh viện Hữu nghị Việt Đức', '2025-10-01'),
(2, N'Đã gửi', N'Yêu cầu hiến máu', N'Cần 3 đơn vị máu O+ cho bệnh nhân tai nạn giao thông tại Bệnh viện Bạch Mai', '2025-10-05'),
(3, N'Đã gửi', N'Yêu cầu hiến máu khẩn cấp', N'Cần gấp 4 đơn vị máu B+ cho sản phụ tại Bệnh viện Phụ sản Trung ương', '2025-10-10');

--4
INSERT INTO BloodBank (BloodTypeName, Unit, DonationHistoryId, ExpiryDate, [Status])
VALUES 
  ('A+', 5, 1, '2025-07-01', N'Đã kiểm tra'),
  ('O-', 3, 2, '2025-06-25', N'Chưa kiểm tra'),
  ('B+', 7, 3, '2025-08-10', N'Đang sử dụng'),
  ('AB-', 2, 4, '2025-07-15', N'Đã kiểm tra'),
  ('A-', 4, 5, '2025-06-30', N'Hết hạn'),
  ('O+', 6, 1, '2025-09-01', N'Chưa kiểm tra'),
  ('B-', 3, 2, '2025-07-20', N'Đã kiểm tra'),
  ('AB+', 5, 3, '2025-08-05', N'Đã sử dụng'),
  ('A+', 8, 4, '2025-07-10', N'Chưa kiểm tra'),
  ('O-', 2, 5, '2025-06-28', N'Hết hạn');

--4
INSERT INTO Certificate (DonationHistoryId, CertificateCode, IssueDate)
VALUES 
(1, N'CERT-2023-0001', '2025-01-15'),
(2, N'CERT-2023-0002', '2025-02-20'),
(3, N'CERT-2023-0003', '2025-05-10'),
(4, N'CERT-2023-0004', '2025-06-25'),
(5, N'CERT-2023-0005', '2025-08-12');

--5
INSERT INTO BloodMove (BloodTypeId, Unit, HospitalId, DateMove, Note)
VALUES 
(1, 5, 1, '2025-09-01', N'Chuyển máu từ ngân hàng máu đến Bệnh viện Chợ Rẫy'),
(2, 3, 2, '2025-09-05', N'Chuyển máu từ ngân hàng máu đến Bệnh viện Bạch Mai'),
(3, 7, 3, '2025-09-10', N'Chuyển máu từ ngân hàng máu đến Bệnh viện Trung ương Huế');

--5
INSERT INTO NotificationRecipient (NotificationID, Username, ResponseStatus, ResponseDate)
VALUES 
(2, N'user2', N'Chấp nhận', '2025-10-01 10:30:00'),
(1, N'user3', N'Từ chối', '2025-10-01 11:15:00'),
(3, N'user4', N'Chưa phản hồi', NULL),
(1, N'user4', N'Chấp nhận', '2025-10-05 09:20:00'),
(3, N'user3', N'Chấp nhận', '2025-10-10 14:45:00');
