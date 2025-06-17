using BloodDonationAPI.DTO;
using BloodDonationAPI.DTO.BloodInventory;
using BloodDonationAPI.DTOs.BloodInventory;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodDonationAPI.Service.Impl;

public class BloodInventoryService : IBloodInventoryService
{
    private readonly BloodDonationSystemContext _context;
    private readonly ILogger<BloodInventoryService> _logger;

    public BloodInventoryService(BloodDonationSystemContext context, ILogger<BloodInventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Lấy tổng số lượng máu từ bảng BloodBank
    public async Task<List<BloodBankDTO>> GetBloodBankAsync()
    {
        try
        {
            var bloodBanks = await _context.BloodBanks
                .Select(b => new BloodBankDTO
                {
                    BloodType = b.BloodType,
                    BloodVolumeTotal = b.BloodVolumeTotal,
                    BloodBankStatus = b.BloodBankStatus
                })
                .OrderBy(b => b.BloodType)
                .ToListAsync();

            return bloodBanks;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in GetBloodBankAsync: {ex.Message}");
            throw;
        }
    }

    // Lấy chi tiết từ bảng BloodDetail
    public async Task<BloodInventoryResponseDTO> GetBloodInventoryAsync()
    {
        try
        {
            var bloodDetails = await _context.BloodDetails
                .Select(b => new BloodInventoryItemDTO
                {
                    BloodDetailId = b.BloodDetailId,
                    BloodType = b.BloodType,
                    Volume = b.Volume ?? 0,
                    BloodDetailDate = b.BloodDetailDate,
                    BloodDetailStatus = b.BloodDetailStatus,
                    Note = b.Note,
                    HospitalId = b.HospitalId
                })
                .OrderBy(b => b.BloodType)
                .ThenBy(b => b.BloodDetailDate)
                .ToListAsync();

            return new BloodInventoryResponseDTO
            {
                Inventory = bloodDetails
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in GetBloodInventoryAsync: {ex.Message}");
            throw;
        }
    }

    // Thêm máu mới vào BloodDetail và cập nhật BloodBank
    public async Task<BloodBankDTO> AddBloodInventoryAsync(AddBloodBankDto request)
    {
        try 
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Thêm bản ghi mới vào BloodDetail, luôn gán HospitalId = 1
            var newBloodDetail = new BloodDetail
            {
                BloodType = request.BloodType,
                Volume = request.Volume,
                BloodDetailDate = request.BloodDetailDate,
                BloodDetailStatus = "Còn hạn", // Mặc định là còn hạn
                Note = request.Note,
                HospitalId = 1 // Gán mặc định là 1
            };

            _context.BloodDetails.Add(newBloodDetail);
            await _context.SaveChangesAsync();

            // Tính tổng số lượng máu còn hạn từ BloodDetail cho nhóm máu này
            var totalVolume = await _context.BloodDetails
                .Where(b => b.BloodType == request.BloodType && 
                           b.BloodDetailStatus == "Còn hạn")
                .SumAsync(b => b.Volume ?? 0);

            // Tìm hoặc tạo mới BloodBank cho nhóm máu này
            var bloodBank = await _context.BloodBanks
                .FirstOrDefaultAsync(b => b.BloodType == request.BloodType);

            if (bloodBank != null)
            {
                bloodBank.BloodVolumeTotal = totalVolume;
                bloodBank.BloodBankStatus = totalVolume > 0 ? "Còn" : "Hết";
            }
            else
            {
                bloodBank = new BloodBank
                {
                    BloodType = request.BloodType,
                    BloodVolumeTotal = totalVolume,
                    BloodBankStatus = totalVolume > 0 ? "Còn" : "Hết"
                };
                _context.BloodBanks.Add(bloodBank);
            }

            await _context.SaveChangesAsync();

            return new BloodBankDTO
            {
                BloodType = bloodBank.BloodType,
                BloodVolumeTotal = bloodBank.BloodVolumeTotal,
                BloodBankStatus = bloodBank.BloodBankStatus
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in AddBloodInventoryAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<BloodBankDTO> ExpireBloodAsync(ExpireBloodRequestDTO request)
    {
        try
        {
            // Tìm bản ghi máu theo ID trong BloodDetail
            var bloodDetail = await _context.BloodDetails
                .FirstOrDefaultAsync(b => b.BloodDetailId == request.BloodDetailId);

            if (bloodDetail == null)
            {
                throw new InvalidOperationException($"Blood detail record with ID {request.BloodDetailId} not found");
            }

            if (bloodDetail.BloodDetailStatus == "Hết hạn")
            {
                throw new InvalidOperationException($"Blood detail record with ID {request.BloodDetailId} is already expired");
            }

            // Lưu lại volume trước khi đổi trạng thái
            var expiredVolume = bloodDetail.Volume ?? 0;
            var bloodType = bloodDetail.BloodType;

            // Cập nhật trạng thái
            bloodDetail.BloodDetailStatus = "Hết hạn";
            bloodDetail.BloodDetailDate = DateOnly.FromDateTime(DateTime.Today);

            // Cập nhật tổng trong BloodBank
            var bloodBank = await _context.BloodBanks
                .FirstOrDefaultAsync(b => b.BloodType == bloodType);

            if (bloodBank != null)
            {
                bloodBank.BloodVolumeTotal = (bloodBank.BloodVolumeTotal ?? 0) - expiredVolume;
                if (bloodBank.BloodVolumeTotal < 0) bloodBank.BloodVolumeTotal = 0;
                bloodBank.BloodBankStatus = bloodBank.BloodVolumeTotal > 0 ? "Còn" : "Hết";
            }

            await _context.SaveChangesAsync();

            return new BloodBankDTO
            {
                BloodType = bloodBank?.BloodType ?? bloodType,
                BloodVolumeTotal = bloodBank?.BloodVolumeTotal ?? 0,
                BloodBankStatus = bloodBank?.BloodBankStatus ?? "Hết"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in ExpireBloodAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<UseBloodResponseDTO> UseBloodAsync(UseBloodRequestDTO request)
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Lấy các dòng máu còn hạn, sắp xếp ngày cũ nhất trước
            var availableBlood = await _context.BloodDetails
                .Where(b => b.BloodType == request.BloodType
                         && b.BloodDetailStatus == "Còn hạn"
                         && b.Volume > 0)
                .OrderBy(b => b.BloodDetailDate)
                .ToListAsync();

            var totalAvailable = availableBlood.Sum(b => b.Volume ?? 0);

            if (totalAvailable < request.RequiredUnits)
            {
                throw new InvalidOperationException($"Không đủ số lượng máu. Yêu cầu: {request.RequiredUnits}, Hiện có: {totalAvailable}");
            }

            var response = new UseBloodResponseDTO
            {
                BloodType = request.BloodType,
                Note = request.Note
            };

            var remainingUnits = request.RequiredUnits;

            foreach (var blood in availableBlood)
            {
                if (remainingUnits <= 0) break;

                int available = blood.Volume ?? 0;
                if (available <= remainingUnits)
                {
                    // Đổi status thành "Đã sử dụng", cập nhật note, hospitalId, ngày sử dụng
                    blood.BloodDetailStatus = "Đã sử dụng";
                    blood.Note = request.Note;
                    blood.HospitalId = request.HospitalId ?? 1;
                    blood.BloodDetailDate = today;

                    response.UsageDetails.Add(new BloodUsageDetailDTO
                    {
                        BloodDetailId = blood.BloodDetailId,
                        UsedUnits = available,
                        BloodDetailDate = today
                    });

                    remainingUnits -= available;
                }
                else
                {
                    // Tách dòng: giữ lại dòng gốc với volume mới, tạo dòng mới đã sử dụng
                    blood.Volume = available - remainingUnits;

                    var usedDetail = new BloodDetail
                    {
                        BloodType = blood.BloodType,
                        Volume = remainingUnits,
                        BloodDetailDate = today,
                        BloodDetailStatus = "Đã sử dụng",
                        Note = request.Note,
                        AppointmentId = blood.AppointmentId,
                        HospitalId = request.HospitalId ?? 1
                    };
                    _context.BloodDetails.Add(usedDetail);
                    await _context.SaveChangesAsync(); // Để lấy BloodDetailId mới

                    response.UsageDetails.Add(new BloodUsageDetailDTO
                    {
                        BloodDetailId = usedDetail.BloodDetailId,
                        UsedUnits = remainingUnits,
                        BloodDetailDate = today
                    });

                    remainingUnits = 0;
                    break;
                }
            }

            response.TotalUsedUnits = request.RequiredUnits;

            // Cập nhật lại BloodBank
            var bloodBank = await _context.BloodBanks
                .FirstOrDefaultAsync(b => b.BloodType == request.BloodType);

            if (bloodBank != null)
            {
                bloodBank.BloodVolumeTotal = (bloodBank.BloodVolumeTotal ?? 0) - request.RequiredUnits;
                if (bloodBank.BloodVolumeTotal < 0) bloodBank.BloodVolumeTotal = 0;
                bloodBank.BloodBankStatus = bloodBank.BloodVolumeTotal > 0 ? "Còn" : "Hết";
            }

            await _context.SaveChangesAsync();

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in UseBloodAsync: {ex.Message}");
            throw;
        }
    }
}
