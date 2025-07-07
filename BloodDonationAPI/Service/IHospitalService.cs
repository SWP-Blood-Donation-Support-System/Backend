using BloodDonationAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodDonationAPI.Service
{
    public interface IHospitalService
    {
        Task<List<Hospital>> GetAllHospitals();
        Task<Hospital?> GetHospitalById(int id);
        Task<Hospital> CreateHospital(Hospital hospital);
        Task<Hospital?> UpdateHospital(int id, Hospital updatedHospital);
        Task<bool> DeleteHospital(int id);
    }
} 