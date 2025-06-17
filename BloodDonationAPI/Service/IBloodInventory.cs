using BloodDonationAPI.DTO;
using BloodDonationAPI.DTO.BloodInventory;
using BloodDonationAPI.DTOs.BloodInventory;

namespace BloodDonationAPI.Service;

public interface IBloodInventoryService
{
    Task<List<BloodBankDTO>> GetBloodBankAsync();
    Task<BloodInventoryResponseDTO> GetBloodInventoryAsync();
    Task<BloodBankDTO> AddBloodInventoryAsync(AddBloodBankDto request);
    Task<BloodBankDTO> ExpireBloodAsync(ExpireBloodRequestDTO request);
    Task<UseBloodResponseDTO> UseBloodAsync(UseBloodRequestDTO request);
}
