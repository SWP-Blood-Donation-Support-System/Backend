-- Lưu ý: Trước khi chạy đoạn code này hãy xóa tất bảng database Blood_Donation_System nếu đã tồn tại nếu ko Id sẽ tăng sai
-- Lưu ý: ko chạy 1 mạch
-- Chạy theo thứ tự tạo database -> tạo bảng -> add dữ liệu theo thứ tự 1 -> 2 -> 3 -> 4 -> 5
Create Database Blood_Donation_System;
USE Blood_Donation_System;


-- 1. Hospital
CREATE TABLE Hospital (
  HospitalId INT IDENTITY(1,1) PRIMARY KEY,
  HospitalName NVARCHAR(100),
  HospitalAddress NVARCHAR(200),
  HospitalImage NVARCHAR(MAX),
  HospitalPhone NVARCHAR(20)
);

-- 2. User
CREATE TABLE [User] (
  Username NVARCHAR(50) PRIMARY KEY,
  Password NVARCHAR(100),
  Email NVARCHAR(100),
  [Role] NVARCHAR(50),
  FullName NVARCHAR(100),
  DateOfBirth DATE,
  Gender NVARCHAR(10),
  Phone NVARCHAR(20),
  [Address] NVARCHAR(200),
  ProfileStatus NVARCHAR(50),
  BloodType NVARCHAR(5)
);

-- 3. Emergency
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

-- 4. Notification
CREATE TABLE Notification (
  NotificationId int IDENTITY(1,1) PRIMARY KEY ,
  EmergencyId INT FOREIGN KEY REFERENCES Emergency(EmergencyId),
  NotificationTitle NVARCHAR(100),
  NotificationContent NVARCHAR(MAX),
  NotificationDate DATE
);

-- 5. AppointmentList
CREATE TABLE AppointmentList (
  AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
  AppointmentDate DATE,
  AppointmentTime TIME,
  AppointmentTitle NVARCHAR(100),
  AppointmentContent NVARCHAR(MAX)
);

-- 6. UserActivityHistory
CREATE TABLE UserActivityHistory (
  HistoryId INT IDENTITY(1,1) PRIMARY KEY,
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  ActivityType NVARCHAR(100) NOT NULL,
  ActivityDate DATETIME NOT NULL,
  Location NVARCHAR(255),
  BloodType NVARCHAR(10),
  DonationUnit INT,
  AppointmentTitle NVARCHAR(255),
  Status NVARCHAR(50),
  AppointmentId INT FOREIGN KEY REFERENCES AppointmentList(AppointmentId)
);

-- 7. Certificate
CREATE TABLE [Certificate] (
  HistoryId INT PRIMARY KEY FOREIGN KEY REFERENCES UserActivityHistory(HistoryId),
  CertificateCode NVARCHAR(50),
  IssueDate DATE
);

-- 8. BloodBank trước khi chạy nhớ tạo database Blood_Donation_System
CREATE TABLE BloodBank (
  BloodType nvarchar (5) PRIMARY KEY,
  BloodVolumeTotal int,
  BloodBankStatus nvarchar (100)
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

-- 11. BloodDetail
CREATE TABLE BloodDetail (
  BloodDetailId INT IDENTITY(1,1) PRIMARY KEY,
  BloodType NVARCHAR(5) FOREIGN KEY REFERENCES BloodBank(BloodType),
  Volume INT,
  DonationHistoryId INT FOREIGN KEY REFERENCES UserActivityHistory(HistoryId),
  HospitalId INT FOREIGN KEY REFERENCES Hospital(HospitalId),
  BloodDetailDate date,
  BloodDetailStatus nvarchar (100),
  [Note] NVARCHAR(MAX)
);

-- 12. NotificationRecipient
CREATE TABLE NotificationRecipient (
  NotificationRecipientId INT IDENTITY(1,1) PRIMARY KEY,
  NotificationID int FOREIGN KEY REFERENCES Notification(NotificationID),  
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  ResponseStatus NVARCHAR(50), -- 'Chưa phản hồi', 'Chấp nhận', 'Từ chối'
  ResponseDate DATETIME
);


--1
INSERT INTO [User] (Username, Password, Email, Role, FullName,DateOfBirth, Gender, Phone, Address, ProfileStatus, BloodType) VALUES
(N'admin1', N'admin1', N'admin@example.com', N'Admin', N'Quản trị viên',
 '1990-01-01', N'Nam', N'0909090909', N'Đà Nẵng', N'Active', NULL),
 
(N'staff1', N'staff1', N'user1@example.com', N'Staff', N'Nguyễn Văn A',
 '1995-05-10', N'Nam', N'0912345678', N'TP. Hồ Chí Minh', N'Active', NULL),

(N'string', N'string', 'Nuser4@email.com', N'Staff', N'Phạm Thị D', 
  N'1992-11-25', N'Nữ', '0978123456', N'321 Đường Lý Tự Trọng, Q1, TP.HCM', N'Active', NULL),

(N'user1', N'pass1', N'user2@example.com', N'User', N'Trần Thị B',
 '1998-07-20', N'Nữ', N'0987654321', N'Hà Nội', N'Active', 'B-'),

(N'user2', N'pass2', N'user1@email.com', N'User', N'Nguyễn Văn A', 
 N'1990-05-15', N'Nam', N'0912345678', N'123 Đường Lê Lợi, Q1, TP.HCM', N'Active', N'A+'),

(N'user3', N'pass3', N'user2@email.com', N'User', N'Trần Thị B', 
  N'1995-08-20', N'Nữ', N'0987654321', N'456 Đường Nguyễn Huệ, Q1, TP.HCM', N'Active', N'B+'),

(N'user4', N'pass4', N'user3@email.com', N'User', N'Lê Văn C', 
  N'1985-03-10', 'Nam', N'0909123456', N'789 Đường CMT8, Q3, TP.HCM', N'Active', N'O+'),

(N'user5', N'pass5', N'user5@email.com', N'User', N'Hoàng Thị E', 
 '1993-04-12', N'Nữ', N'0911223344', N'101 Đường Hai Bà Trưng, Q1, TP.HCM', N'Active', N'AB+'),

(N'user6', N'pass6', N'user6@email.com', N'User', N'Vũ Văn F', 
 '1988-09-05', N'Nam', N'0988776655', N'202 Đường Lê Duẩn, Q1, TP.HCM', N'Active', N'AB-'),

(N'user7', N'pass7', N'user7@email.com', N'User', N'Đặng Thị G', 
 '1997-12-30', N'Nữ', N'0901122334', N'303 Đường Pasteur, Q3, TP.HCM', N'Active', N'O-'),

(N'user8', N'pass8', N'user8@email.com', N'User', N'Bùi Văn H', 
 '1991-07-18', N'Nam', N'0912345000', N'404 Đường Nguyễn Đình Chiểu, Q1, TP.HCM', N'Active', N'A-'),

(N'user9', N'pass9', N'user9@email.com', N'User', N'Lý Thị I', 
 '1994-02-22', N'Nữ', N'0987650001', N'505 Đường Trần Hưng Đạo, Q5, TP.HCM', N'Active', N'B-'),

(N'user10', N'pass10', N'user10@email.com', N'User', N'Phan Văn K', 
 '1989-06-15', N'Nam', N'0909111222', N'606 Đường Cách Mạng Tháng 8, Q10, TP.HCM', N'Active', N'A+'),

(N'user11', N'pass11', N'user11@email.com', N'User', N'Mai Thị L', 
 '1996-10-08', N'Nữ', N'0912121212', N'707 Đường 3 Tháng 2, Q10, TP.HCM', N'Active', N'B+'),

(N'user12', N'pass12', N'user12@email.com', N'User', N'Trịnh Văn M', 
 '1990-11-11', N'Nam', N'0988989898', N'808 Đường Lý Thường Kiệt, Q10, TP.HCM', N'Active', N'O+');

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
(N'Bệnh viện Phụ sản Hà Nội', N'929 La Thành, Ba Đình, Hà Nội', N'https://example.com/images/phusanhanoi.jpg', '02438343223'),
(N'Bệnh viện Nhi đồng 2', N'14 Lý Tự Trọng, Quận 1, TP.HCM', N'https://example.com/images/nhidong2.jpg', '02838295725'),
(N'Bệnh viện Ung bướu TP.HCM', N'3 Nơ Trang Long, Bình Thạnh, TP.HCM', N'https://example.com/images/ungbuouhcm.jpg', '02838412939'),
(N'Bệnh viện Tai Mũi Họng TW', N'78 Giải Phóng, Đống Đa, Hà Nội', N'https://example.com/images/taimuihong.jpg', '02438691337'),
(N'Bệnh viện Mắt TP.HCM', N'280 Điện Biên Phủ, Quận 3, TP.HCM', N'https://example.com/images/mathcm.jpg', '02839327546'),
(N'Bệnh viện Việt Pháp', N'1 Phương Mai, Đống Đa, Hà Nội', N'https://example.com/images/vietphap.jpg', '02435771111'),
(N'Bệnh viện Đa khoa Quốc tế Vinmec', N'458 Minh Khai, Hai Bà Trưng, Hà Nội', N'https://example.com/images/vinmec.jpg', '02439743456'),
(N'Bệnh viện Đại học Y Hà Nội', N'1 Tôn Thất Tùng, Đống Đa, Hà Nội', N'https://example.com/images/ydhn.jpg', '02435743291'),
(N'Bệnh viện Thống Nhất', N'1 Lý Thường Kiệt, Quận Tân Bình, TP.HCM', N'https://example.com/images/thongnhat.jpg', '02838695735'),
(N'Bệnh viện Nhân dân 115', N'527 Sư Vạn Hạnh, Quận 10, TP.HCM', N'https://example.com/images/115.jpg', '02838654127'),
(N'Bệnh viện Phổi Trung ương', N'463 Hoàng Hoa Thám, Ba Đình, Hà Nội', N'https://example.com/images/phoitw.jpg', '02438233044');

--2
INSERT INTO AppointmentList(AppointmentDate, AppointmentTime, AppointmentTitle, AppointmentContent)
VALUES
('2025-01-15', '08:30:00', N'Hiến máu đầu năm', N'Chương trình hiến máu "Máu đỏ đầu xuân" tại bệnh viện Chợ Rẫy'),
('2025-02-20', '14:00:00', N'Hiến máu nhân ngày Thầy thuốc', N'Sự kiện hiến máu chào mừng ngày Thầy thuốc Việt Nam 27/2'),
('2025-03-08', '09:00:00', N'Hiến máu ngày Quốc tế Phụ nữ', N'Chương trình hiến máu "Phụ nữ hiến máu cứu người"'),
('2025-04-19', '07:30:00', N'Ngày hội hiến máu Thanh niên', N'Hiến máu "Tuổi trẻ sẻ chia" do Đoàn thanh niên tổ chức'),
('2025-04-30', '10:00:00', N'Hiến máu chào mừng 30/4', N'Sự kiện hiến máu kỷ niệm ngày Giải phóng miền Nam'),
('2025-05-01', '08:00:00', N'Hiến máu ngày Quốc tế Lao động', N'Chương trình "Một giọt máu - Triệu tấm lòng"'),
('2025-05-19', '13:30:00', N'Hiến máu sinh nhật Bác', N'Hiến máu nhân kỷ niệm ngày sinh Chủ tịch Hồ Chí Minh'),
('2025-05-27', '15:00:00', N'Hiến máu khẩn cấp', N'Kêu gọi hiến máu cho bệnh nhân bị tai nạn giao thông nghiêm trọng'),
('2025-06-01', '09:00:00', N'Hiến máu Ngày Quốc tế Thiếu nhi', N'Chương trình "Hiến máu vì nụ cười trẻ thơ"'),
('2025-06-10', '10:30:00', N'Hiến máu hè', N'Chiến dịch hiến máu hè tại bệnh viện Nhi đồng 1'),
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
(N'user5', '2024-01-10', N'Góp ý', N'Đề nghị tăng số lượng điểm hiến máu di động trong thành phố'),
(N'user6', '2024-02-15', N'Khiếu nại', N'Thủ tục đăng ký hiến máu trực tuyến bị lỗi không hoàn thành'),
(N'staff1', '2024-03-20', N'Báo cáo', N'Tổng kết chương trình hiến máu Xuân hồng 2024'),
(N'user8', '2024-04-25', N'Góp ý', N'Cần bổ sung đồ ăn nhẹ sau khi hiến máu'),
(N'user1', '2025-01-05', N'Khiếu nại', N'Nhân viên lấy máu thao tác không đúng quy trình'),
(N'staff2', '2025-01-15', N'Báo cáo', N'Báo cáo sự cố thiết bị y tế tại điểm hiến máu'),
(N'user4', '2025-02-02', N'Góp ý', N'Đề nghị cung cấp giấy chứng nhận hiến máu điện tử'),
(N'user2', '2025-02-18', N'Khiếu nại', N'Chờ đợi quá lâu trước khi được hiến máu'),
(N'staff1', '2025-03-10', N'Báo cáo', N'Báo cáo kết quả chiến dịch hiến máu 8/3'),
(N'user4', '2025-03-28', N'Góp ý', N'Cần có thêm nhân viên hướng dẫn tại các điểm hiến máu'),
(N'user5', '2025-04-05', N'Khiếu nại', N'Thông tin cá nhân bị sai trên giấy chứng nhận'),
(N'staff2', '2025-04-15', N'Báo cáo', N'Báo cáo lượng máu tiếp nhận quý I/2025'),
(N'user7', '2025-06-01', N'Góp ý', N'Đề nghị cải thiện chất lượng áo phông tặng cho người hiến máu'),
(N'user8', '2025-06-05', N'Khiếu nại', N'Điểm hiến máu không đủ chỗ ngồi chờ'),
(N'staff1', '2025-06-10', N'Báo cáo', N'Báo cáo sơ bộ về ngày hội hiến máu 1/6'),
(N'user2', '2025-06-20', N'Góp ý', N'Đề xuất tổ chức hiến máu tại các trường đại học thường xuyên hơn'),
(N'user1', '2025-06-25', N'Khiếu nại', N'Không nhận được thông báo kết quả xét nghiệm sau hiến máu'),
(N'staff2', '2025-06-30', N'Báo cáo', N'Báo cáo tổng kết hoạt động hiến máu 6 tháng đầu năm'),
(N'user2', '2025-03-15', N'Khiếu nại', N'Thái độ nhân viên chưa tốt khi đi hiến máu'),
(N'user3', '2025-04-20', N'Góp ý', N'Đề nghị cải thiện cơ sở vật chất tại điểm hiến máu'),
(N'staff1', '2025-07-05', N'Báo cáo', N'Báo cáo hoạt động hiến máu quý 2 năm 2023');

--2
INSERT INTO Blog (BlogTitle, BlogContent, BlogImage, Username)
VALUES 
(N'Lợi ích của việc hiến máu', N'Hiến máu không chỉ cứu người mà còn có lợi cho sức khỏe của bạn...', N'https://example.com/hienmau1.jpg', N'user1'),
(N'Chuẩn bị gì trước khi hiến máu?', N'Để có một lần hiến máu thành công, bạn cần chuẩn bị...', N'https://example.com/hienmau2.jpg', N'user6'),
(N'Những điều cần biết sau khi hiến máu', N'Sau khi hiến máu, bạn cần lưu ý những điều sau để đảm bảo sức khỏe...', N'https://example.com/hienmau3.jpg', N'user1');

--3
INSERT INTO UserActivityHistory (Username, ActivityType, ActivityDate, [Location], BloodType, DonationUnit, AppointmentTitle, [Status], AppointmentId)
VALUES 
(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

--3
INSERT INTO Emergency (Username, EmergencyDate, bloodType, EmergencyStatus, EmergencyNote, RequiredUnits, HospitalId)
VALUES 
(N'user5', '2025-06-11', N'B-', N'Đã xét duyệt', N'Cần 5 đơn vị nhóm máu B- tại Bệnh viện 108', 5, 5);

--4
INSERT INTO Notification (EmergencyId, NotificationTitle, NotificationContent, NotificationDate)
VALUES 
(1, N'Yêu cầu hiến máu khẩn cấp - Bệnh viện 108', N'Cần 5 đơn vị nhóm máu B- tại Bệnh viện 108', '2025-06-11');

--4
INSERT INTO BloodBank (BloodType, BloodVolumeTotal, BloodBankStatus)
VALUES 
(NULL, NULL, NULL);

--4
INSERT INTO Certificate (DonationHistoryId, CertificateCode, IssueDate)
VALUES 
(1, N'CERT-2023-0001', '2025-01-15'),
(2, N'CERT-2023-0002', '2025-02-20'),
(3, N'CERT-2023-0003', '2025-05-10'),
(4, N'CERT-2023-0004', '2025-06-25'),
(5, N'CERT-2023-0005', '2025-08-12');

--5
INSERT INTO BloodDetail (BloodType, Volume, DonationHistoryId, HospitalId, BloodDetailDate, BloodDetailStatus, Note)
VALUES 
(NULL, NULL, NULL, NULL, NULL, NULL, NULL);

--5
INSERT INTO NotificationRecipient (NotificationID, Username, ResponseStatus, ResponseDate)
VALUES 
(1, N'user1', N'Chấp nhận', '2025-10-01 10:30:00'),
(1, N'user9', N'Chưa phản hồi', NULL);
