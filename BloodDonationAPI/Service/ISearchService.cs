using BloodDonationAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodDonationAPI.Service
{    public interface ISearchService
    {
        Task<IEnumerable<object>> FindDonorsByBloodType(string bloodType);
        Task<IEnumerable<object>> FindEmergenciesByBloodType(string bloodType);
    }
}