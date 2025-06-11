using BloodDonationAPI.DTO.BloodInventory;
using BloodDonationAPI.DTOs.BloodInventory;

namespace BloodDonationAPI.Service;

public interface IBloodInventoryService
{
    Task<List<BloodBankDTO>> GetAllBloodBankAsync();
    Task<BloodInventoryResponseDTO> GetBloodInventoryAsync();
    Task<BloodBankDTO> UpdateBloodInventoryAsync(UpdateBloodInventoryRequestDTO request);
    Task<BloodBankDTO> ExpireBloodAsync(ExpireBloodRequestDTO request);
    Task<UseBloodResponseDTO> UseBloodAsync(UseBloodRequestDTO request);
}
