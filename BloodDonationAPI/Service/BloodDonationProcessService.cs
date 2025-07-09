using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Service
{
    public class BloodDonationProcessService : IBloodDonationProcessService
    {
        private readonly BloodDonationSystemContext _context;

        public BloodDonationProcessService(BloodDonationSystemContext context)
        {
            _context = context;
        }



        public async Task<List<AppointmentRegistrationDto>> GetRegistrationsByEventID(int EventID)
        {
            return await _context.AppointmentRecords
                 .Where(h => h.EventId == EventID)
                 .Include(h => h.UsernameNavigation)
                 .Select(h => new AppointmentRegistrationDto
                 {
                     AppointmentId = h.AppointmentId,
                     Username = h.Username,
                     FullName = h.UsernameNavigation.FullName,
                     Phone = h.UsernameNavigation.Phone,
                     AppointmentStatus = h.Status,
                     BloodType = h.UsernameNavigation.BloodType,
                 })
                 .ToListAsync();

        }

        //public async Task<bool> VerifyUserIdentityAsync(CheckInDto checkInDto)
        //{
        //    // Tìm lịch hẹn theo ID
        //    var appointment = await _context.AppointmentRecords.FindAsync(checkInDto.AppointmentId);
        //    if (appointment == null || appointment.Status == "Đã hiến")
        //        return false;
            
        //    // Tìm người dùng theo fullname và email để xác minh danh tính
        //    var user = await _context.Users
        //        .FirstOrDefaultAsync(u => u.FullName == checkInDto.FullName && 
        //                              u.Email == checkInDto.Email && 
        //                              u.Username == appointment.Username);
            
        //    if (user == null)
        //        return false; // Không tìm thấy user với thông tin đã cung cấp
            
        //    return true; // Xác thực danh tính thành công
        //}
        
        public async Task<bool> UpdateDonationStatusAsync(int appointmentId, string status, string staffNote)
        {
            // Tìm lịch hẹn theo ID
            var appointment = await _context.AppointmentRecords.FindAsync(appointmentId);
            if (appointment == null)
                return false;
            
            // Cập nhật trạng thái và ghi chú
            appointment.Status = status;
            appointment.StaffNote = staffNote;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckInAsync(CheckInDto checkInDto)
        {
            var check = await _context.AppointmentRecords.FindAsync(checkInDto.AppointmentId);
            if (check == null || check.Status == "Đã hiến")
                return false;
            check.Status = "Đã đến";
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> RecordDonationAsync(DonateDto donateDto)
        {
            var appointment = await _context.AppointmentRecords.FirstOrDefaultAsync(a => a.AppointmentId == donateDto.AppointmentId);

            if (appointment == null)
                return false;
            // lay nhom mau tu user đã đăng ký de su dung cho việc hiến nếu có 
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == appointment.Username);
            if (user == null)
                return false;
            //kiểm tra xem người này đã được ghi nhận hiến máu chưa

            if (appointment.Status == "Đã hiến")
            {
                throw new Exception("Người dùng đã được ghi nhận hiến máu ");
            }

            //ưu tiên lấy nhóm máu từ người dùng đã đăng ký, nếu không có thì sử dụng nhóm máu từ donateDto
            string? bloodType = user.BloodType;
            if (string.IsNullOrEmpty(bloodType))
            {
                if (string.IsNullOrEmpty(donateDto.BloodType))
                {
                    throw new Exception("Không có nhóm máu trong hồ sơ người dùng và nhân viên cũng không cung cấp nhóm máu");
                }
                // Nếu người dùng không có nhóm máu trong hồ sơ, sử dụng nhóm máu từ donateDto 
                bloodType = donateDto.BloodType;
                user.BloodType = bloodType; // Cập nhật nhóm máu cho người dùng nếu không có trong hồ sơ
            }

            // cap nhat trong bang appointment records
            appointment.Status = "Đã hiến";
            appointment.BloodType = bloodType;//lay nhom mau tu user đã đăng ký hoặc từ donateDto
            appointment.DonationUnit = donateDto.Volume;

            // cap nhat trang thai cho user 

            user.ProfileStatus = "Đang nghỉ ngơi";
            // Tạo bản ghi trong bảng BloodDetail
            var bloodDetail = new BloodDetail
            {
                AppointmentId = donateDto.AppointmentId,
                BloodType = bloodType,
                Volume = donateDto.Volume,
                HospitalId = 1, // Giả sử HospitalId là 1, bạn có thể thay đổi theo logic của bạn
                BloodDetailDate = DateOnly.FromDateTime(DateTime.Now),
                BloodDetailStatus = "Đã lưu trữ",
            };
            _context.BloodDetails.Add(bloodDetail);
            //cap nhat trong bang blood bank
            var bloodBank = await _context.BloodBanks.FirstOrDefaultAsync(b => b.BloodType == donateDto.BloodType);

            if (bloodBank != null)
            {
                bloodBank.BloodVolumeTotal += donateDto.Volume; // Cộng thêm đơn vị máu vào ngân hàng máu
            }
            else
            {
                // Nếu không có bản ghi nào trong BloodBank, bạn có thể tạo mới hoặc xử lý theo logic của bạn
                bloodBank = new BloodBank
                {
                    BloodType = donateDto.BloodType,
                    BloodVolumeTotal = donateDto.Volume,

                    BloodBankStatus = "Còn"
                };
                _context.BloodBanks.Add(bloodBank);

            }
            // Lưu tất cả các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();
            //tao chung nhan sau khi hiến máu
            string hospitalName = "Viện Huyết học - Truyền máu TW"; // Giả sử tên bệnh viện là cố định, bạn có thể thay đổi theo logic của bạn
            string certificateCode = $"CTF-{DateTime.Today:yyyyMMdd}-{user.Username.ToUpper()}"; // Tạo mã chứng nhận theo định dạng mong muốn

            //them vao bang certificatr
            var certificate = new Certificate
            {
                AppointmentId = donateDto.AppointmentId,
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth ?? DateOnly.FromDateTime(new DateTime(2000, 1, 1)),
                Address = user.Address ,
                HospitalName = hospitalName,
                BloodAmount = donateDto.Volume,
                DonationDate = DateOnly.FromDateTime(DateTime.Now),
                CertificateCode = certificateCode,
                IssueDate = DateOnly.FromDateTime(DateTime.Now),
            };
            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task UpdateEligibleUsersAsync()
        {
            var today = DateTime.Today;

            var users = await _context.Users
                .Include(u => u.AppointmentRecords)
                .Where(u => u.ProfileStatus == "Đang nghỉ ngơi")
                .Select(u => new
                {
                    User = u,
                    LastDonationDate = u.AppointmentRecords
                        .Where(a => a.Status == "Đã hiến")
                        .OrderByDescending(a => a.RegistrationDate)
                        .Select(a => a.RegistrationDate)
                        .FirstOrDefault()
                }).ToListAsync();

            foreach (var item in users)
            {
                if (item.LastDonationDate == null) continue;

                int waitDate = item.User.Gender == "Nữ" ? 112 : 84; // Ngày chờ tùy theo giới tính
                DateTime nextEligibleDate = item.LastDonationDate.Value.AddDays(waitDate);

                if (today >= nextEligibleDate)
                {
                    item.User.ProfileStatus = "Active"; // Cập nhật trạng thái người dùng thành "Active"
                    Console.WriteLine($"✅ {item.User.Username} đã nghỉ đủ {waitDate} ngày.");
                }

                await _context.SaveChangesAsync();

            }
        }

        public async Task<bool> UpdateAppointmentNoteAsync(AppointmentNoteDto appointmentNoteDto)
        {
            var appointment = await _context.AppointmentRecords
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentNoteDto.AppointmentId);
            if (appointment == null) return false;

            var reasonCode = await _context.DeferralReasons
                .FirstOrDefaultAsync(r => r.ReasonCode == appointmentNoteDto.ReasonCode);
            if (reasonCode == null) throw new Exception("ReasonCode khong hop le.");
            appointment.StaffNote = reasonCode.ReasonText;

            var deferral = new DonorDeferral
            {
                Username = appointment.Username,
                ReasonCode = reasonCode.ReasonCode,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                IsPermanent = reasonCode.IsPermanent,
                Note = appointmentNoteDto.CustomNote,
            };
            if (!reasonCode.IsPermanent && reasonCode.MinDays.HasValue)
            {
               deferral.EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(reasonCode.MinDays.Value));
            }
            else
            {
                deferral.EndDate = null; // Không có ngày kết thúc nếu là vĩnh viễn hoặc không có ngày tối thiểu
            }
            _context.DonorDeferrals.Add(deferral);
            await _context.SaveChangesAsync();
            return true;


        }

        public async Task<bool> DestroyBloodDonationAsync(DestroyBloodDonationDto dto)
        {
            var bloodDetail = await _context.BloodDetails
                .Include(b => b.Appointment)
                .FirstOrDefaultAsync(b => b.BloodDetailId == dto.BloodDetailID);
            // kiem tra xem co hop le khong
            if (bloodDetail == null)
                return false;
            // cap nhat vap bloodDetail
            bloodDetail.BloodDetailStatus = "Tiêu hủy";
            bloodDetail.Note = dto.CustomNote;
            _context.BloodDetails.Update(bloodDetail);
            
            //kiem tra xem co li do nay hay khong va user name nay co ton tai hay ko
            if (!string.IsNullOrEmpty(dto.ReasonCode) && bloodDetail.Appointment?.Username != null) 
            {
                var reasonCode = await _context.DeferralReasons
                    .FirstOrDefaultAsync(r => r.ReasonCode == dto.ReasonCode);
                // kiem tra xem li do nay da co hay chua 
                if (reasonCode != null) 
                {
                    bool alreadyExists = await _context.DonorDeferrals
                        .AnyAsync(d => d.Username == bloodDetail.Appointment.Username &&
                                       d.ReasonCode == dto.ReasonCode &&
                                       d.IsPermanent == true);
                    if (!alreadyExists)
                    {
                        var deferral = new DonorDeferral
                        {
                            Username = bloodDetail.Appointment.Username,
                            ReasonCode = reasonCode.ReasonCode,
                            StartDate = DateOnly.FromDateTime(DateTime.Now),
                            IsPermanent = reasonCode.IsPermanent,
                            Note = dto.CustomNote,
                        };
                        if (!reasonCode.IsPermanent && reasonCode.MinDays.HasValue)
                        {
                            deferral.EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(reasonCode.MinDays.Value));
                        }
                        else
                        {
                            deferral.EndDate = null; // Không có ngày kết thúc nếu là vĩnh viễn hoặc không có ngày tối thiểu
                        }
                        _context.DonorDeferrals.Add(deferral);
                    }



                }

            }
            if(!string.IsNullOrWhiteSpace(bloodDetail.BloodType) && bloodDetail.Volume > 0)
            {
                // Cập nhật tổng lượng máu trong ngân hàng máu
                var bloodBank = await _context.BloodBanks.FirstOrDefaultAsync(b => b.BloodType == bloodDetail.BloodType);
                if (bloodBank != null)
                {
                    bloodBank.BloodVolumeTotal -= bloodDetail.Volume; // Giảm lượng máu trong ngân hàng máu
                    if (bloodBank.BloodVolumeTotal < 0)
                        bloodBank.BloodVolumeTotal = 0; // Đảm bảo không âm
                    _context.BloodBanks.Update(bloodBank);
                }
            }

            await _context.SaveChangesAsync();
            return true;


        }




        //public async Task<bool> AddDonationHistoryAsync(CreateDonationHistoryDto registrationDto)
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == registrationDto.Username);
        //    if (user == null)
        //        return false;
        //    var donationHistory = new DonationHistory
        //    {
        //        Username = registrationDto.Username,
        //        BloodType = registrationDto.BloodType,
        //        DonationDate = registrationDto.DonationDate,
        //        DonationStatus = registrationDto.DonationStatus,
        //        DonationUnit = registrationDto.DonationUnit
        //    };

        //    _context.DonationHistories.Add(donationHistory);
        //    await _context.SaveChangesAsync();
        //    return true;
        //}

        //public async Task<BloodBank> AddBloodToBankAsync(AddBloodBankDto dto)
        //{
        //    var donation = await _context.DonationHistories.FirstOrDefaultAsync(h => h.DonationHistoryId == dto.DonationHistoryId);
        //    if (donation == null)
        //        return null;
        //    var bloodBank = new BloodBank
        //    {

        //        BloodTypeName = dto.BloodTypeName,
        //        Unit = dto.Unit,
        //        DonationHistoryId = dto.DonationHistoryId,
        //        ExpiryDate = dto.ExpiryDate,
        //        Status = string.IsNullOrWhiteSpace(dto.Status) ? "storing" : dto.Status.Trim().ToLower()
        //    };
        //    _context.BloodBanks.Add(bloodBank);

        //    donation.DonationStatus = "stored"; // Update donation status to 'stored'
        //    _context.DonationHistories.Update(donation);
        //    await _context.SaveChangesAsync();
        //    return bloodBank;


        //}

        //public async Task<List<DonationHistoryDto>> GetDonationHistoryByUserNameAsync(string username)
        //{
        //    return await _context.DonationHistories
        //        .Where(d => d.Username == username)
        //        .Select(d => new DonationHistoryDto
        //        {
        //            DonationHistoryId = d.DonationHistoryId,
        //            Username = d.Username,
        //            BloodType = d.BloodType,
        //            DonationDate = d.DonationDate,
        //            DonationStatus = d.DonationStatus,
        //            DonationUnit = d.DonationUnit
        //        })
        //        .OrderByDescending(d => d.DonationDate)
        //        .ToListAsync();
        //}
    }
}
