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

    public async Task<BloodInventoryResponseDTO> GetBloodInventoryAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var bloodInventory = await _context.BloodBanks
            .GroupBy(b => b.BloodTypeName)
            .Select(group => new BloodInventoryItemDTO
            {
                BloodTypeName = group.Key,
                TotalUnits = group.Sum(b => b.Unit ?? 0),
                AvailableUnits = group.Where(b =>
                        b.Status != "Expired" &&
                        (b.ExpiryDate == null || b.ExpiryDate > today))
                    .Sum(b => b.Unit ?? 0),
                ExpiredUnits = group.Where(b =>
                        b.Status == "Expired" ||
                        (b.ExpiryDate != null && b.ExpiryDate <= today))
                    .Sum(b => b.Unit ?? 0)
            })
            .ToListAsync();

        return new BloodInventoryResponseDTO
        {
            Inventory = bloodInventory
        };
    }

    public async Task<BloodBankDTO> UpdateBloodInventoryAsync(UpdateBloodInventoryRequestDTO request)
    {
        try 
        {
            // Validate request
            if (string.IsNullOrEmpty(request.BloodType))
                throw new ArgumentException("Blood type is required");

            if (request.Unit <= 0)
                throw new ArgumentException("Unit amount must be positive");

            var today = DateOnly.FromDateTime(DateTime.Today);

            // Tạo bản ghi máu mới
            var newBlood = new BloodBank
            {
                BloodTypeName = request.BloodType,
                Unit = request.Unit,
                ExpiryDate = today.AddDays(42), // Máu có hạn sử dụng 42 ngày
                Status = "Available"
            };

            _context.BloodBanks.Add(newBlood);
            await _context.SaveChangesAsync();

            // Return thông tin bản ghi vừa tạo
            return new BloodBankDTO
            {
                BloodTypeId = newBlood.BloodTypeId,
                BloodTypeName = newBlood.BloodTypeName,
                Unit = newBlood.Unit,
                ExpiryDate = newBlood.ExpiryDate,
                Status = newBlood.Status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in UpdateBloodInventoryAsync: {ex.Message}");
            throw;
        }
    }

    // Xử lý thêm máu mới
    private async Task<BloodInventoryItemDTO> HandleDonation(string bloodType, int amount, DateOnly today)
    {
        var bloodBank = new BloodBank
        {
            BloodTypeName = bloodType,
            Unit = amount,
            ExpiryDate = today.AddDays(42), // Máu có hạn sử dụng 42 ngày
            Status = "Available"
        };
        
        _context.BloodBanks.Add(bloodBank);
        await _context.SaveChangesAsync();
        
        return await GetInventoryStatus(bloodType, today);
    }

    // Xử lý sử dụng máu
    private async Task<BloodInventoryItemDTO> HandleUsage(string bloodType, int amount, DateOnly today)
    {
        // Lấy danh sách máu còn sử dụng được, sắp xếp theo ngày hết hạn (để dùng máu gần hết hạn trước)
        var availableBlood = await _context.BloodBanks
            .Where(b => b.BloodTypeName == bloodType &&
                       b.Status != "Expired" &&
                       (b.ExpiryDate == null || b.ExpiryDate > today))
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        var totalAvailable = availableBlood.Sum(b => b.Unit ?? 0);
        if (amount > totalAvailable)
        {
            throw new InvalidOperationException($"Not enough available units. Requested: {amount}, Available: {totalAvailable}");
        }

        var remainingAmount = amount;
        foreach (var blood in availableBlood)
        {
            if (remainingAmount <= 0) break;

            var unitsToUse = Math.Min(blood.Unit ?? 0, remainingAmount);
            blood.Unit -= unitsToUse;
            remainingAmount -= unitsToUse;

            // Nếu đã sử dụng hết đơn vị máu này
            if (blood.Unit <= 0)
            {
                _context.BloodBanks.Remove(blood);
            }
        }

        await _context.SaveChangesAsync();
        return await GetInventoryStatus(bloodType, today);
    }

    // Xử lý máu hết hạn
    private async Task<BloodInventoryItemDTO> HandleExpired(string bloodType, int amount, DateOnly today)
    {
        // Lấy danh sách máu chưa hết hạn, sắp xếp theo ngày hết hạn (để đánh dấu máu gần hết hạn trước)
        var availableBlood = await _context.BloodBanks
            .Where(b => b.BloodTypeName == bloodType &&
                       b.Status != "Expired" &&
                       (b.ExpiryDate == null || b.ExpiryDate > today))
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        var totalAvailable = availableBlood.Sum(b => b.Unit ?? 0);
        if (amount > totalAvailable)
        {
            throw new InvalidOperationException($"Not enough available units to mark as expired. Requested: {amount}, Available: {totalAvailable}");
        }

        var remainingAmount = amount;
        foreach (var blood in availableBlood)
        {
            if (remainingAmount <= 0) break;

            var unitsToExpire = Math.Min(blood.Unit ?? 0, remainingAmount);
            
            if (unitsToExpire == blood.Unit)
            {
                // Nếu đánh dấu hết hạn toàn bộ đơn vị máu này
                blood.Status = "Expired";
            }
            else
            {
                // Nếu chỉ đánh dấu hết hạn một phần
                // Tạo bản ghi mới cho phần máu hết hạn
                var expiredBlood = new BloodBank
                {
                    BloodTypeName = bloodType,
                    Unit = unitsToExpire,
                    ExpiryDate = today,
                    Status = "Expired"
                };
                _context.BloodBanks.Add(expiredBlood);
                
                // Giảm số lượng máu còn lại
                blood.Unit -= unitsToExpire;
            }
            
            remainingAmount -= unitsToExpire;
        }

        await _context.SaveChangesAsync();
        return await GetInventoryStatus(bloodType, today);
    }

    // Helper method để lấy trạng thái hiện tại của kho máu
    private async Task<BloodInventoryItemDTO> GetInventoryStatus(string bloodType, DateOnly today)
    {
        var inventory = await _context.BloodBanks
            .Where(b => b.BloodTypeName == bloodType)
            .GroupBy(b => b.BloodTypeName)
            .Select(group => new BloodInventoryItemDTO
            {
                BloodTypeName = group.Key,
                TotalUnits = group.Sum(b => b.Unit ?? 0),
                AvailableUnits = group.Where(b => 
                    b.Status != "Expired" && 
                    (b.ExpiryDate == null || b.ExpiryDate > today))
                    .Sum(b => b.Unit ?? 0),
                ExpiredUnits = group.Where(b => 
                    b.Status == "Expired" || 
                    (b.ExpiryDate != null && b.ExpiryDate <= today))
                    .Sum(b => b.Unit ?? 0)
            })
            .FirstOrDefaultAsync();

        return inventory ?? new BloodInventoryItemDTO
        {
            BloodTypeName = bloodType,
            TotalUnits = 0,
            AvailableUnits = 0,
            ExpiredUnits = 0
        };
    }

    public async Task<List<BloodBankDTO>> GetAllBloodBankAsync()
    {
        try
        {
            var bloodBanks = await _context.BloodBanks
                .Select(b => new BloodBankDTO
                {
                    BloodTypeId = b.BloodTypeId,
                    BloodTypeName = b.BloodTypeName,
                    Unit = b.Unit,
                    DonationHistoryId = b.DonationHistoryId,
                    ExpiryDate = b.ExpiryDate,
                    Status = b.Status
                })
                .OrderBy(b => b.BloodTypeName)
                .ThenBy(b => b.ExpiryDate)
                .ToListAsync();

            return bloodBanks;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in GetAllBloodBankAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<BloodBankDTO> ExpireBloodAsync(ExpireBloodRequestDTO request)
    {
        try
        {
            // Tìm bản ghi máu theo ID
            var blood = await _context.BloodBanks
                .FirstOrDefaultAsync(b => b.BloodTypeId == request.BloodTypeId);

            if (blood == null)
            {
                throw new InvalidOperationException($"Blood record with ID {request.BloodTypeId} not found");
            }

            // Kiểm tra nếu đã expired
            if (blood.Status == "Expired")
            {
                throw new InvalidOperationException($"Blood record with ID {request.BloodTypeId} is already expired");
            }

            // Cập nhật trạng thái
            blood.Status = "Expired";
            blood.ExpiryDate = DateOnly.FromDateTime(DateTime.Today); // Cập nhật ngày hết hạn thành ngày hiện tại

            await _context.SaveChangesAsync();

            // Trả về thông tin đã cập nhật
            return new BloodBankDTO
            {
                BloodTypeId = blood.BloodTypeId,
                BloodTypeName = blood.BloodTypeName,
                Unit = blood.Unit,
                ExpiryDate = blood.ExpiryDate,
                Status = blood.Status
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
            
            // Lấy tất cả máu khả dụng của loại máu yêu cầu, sắp xếp theo ngày hết hạn
            var availableBlood = await _context.BloodBanks
                .Where(b => b.BloodTypeName == request.BloodTypeName &&
                           b.Status != "Expired" &&
                           b.Unit > 0 &&
                           (b.ExpiryDate == null || b.ExpiryDate > today))
                .OrderBy(b => b.ExpiryDate)
                .ToListAsync();

            var totalAvailable = availableBlood.Sum(b => b.Unit ?? 0);
            var response = new UseBloodResponseDTO
            {
                BloodTypeName = request.BloodTypeName,
                Note = request.Note
            };

            var remainingUnits = request.RequiredUnits;
            var usedBloodIds = new List<int>();

            // Sử dụng máu từ các bản ghi hiện có
            foreach (var blood in availableBlood)
            {
                if (remainingUnits <= 0) break;

                var unitsToUse = Math.Min(blood.Unit ?? 0, remainingUnits);
                remainingUnits -= unitsToUse;

                // Tạo chi tiết sử dụng
                response.UsageDetails.Add(new BloodUsageDetailDTO
                {
                    BloodTypeId = blood.BloodTypeId,
                    UsedUnits = unitsToUse,
                    ExpiryDate = blood.ExpiryDate ?? today
                });

                // Cập nhật số lượng trong BloodBank
                blood.Unit -= unitsToUse;
                if (blood.Unit <= 0)
                {
                    usedBloodIds.Add(blood.BloodTypeId);
                }

                // Tạo bản ghi BloodMove
                var bloodMove = new BloodMove
                {
                    BloodTypeId = blood.BloodTypeId,
                    Unit = unitsToUse,
                    DateMove = today,
                    Note = request.Note
                };
                _context.BloodMoves.Add(bloodMove);
            }

            // Nếu vẫn chưa đủ số lượng, kiểm tra các loại máu tương thích
            if (remainingUnits > 0)
            {
                var compatibleBlood = await GetCompatibleBlood(request.BloodTypeName, remainingUnits, today);
                foreach (var blood in compatibleBlood)
                {
                    if (remainingUnits <= 0) break;

                    var unitsToUse = Math.Min(blood.Unit ?? 0, remainingUnits);
                    remainingUnits -= unitsToUse;

                    // Tạo chi tiết sử dụng
                    response.UsageDetails.Add(new BloodUsageDetailDTO
                    {
                        BloodTypeId = blood.BloodTypeId,
                        UsedUnits = unitsToUse,
                        ExpiryDate = blood.ExpiryDate ?? today
                    });

                    // Cập nhật số lượng trong BloodBank
                    blood.Unit -= unitsToUse;
                    if (blood.Unit <= 0)
                    {
                        usedBloodIds.Add(blood.BloodTypeId);
                    }

                    // Tạo bản ghi BloodMove
                    var bloodMove = new BloodMove
                    {
                        BloodTypeId = blood.BloodTypeId,
                        Unit = unitsToUse,
                        DateMove = today,
                        Note = $"{request.Note} (Compatible blood used for {request.BloodTypeName})"
                    };
                    _context.BloodMoves.Add(bloodMove);
                }
            }

            // Nếu vẫn không đủ số lượng
            if (remainingUnits > 0)
            {
                throw new InvalidOperationException($"Not enough blood units available. Required: {request.RequiredUnits}, Available: {request.RequiredUnits - remainingUnits}");
            }

            // Cập nhật tổng số đơn vị đã sử dụng
            response.TotalUsedUnits = request.RequiredUnits;

            await _context.SaveChangesAsync();

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in UseBloodAsync: {ex.Message}");
            throw;
        }
    }

    // Helper method để lấy máu tương thích
    private async Task<List<BloodBank>> GetCompatibleBlood(string bloodType, int requiredUnits, DateOnly today)
    {
        // Định nghĩa các loại máu tương thích
        var compatibleTypes = GetCompatibleBloodTypes(bloodType);

        return await _context.BloodBanks
            .Where(b => compatibleTypes.Contains(b.BloodTypeName) &&
                       b.Status != "Expired" &&
                       b.Unit > 0 &&
                       (b.ExpiryDate == null || b.ExpiryDate > today))
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    private List<string> GetCompatibleBloodTypes(string bloodType)
    {
        // Định nghĩa quy tắc tương thích máu
        switch (bloodType.ToUpper())
        {
            case "A+":
                return new List<string> { "A+", "A-", "O+", "O-" };
            case "A-":
                return new List<string> { "A-", "O-" };
            case "B+":
                return new List<string> { "B+", "B-", "O+", "O-" };
            case "B-":
                return new List<string> { "B-", "O-" };
            case "AB+":
                return new List<string> { "AB+", "AB-", "A+", "A-", "B+", "B-", "O+", "O-" };
            case "AB-":
                return new List<string> { "AB-", "A-", "B-", "O-" };
            case "O+":
                return new List<string> { "O+", "O-" };
            case "O-":
                return new List<string> { "O-" };
            default:
                return new List<string>();
        }
    }
}
