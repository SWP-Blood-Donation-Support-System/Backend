-- Lưu ý: Trước khi chạy đoạn code này hãy xóa tất bảng database Blood_Donation_System nếu đã tồn tại nếu ko Id sẽ tăng sai
-- Lưu ý: ko chạy 1 mạch
-- Chạy theo thứ tự tạo database -> tạo bảng -> add dữ liệu theo thứ tự 1 -> 2 -> 3 -> 4 -> 5
-- Tạo CSDL


CREATE DATABASE Blood_Donation_System;
GO

USE Blood_Donation_System;
GO

----------------------------------------------------------
-- 1. Bảng Hospital: Thông tin cơ sở y tế tổ chức hiến máu
----------------------------------------------------------
CREATE TABLE Hospital (
  HospitalId INT IDENTITY(1,1) PRIMARY KEY,
  HospitalName NVARCHAR(100),
  HospitalAddress NVARCHAR(200),
  HospitalImage NVARCHAR(MAX),
  HospitalPhone NVARCHAR(20)
);

----------------------------------------------------------
-- 2. Bảng User: Quản lý người dùng hệ thống (người hiến máu, admin)
----------------------------------------------------------
CREATE TABLE [User] (
  Username NVARCHAR(50) PRIMARY KEY,
  Password NVARCHAR(100),
  Email NVARCHAR(100),
  [Role] NVARCHAR(50), -- Ví dụ: Donor, Admin
  FullName NVARCHAR(100),
  DateOfBirth DATE,
  Gender NVARCHAR(10),
  Phone NVARCHAR(20),
  [Address] NVARCHAR(200),
  ProfileStatus NVARCHAR(50), -- Trạng thái hồ sơ
  BloodType NVARCHAR(5)
);

----------------------------------------------------------
-- 3. Bảng Emergency: Ghi lại các yêu cầu cần máu khẩn cấp
----------------------------------------------------------
CREATE TABLE Emergency (
  EmergencyId INT IDENTITY(1,1) PRIMARY KEY,
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  EmergencyDate DATE,
  BloodType NVARCHAR(5),
  EmergencyStatus NVARCHAR(50),
  EmergencyNote NVARCHAR(MAX),
  RequiredUnits INT,
  EmergencyMedical NVARCHAR(MAX),
  EmergencyImage NVARCHAR(MAX),
  EndDate Date,
  HospitalId INT FOREIGN KEY REFERENCES Hospital(HospitalId)
);

----------------------------------------------------------
-- 4. Bảng Notification: Thông báo liên quan đến các ca khẩn cấp
----------------------------------------------------------
CREATE TABLE Notification (
  NotificationId INT IDENTITY(1,1) PRIMARY KEY,
  EmergencyId INT FOREIGN KEY REFERENCES Emergency(EmergencyId),
  NotificationStatus NVARCHAR(50),
  NotificationTitle NVARCHAR(100),
  NotificationContent NVARCHAR(MAX),
  NotificationDate DATE
);

----------------------------------------------------------
-- 5. Bảng Events: Quản lý các đợt tổ chức hiến máu
----------------------------------------------------------
CREATE TABLE Events (
  EventId INT IDENTITY(1,1) PRIMARY KEY,
  EventDate DATE,
  EventTime TIME,
  EventTitle NVARCHAR(100),
  EventContent NVARCHAR(MAX),
  Location NVARCHAR(255),
  MaxParticipants INT -- Số người đăng ký tối đa
);

----------------------------------------------------------
-- 6. Bảng AppointmentRecord: 
-- Ghi nhận các đăng ký lịch hẹn và kết quả hiến máu của người dùng
----------------------------------------------------------
CREATE TABLE AppointmentRecord (
  AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  EventId INT FOREIGN KEY REFERENCES Events(EventId),
  RegistrationDate DATETIME,
  Status NVARCHAR(50), -- VD: Đã hiến, Chờ xác nhận, Huỷ
  BloodType NVARCHAR(5),
  DonationUnit INT, -- Số đơn vị máu đã hiến (nếu có)
  StaffNote NVARCHAR(MAX); -- Ghi chú của nhân viên y tế về ca hiến máu
);

----------------------------------------------------------
-- 7. Bảng Certificate: Giấy chứng nhận cho người đã hiến máu
----------------------------------------------------------
CREATE TABLE Certificate (
  AppointmentId INT PRIMARY KEY FOREIGN KEY REFERENCES AppointmentRecord(AppointmentId),
  CertificateCode NVARCHAR(50),
  IssueDate DATE
);

----------------------------------------------------------
-- 8. Bảng BloodBank: Tổng kho máu hiện có, theo nhóm máu
----------------------------------------------------------
CREATE TABLE BloodBank (
  BloodType NVARCHAR(5) PRIMARY KEY,
  BloodVolumeTotal INT,
  BloodBankStatus NVARCHAR(100)
);

----------------------------------------------------------
-- 9. Bảng Report: Người dùng gửi phản ánh, báo cáo
----------------------------------------------------------
CREATE TABLE Report (
  ReportId INT IDENTITY(1,1) PRIMARY KEY,
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  ReportDate DATE,
  ReportType NVARCHAR(50), -- VD: Vấn đề lịch hẹn, sự kiện, tài khoản
  ReportContent NVARCHAR(MAX)
);

----------------------------------------------------------
-- 10. Bảng Blog: Bài viết chia sẻ về hiến máu
----------------------------------------------------------
CREATE TABLE Blog (
  BlogId INT IDENTITY(1,1) PRIMARY KEY,
  BlogTitle NVARCHAR(100),
  BlogContent NVARCHAR(MAX),
  BlogImage NVARCHAR(MAX),
  BlogStatus NVARCHAR(MAX),
  BlogDetail NVARCHAR(MAX),
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username)
);

----------------------------------------------------------
-- 11. Bảng BloodDetail: 
-- Ghi lại đơn vị máu hiến cụ thể, gắn với sự kiện và bệnh viện
----------------------------------------------------------
CREATE TABLE BloodDetail (
  BloodDetailId INT IDENTITY(1,1) PRIMARY KEY,
  BloodType NVARCHAR(5) FOREIGN KEY REFERENCES BloodBank(BloodType),
  Volume INT,
  AppointmentId INT FOREIGN KEY REFERENCES AppointmentRecord(AppointmentId),
  HospitalId INT FOREIGN KEY REFERENCES Hospital(HospitalId),
  BloodDetailDate DATE,
  BloodDetailStatus NVARCHAR(100), -- VD: Còn hạn, Hết hạn
  Note NVARCHAR(MAX)
);

----------------------------------------------------------
-- 12. Bảng NotificationRecipient: 
-- Ghi nhận người nhận thông báo và trạng thái phản hồi
----------------------------------------------------------
CREATE TABLE NotificationRecipient (
  NotificationRecipientId INT IDENTITY(1,1) PRIMARY KEY,
  NotificationId INT FOREIGN KEY REFERENCES Notification(NotificationId),
  Username NVARCHAR(50) FOREIGN KEY REFERENCES [User](Username),
  ResponseStatus NVARCHAR(50), -- VD: Chưa phản hồi, Chấp nhận, Từ chối
  ResponseDate DATETIME,
  ResponseGo DATE,
  ResponseTime TIME
);



--1
INSERT INTO [User] (Username, Password, Email, Role, FullName,DateOfBirth, Gender, Phone, Address, ProfileStatus, BloodType) VALUES
(N'admin1', N'admin1', N'admin@example.com', N'Admin', N'Quản trị viên',
 '1990-01-01', N'Nam', N'0909090909', N'Đà Nẵng', NULL, NULL),
 
(N'staff1', N'staff1', N'user1@example.com', N'Staff', N'Nguyễn Văn A',
 '1995-05-10', N'Nam', N'0912345678', N'TP.Hồ Chí Minh', NULL, NULL),

(N'string', N'string', 'Nuser4@email.com', N'Staff', N'Phạm Thị D', 
  N'1992-11-25', N'Nữ', '0978123456', N'321 Đường Lý Tự Trọng, Q1, TP.Hồ Chí Minh', NULL, NULL),

(N'user1', N'pass1', N'user2@example.com', N'User', N'Trần Thị B',
 '1998-07-20', N'Nữ', N'0987654321', N'Hà Nội', N'Active', 'B-'),

(N'user2', N'pass2', N'user1@email.com', N'User', N'Nguyễn Văn A', 
 N'1990-05-15', N'Nam', N'0912345678', N'103 Đ. 30 Tháng 4, Phường Thống Nhất, Vũng Tàu, Bà Rịa - Vũng Tàu',N'Active', N'A+'),

(N'user3', N'pass3', N'user2@email.com', N'User', N'Trần Thị B', 
  N'1995-08-20', N'Nữ', N'0987654321', N'456 Đường Nguyễn Huệ, Q1, TP.Hồ Chí Minh', N'Active', N'B+'),

(N'user4', N'pass4', N'user3@email.com', N'User', N'Lê Văn C', 
  N'1985-03-10', 'Nam', N'0909123456', N'789 Đường CMT8, Q3, TP.Hồ Chí Minh', N'Active', N'O+'),

(N'user5', N'pass5', N'user5@email.com', N'User', N'Hoàng Thị E', 
 '1993-04-12', N'Nữ', N'0911223344', N'101 Đường Hai Bà Trưng, Q1, TP.Hồ Chí Minh', N'Active', N'AB+'),

(N'user6', N'pass6', N'user6@email.com', N'User', N'Vũ Văn F', 
 '1988-09-05', N'Nam', N'0988776655', N'202 Đường Lê Duẩn, Q1, TP.Hồ Chí Minh', N'Active', N'AB-'),

(N'user7', N'pass7', N'user7@email.com', N'User', N'Đặng Thị G', 
 '1997-12-30', N'Nữ', N'0901122334', N'303 Đường Pasteur, Q3, TP.Hồ Chí Minh', N'Active', N'O-'),

(N'user8', N'pass8', N'user8@email.com', N'User', N'Bùi Văn H', 
 '1991-07-18', N'Nam', N'0912345000', N'404 Đường Nguyễn Đình Chiểu, Q1, TP.Hồ Chí Minh', N'Active', N'A-'),

(N'user9', N'pass9', N'user9@email.com', N'User', N'Lý Thị I', 
 '1994-02-22', N'Nữ', N'0987650001', N'505 Đường Trần Hưng Đạo, Q5, TP.Hồ Chí Minh', N'Active', N'B-'),

(N'user10', N'pass10', N'user10@email.com', N'User', N'Phan Văn K', 
 '1989-06-15', N'Nam', N'0909111222', N'606 Đường Cách Mạng Tháng 8, Q10, TP.Hồ Chí Minh', N'Active', N'A+'),

(N'user11', N'pass11', N'user11@email.com', N'User', N'Mai Thị L', 
 '1996-10-08', N'Nữ', N'0912121212', N'707 Đường 3 Tháng 2, Q10, TP.Hồ Chí Minh', N'Active', N'B+'),

(N'user12', N'pass12', N'user12@email.com', N'User', N'Trịnh Văn M', 
 '1990-11-11', N'Nam', N'0988989898', N'808 Đường Lý Thường Kiệt, Q10, TP.Hồ Chí Minh', N'Active', N'O+');

--2
INSERT INTO Hospital (HospitalName, HospitalAddress, HospitalImage, HospitalPhone)
VALUES
(N'Bệnh viện Chợ Rẫy', N'201B Nguyễn Chí Thanh, Quận 5, TP.Hồ Chí Minh', N'https://example.com/images/choray.jpg', '02838554137'),
(N'Bệnh viện Bạch Mai', N'78 Giải Phóng, Đống Đa, Hà Nội', N'https://example.com/images/bachmai.jpg', '02438693731'),
(N'Bệnh viện Trung ương Huế', N'16 Lê Lợi, TP.Huế', N'https://example.com/images/hue.jpg', '02343822231'),
(N'Bệnh viện Đại học Y Dược', N'215 Hồng Bàng, Quận 5, TP.Hồ Chí Minh', N'https://example.com/images/ydhcm.jpg', '02839525353'),
(N'Bệnh viện 108', N'1 Trần Hưng Đạo, Hai Bà Trưng, Hà Nội', N'https://example.com/images/108.jpg', '02462784108'),
(N'Bệnh viện FV', N'6 Nguyễn Lương Bằng, Phú Mỹ Hưng, Quận 7, TP.Hồ Chí Minh', N'https://example.com/images/fv.jpg', '02854113333'),
(N'Bệnh viện Hữu nghị Việt Đức', N'40 Tràng Thi, Hoàn Kiếm, Hà Nội', N'https://example.com/images/vietduc.jpg', '02438253531'),
(N'Bệnh viện Nhi đồng 1', N'341 Sư Vạn Hạnh, Quận 10, TP.Hồ Chí Minh', N'https://example.com/images/nhidong1.jpg', '02839271119'),
(N'Bệnh viện Từ Dũ', N'284 Cống Quỳnh, Quận 1, TP.Hồ Chí Minh', N'https://example.com/images/tudu.jpg', '02854042525'),
(N'Bệnh viện Phụ sản Hà Nội', N'929 La Thành, Ba Đình, Hà Nội', N'https://example.com/images/phusanhanoi.jpg', '02438343223'),
(N'Bệnh viện Nhi đồng 2', N'14 Lý Tự Trọng, Quận 1, TP.Hồ Chí Minh', N'https://example.com/images/nhidong2.jpg', '02838295725'),
(N'Bệnh viện Ung bướu TP.Hồ Chí Minh', N'3 Nơ Trang Long, Bình Thạnh, TP.Hồ Chí Minh', N'https://example.com/images/ungbuouhcm.jpg', '02838412939'),
(N'Bệnh viện Tai Mũi Họng TW', N'78 Giải Phóng, Đống Đa, Hà Nội', N'https://example.com/images/taimuihong.jpg', '02438691337'),
(N'Bệnh viện Mắt TP.Hồ Chí Minh', N'280 Điện Biên Phủ, Quận 3, TP.Hồ Chí Minh', N'https://example.com/images/mathcm.jpg', '02839327546'),
(N'Bệnh viện Việt Pháp', N'1 Phương Mai, Đống Đa, Hà Nội', N'https://example.com/images/vietphap.jpg', '02435771111'),
(N'Bệnh viện Đa khoa Quốc tế Vinmec', N'458 Minh Khai, Hai Bà Trưng, Hà Nội', N'https://example.com/images/vinmec.jpg', '02439743456'),
(N'Bệnh viện Đại học Y Hà Nội', N'1 Tôn Thất Tùng, Đống Đa, Hà Nội', N'https://example.com/images/ydhn.jpg', '02435743291'),
(N'Bệnh viện Thống Nhất', N'1 Lý Thường Kiệt, Quận Tân Bình, TP.Hồ Chí Minh', N'https://example.com/images/thongnhat.jpg', '02838695735'),
(N'Bệnh viện Nhân dân 115', N'527 Sư Vạn Hạnh, Quận 10, TP.Hồ Chí Minh', N'https://example.com/images/115.jpg', '02838654127'),
(N'Bệnh viện Phổi Trung ương', N'463 Hoàng Hoa Thám, Ba Đình, Hà Nội', N'https://example.com/images/phoitw.jpg', '02438233044');

--2
INSERT INTO Events(EventDate, EventTime, EventTitle, EventContent, Location, MaxParticipants)
VALUES
('2024-03-15', '09:00', N'Ngày hội hiến máu mùa xuân', N'Sự kiện hiến máu đầu năm tại Bệnh viện B', N'456 Nguyễn Trãi, Q5', 60),
('2024-07-10', '13:30', N'Hiến máu nhân đạo', N'Chương trình hiến máu do Đoàn trường tổ chức', N'Đại học Y Dược TP.Hồ Chí Minh', 80),
('2023-11-25', '08:15', N'Giọt máu yêu thương', N'Hiến máu cứu người tại Bệnh viện C', N'789 Trần Hưng Đạo, Q3', 45),
('2024-01-20', '10:00', N'Tình nguyện vì sự sống', N'Chương trình hiến máu cho bệnh nhi ung thư', N'Viện Huyết học TP.Hồ Chí Minh', 70),
('2025-06-20', '08:00', N'Ngày hội hiến máu', N'Hiến máu cứu người tại Bệnh viện A', N'123 Lê Lợi, Q1', 50),
('2025-07-05', '07:30', N'Mỗi giọt máu – Một tấm lòng', N'Ngày hội hiến máu toàn thành phố', N'Công viên 23/9, Q1', 100),
('2025-08-12', '09:00', N'Hiến máu cứu người', N'Sự kiện phối hợp giữa Hội chữ thập đỏ và Bệnh viện D', N'12 Nguyễn Văn Cừ, Q5', 55),
('2025-09-01', '08:30', N'Trái tim nhân ái', N'Hiến máu vào dịp Quốc khánh', N'Nhà văn hóa Thanh Niên', 65),
('2025-10-15', '10:15', N'Ngày hội đỏ', N'Trao giọt máu – Trao hy vọng', N'Đại học Quốc Gia TP.Hồ Chí Minh', 120),
('2025-11-30', '13:00', N'Chung tay vì cộng đồng', N'Sự kiện lớn do Thành đoàn tổ chức', N'Nhà thi đấu Phú Thọ, Q11', 150);

--2
INSERT INTO Report (Username, ReportDate, ReportType, ReportContent)
VALUES
(N'user5', '2024-01-10', N'Góp ý', N'Đề nghị tăng số lượng điểm hiến máu di động trong thành phố'),
(N'user6', '2024-02-15', N'Khiếu nại', N'Thủ tục đăng ký hiến máu trực tuyến bị lỗi không hoàn thành'),
(N'staff1', '2024-03-20', N'Báo cáo', N'Tổng kết chương trình hiến máu Xuân hồng 2024'),
(N'user8', '2024-04-25', N'Góp ý', N'Cần bổ sung đồ ăn nhẹ sau khi hiến máu'),
(N'user1', '2025-01-05', N'Khiếu nại', N'Nhân viên lấy máu thao tác không đúng quy trình'),
(N'string', '2025-01-15', N'Báo cáo', N'Báo cáo sự cố thiết bị y tế tại điểm hiến máu'),
(N'user4', '2025-02-02', N'Góp ý', N'Đề nghị cung cấp giấy chứng nhận hiến máu điện tử'),
(N'user2', '2025-02-18', N'Khiếu nại', N'Chờ đợi quá lâu trước khi được hiến máu'),
(N'staff1', '2025-03-10', N'Báo cáo', N'Báo cáo kết quả chiến dịch hiến máu 8/3'),
(N'user4', '2025-03-28', N'Góp ý', N'Cần có thêm nhân viên hướng dẫn tại các điểm hiến máu'),
(N'user5', '2025-04-05', N'Khiếu nại', N'Thông tin cá nhân bị sai trên giấy chứng nhận'),
(N'string', '2025-04-15', N'Báo cáo', N'Báo cáo lượng máu tiếp nhận quý I/2025'),
(N'user7', '2025-06-01', N'Góp ý', N'Đề nghị cải thiện chất lượng áo phông tặng cho người hiến máu'),
(N'user8', '2025-06-05', N'Khiếu nại', N'Điểm hiến máu không đủ chỗ ngồi chờ'),
(N'staff1', '2025-06-10', N'Báo cáo', N'Báo cáo sơ bộ về ngày hội hiến máu 1/6'),
(N'user2', '2025-06-20', N'Góp ý', N'Đề xuất tổ chức hiến máu tại các trường đại học thường xuyên hơn'),
(N'user1', '2025-06-25', N'Khiếu nại', N'Không nhận được thông báo kết quả xét nghiệm sau hiến máu'),
(N'string', '2025-06-30', N'Báo cáo', N'Báo cáo tổng kết hoạt động hiến máu 6 tháng đầu năm'),
(N'user2', '2025-03-15', N'Khiếu nại', N'Thái độ nhân viên chưa tốt khi đi hiến máu'),
(N'user3', '2025-04-20', N'Góp ý', N'Đề nghị cải thiện cơ sở vật chất tại điểm hiến máu'),
(N'staff1', '2025-07-05', N'Báo cáo', N'Báo cáo hoạt động hiến máu quý 2 năm 2023');

--2
INSERT INTO Blog  (BlogTitle, BlogContent, BlogImage, BlogStatus, BlogDetail, Username)
VALUES 
(N'Lợi ích của việc hiến máu', N'Hiến máu không chỉ cứu người mà còn có lợi cho sức khỏe của bạn...', N'https://example.com/hienmau1.jpg',N'available', NULL, N'staff1'),
(N'Chuẩn bị gì trước khi hiến máu?', N'Để có một lần hiến máu thành công, bạn cần chuẩn bị...', N'https://example.com/hienmau2.jpg', N'available', NULL, N'string'),
(N'Những điều cần biết sau khi hiến máu', N'Sau khi hiến máu, bạn cần lưu ý những điều sau để đảm bảo sức khỏe...', N'https://example.com/hienmau3.jpg', N'unavailable', NULL, N'staff1');

--3
INSERT INTO AppointmentRecord (Username, EventId, RegistrationDate, Status, BloodType, DonationUnit, StaffNote)
VALUES 
('user1', 1, '2025-06-15', N'Đã hiến', 'B-', 1, NULL),
('user2', 2, '2025-06-14', N'Đã hiến', 'A+', 1, NULL),
('user3', 3, '2025-06-13', N'Chưa hiến', 'B+', 1, NULL),
('user4', 4, '2025-06-10', N'Hủy', 'O+', 0, NULL),
('user5', 5, '2025-06-11', N'Đã hiến', 'AB+', 1, NULL),
('user6', 1, '2025-06-12', N'Chưa hiến', 'AB-', 1, NULL),
('user7', 2, '2025-06-10', N'Đã hiến', 'O-', 1, NULL),
('user8', 3, '2025-06-08', N'Đã hiến', 'A-', 1, NULL),
('user9', 4, '2025-06-09', N'Hủy', 'B-', 0, NULL),
('user10', 5, '2025-06-07', N'Chưa hiến', 'A+', 1, NULL),
('user11', 1, '2025-06-06', N'Đã hiến', 'B+', 1, NULL),
('user12', 2, '2025-06-05', N'Đã hiến', 'O+', 1, NULL),
('user3', 5, '2025-06-20', N'Đang chờ', 'B+', NULL, N'Người hiến có huyết áp cao (150/90 mmHg). Yêu cầu nghỉ ngơi, giảm muối và caffeine trong chế độ ăn. Hẹn ngày mai quay lại kiểm tra lại huyết áp trước khi hiến máu.');
--3
INSERT INTO Emergency (Username, EmergencyDate, bloodType, EmergencyStatus, EmergencyNote, RequiredUnits, HospitalId, EmergencyMedical, EmergencyImage, EndDate)
VALUES 
(N'user5', '2025-06-11', N'B-', N'Đã xét duyệt', N'Cần 5 đơn vị nhóm máu B- tại Bệnh viện 108', 5, 5, N'Tai nạn giao thông', NULL, '2025-06-26');

--4
INSERT INTO Notification (EmergencyId, NotificationStatus, NotificationTitle, NotificationContent, NotificationDate)
VALUES 
(1, N'Đã gửi', N'Yêu cầu hiến máu khẩn cấp - Bệnh viện 108', N'Cần 5 đơn vị nhóm máu B- tại Bệnh viện 108', '2025-06-11');

--4
INSERT INTO BloodBank (BloodType, BloodVolumeTotal, BloodBankStatus)
VALUES 
('O+', 1, N'Còn'),
('A+', 8, N'Còn'),
('B+', 3, N'Còn'),
('AB+', 0, N'Hết'),
('O-', 2, N'Còn'),
('A-', 0, N'Hết'),
('B-', 1, N'Còn'),
('AB-', 4, N'Còn');

--4
INSERT INTO Certificate (AppointmentId, CertificateCode, IssueDate)
VALUES 
(1, N'CERT-2023-0001', '2025-01-15'),
(2, N'CERT-2023-0002', '2025-02-20'),
(3, N'CERT-2023-0003', '2025-05-10'),
(4, N'CERT-2023-0004', '2025-06-25'),
(5, N'CERT-2023-0005', '2025-08-12');

--5
INSERT INTO BloodDetail (BloodType, Volume, AppointmentId, HospitalId, BloodDetailDate, BloodDetailStatus, Note)
VALUES 
('O+', 1, 1, 1, '2025-06-21', N'Còn hạn', NULL),
('A+', 4, 2, 1, '2025-06-20', N'Còn hạn', NULL),
('B+', 3, 3, 2, '2025-06-19', N'Còn hạn', NULL),
('AB+', 1, 4, 3, '2025-06-18', N'Hết hạn', N'Sử dụng trước 2025-06-25'),
('O-', 2, 5, 2, '2025-06-17', N'Còn hạn', NULL),
('A-', 1, 6, 1, '2025-06-16', N'Hết hạn', N'Không sử dụng được'),
('B-', 1, 7, 3, '2025-06-15', N'Còn hạn', NULL),
('AB-', 4, 8, 2, '2025-06-14', N'Còn hạn', NULL),
('O+', 1, 9, 1, '2025-06-13', N'Hết hạn', N'Đã lưu trữ quá thời hạn cho phép'),
('A+', 4, 10, 3, '2025-06-12', N'Còn hạn', NULL);

--5
INSERT INTO NotificationRecipient (NotificationID, Username, ResponseStatus, ResponseDate, ResponseGo, ResponseTime)
VALUES 
(1, N'user1', N'Chấp nhận', '2025-10-01 10:30:00', '2025-10-05', '09:00:00'),
(1, N'user9', N'Chưa phản hồi', NULL, NULL, NULL);


-- Bảng câu hỏi
CREATE TABLE SurveyQuestion (
    QuestionId INT PRIMARY KEY IDENTITY(1,1),
    QuestionText NVARCHAR(MAX),
    QuestionType NVARCHAR(20) -- 'single', 'multiple', 'text'
);

-- Bảng lựa chọn câu trả lời
CREATE TABLE SurveyOption (
    OptionId INT PRIMARY KEY IDENTITY(1,1),
    QuestionId INT FOREIGN KEY REFERENCES SurveyQuestion(QuestionId),
    OptionText NVARCHAR(MAX),
    IsEligible BIT,           -- 1 = Đạt, 0 = Không đạt, NULL = Không xác định
    DisplayOrder INT,
    RequireText BIT DEFAULT 0
);


-- Bảng câu trả lời của người dùng
CREATE TABLE UserSurveyAnswer (
    AnswerId INT PRIMARY KEY IDENTITY(1,1),
    AppointmentId INT FOREIGN KEY REFERENCES AppointmentRecord(AppointmentId),
    QuestionId INT FOREIGN KEY REFERENCES SurveyQuestion(QuestionId),
    OptionId INT FOREIGN KEY REFERENCES SurveyOption(OptionId),
    AdditionalText NVARCHAR(MAX),
    AnswerDate DATETIME DEFAULT GETDATE()
);


INSERT INTO SurveyQuestion (QuestionText, QuestionType) VALUES
(N'1. Anh/chị từng hiến máu chưa?', 'single'),
(N'2. Hiện tại, anh/chị có mắc bệnh lý nào không?', 'single'),
(N'3. Trước đây, anh/chị có từng mắc một trong các bệnh: viêm gan siêu vi B, C, HIV...?', 'single'),
(N'4. Trong 12 tháng gần đây, anh/chị có?', 'multiple'),
(N'5. Trong 06 tháng gần đây, anh/chị có?', 'multiple'),
(N'6. Trong 01 tháng gần đây, anh/chị có?', 'multiple'),
(N'7. Trong 14 ngày gần đây, anh/chị có?', 'single'),
(N'8. Trong 07 ngày gần đây, anh/chị có?', 'single'),
(N'9. Câu hỏi dành cho phụ nữ:', 'multiple');


-- Q1
INSERT INTO SurveyOption VALUES (1, N'Có', 1, 1, 0), (1, N'Không', 1, 2, 0);
-- Q2
INSERT INTO SurveyOption VALUES (2, N'Có', NULL, 1, 1), (2, N'Không', 1, 2, 0);
-- Q3
INSERT INTO SurveyOption VALUES (3, N'Có', 0, 1, 0), (3, N'Không', 1, 2, 0), (3, N'Bệnh khác', NULL, 3, 1);
-- Q4
INSERT INTO SurveyOption VALUES
(4, N'Khởi bệnh sau khi mắc bệnh truyền nhiễm nặng', 0, 1, 0),
(4, N'Được truyền máu hoặc chế phẩm máu', 0, 2, 0),
(4, N'Tiêm vaccin?', NULL, 3, 1),
(4, N'Không', 1, 4, 0);
-- Q5
INSERT INTO SurveyOption VALUES
(5, N'Khởi bệnh nghiêm trọng', 0, 1, 0),
(5, N'Sút cân nhanh không rõ nguyên nhân', 0, 2, 0),
(5, N'Nổi hạch kéo dài', 0, 3, 0),
(5, N'Thủ thuật y tế xâm lấn', 0, 4, 0),
(5, N'Xăm, xỏ cơ thể', 0, 5, 0),
(5, N'Sử dụng ma túy', 0, 6, 0),
(5, N'Tiếp xúc máu hoặc dịch tiết người khác', 0, 7, 0),
(5, N'Sống chung với người bị viêm gan B', 0, 8, 0),
(5, N'Quan hệ tình dục có nguy cơ', 0, 9, 0),
(5, N'Quan hệ tình dục đồng giới', 0, 10, 0),
(5, N'Không', 1, 11, 0);
-- Q6
INSERT INTO SurveyOption VALUES
(6, N'Khỏi bệnh sau mắc bệnh nhiễm trùng', 0, 1, 0),
(6, N'Đi vùng dịch bệnh lưu hành', 0, 2, 0),
(6, N'Không', 1, 3, 0);
-- Q7
INSERT INTO SurveyOption VALUES
(7, N'Bị cúm, cảm lạnh, sốt, ho, đau họng', 0, 1, 0),
(7, N'Không', 1, 2, 0),
(7, N'Khác (cụ thể)', NULL, 3, 1);
-- Q8
INSERT INTO SurveyOption VALUES
(8, N'Dùng thuốc kháng sinh, Corticoid...', 0, 1, 0),
(8, N'Không', 1, 2, 0),
(8, N'Khác (cụ thể)', NULL, 3, 1);
-- Q9
INSERT INTO SurveyOption VALUES
(9, N'Đang mang thai hoặc nuôi con nhỏ', 0, 1, 0),
(9, N'Chấm dứt thai kỳ trong 12 tháng', 0, 2, 0),
(9, N'Không', 1, 3, 0);


-- Dữ liệu mẫu UserSurveyAnswer (giả sử các OptionId tương ứng với "Không")
INSERT INTO UserSurveyAnswer (AppointmentId, QuestionId, OptionId, AdditionalText)
VALUES
(1, 1, 2, NULL),
(1, 2, 4, NULL),
(1, 3, 6, NULL),
(1, 4, 10, NULL),
(1, 5, 21, NULL),
(1, 6, 24, NULL),
(1, 7, 27, NULL),
(1, 8, 30, NULL),
(1, 9, 33, NULL);


--chay dong nay de them vao Event:

ALTER TABLE Events
ADD
  BloodTypeRequired NVARCHAR(10),
  CurrentParticipants INT DEFAULT 0;

--them 2 bảng mới 

CREATE TABLE DeferralReason (
    ReasonCode NVARCHAR(50) PRIMARY KEY,
    ReasonText NVARCHAR(255) NOT NULL,
    MinDays INT NULL,
    IsPermanent BIT NOT NULL DEFAULT 0,
    Note NVARCHAR(MAX)
);

-- Dữ liệu đầy đủ
INSERT INTO DeferralReason (ReasonCode, ReasonText, MinDays, IsPermanent, Note)
VALUES
-- Tạm thời
('LOW_HB',         N'Thiếu Hemoglobin (Hb)',         30, 0, N'Hb < 12.5 (nữ) hoặc < 13.0 (nam), cần bổ sung sắt'),
('HIGH_BP',        N'Huyết áp cao (>140/90)',        14, 0, N'Nghỉ ngơi, ăn nhạt, theo dõi lại'),
('LOW_BP',         N'Huyết áp thấp (<90/60)',         7, 0, N'Bù nước, tăng vận động, theo dõi'),
('HEART_RATE',     N'Nhịp tim bất thường',           10, 0, N'Nhịp tim trên 100 hoặc dưới 50 bpm, đo lại khi bình tĩnh'),
('SYPHILIS_POS',   N'Nhiễm giang mai (+)',          365, 0, N'Tạm hoãn 12 tháng, cần điều trị'),

-- Vĩnh viễn
('HIV_POS',        N'Nhiễm HIV (+)',                NULL, 1, N'HIV dương tính, không đủ điều kiện hiến máu'),
('HBV_POS',        N'Nhiễm viêm gan B (+)',         NULL, 1, N'HBsAg dương tính'),
('HCV_POS',        N'Nhiễm viêm gan C (+)',         NULL, 1, N'HCV dương tính');


CREATE TABLE DonorDeferral (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,               -- Khóa ngoại liên kết với User
    ReasonCode NVARCHAR(50) NOT NULL,             -- Khóa ngoại liên kết lý do trì hoãn
    StartDate DATE NOT NULL,
    EndDate DATE NULL,                            -- NULL nếu trì hoãn vĩnh viễn
    IsPermanent BIT NOT NULL DEFAULT 0,           -- 1 = vĩnh viễn
    Note NVARCHAR(MAX),                           -- Ghi chú chi tiết

    FOREIGN KEY (Username) REFERENCES [User](Username),
    FOREIGN KEY (ReasonCode) REFERENCES DeferralReason(ReasonCode)
);
