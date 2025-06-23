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

        public async Task<bool> CheckInAsync(CheckInDto checkInDto)
        {
            var check = await _context.AppointmentRecords.FindAsync(checkInDto.AppointmentId);
            if (check == null || check.Status== "Đã hiến")
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
            if (user == null )
                return false;
            //kiểm tra xem người này đã được ghi nhận hiến máu chưa
            
            if (appointment.Status == "Đã hiến" )
            {
                throw new Exception("Người dùng đã được ghi nhận hiến máu ");
            }

            //ưu tiên lấy nhóm máu từ người dùng đã đăng ký, nếu không có thì sử dụng nhóm máu từ donateDto
            string? bloodType = user.BloodType;
            if (string.IsNullOrEmpty(bloodType))
            {
               if(string.IsNullOrEmpty(donateDto.BloodType))
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
                BloodType = donateDto.BloodType,
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
