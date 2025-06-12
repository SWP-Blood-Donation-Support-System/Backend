using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BloodDonationAPI.Service
{
    public class SearchService : ISearchService
    {
        private readonly BloodDonationSystemContext _context;

        public SearchService(BloodDonationSystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> FindDonorsByBloodType(string bloodType)
        {
            if (string.IsNullOrEmpty(bloodType))
                throw new ArgumentException("Blood type is required", nameof(bloodType));

            var normalizedBloodType = bloodType.ToUpper().Trim();
            var compatibleDonorTypes = GetCompatibleDonorTypes()[normalizedBloodType];
            
            var donors = await _context.Users.ToListAsync();
            
            var compatibleDonors = donors
                .Where(u => u.BloodType != null && compatibleDonorTypes.Contains(u.BloodType))
                .Select(u => new {
                    Username = u.Username,
                    FullName = u.FullName, 
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    DateOfBirth = u.DateOfBirth,
                    Gender = u.Gender,
                    BloodType = u.BloodType,
                    ProfileStatus = u.ProfileStatus,
                    Role = u.Role
                })
                .OrderBy(u => u.Address)
                .ToList();
                
            return compatibleDonors;
        }

        public async Task<IEnumerable<object>> FindEmergenciesByBloodType(string bloodType)
        {
            if (string.IsNullOrEmpty(bloodType))
                throw new ArgumentException("Blood type is required", nameof(bloodType));
                
            var normalizedBloodType = bloodType.ToUpper().Trim();
            
            // Get all emergencies
            var allEmergencies = await _context.Emergencies.ToListAsync();
            
            // Filter by blood type
            var matchingEmergencies = allEmergencies
                .Where(e => e.BloodType == normalizedBloodType)
                .Select(e => new {
                    e.Username,
                    EmergencyDate = e.EmergencyDate,
                    BloodType = e.BloodType,
                    EmergencyStatus = e.EmergencyStatus,
                    EmergencyNote = e.EmergencyNote,
                    RequiredUnits = e.RequiredUnits,
                    HospitalId = e.HospitalId
                })
                .ToList();

            // Get hospital information for each emergency
            var hospitals = await _context.Hospitals.ToListAsync();
            
            var result = matchingEmergencies.Select(e => {
                var hospital = hospitals.FirstOrDefault(h => h.HospitalId == e.HospitalId);
                return new {
                    e.Username,
                    e.EmergencyDate,
                    e.BloodType,
                    e.EmergencyStatus,
                    e.EmergencyNote,
                    e.RequiredUnits,
                    e.HospitalId,
                    HospitalName = hospital?.HospitalName,
                    HospitalAddress = hospital?.HospitalAddress,
                    HospitalPhone = hospital?.HospitalPhone
                };
            }).ToList<object>();
            
            return result;
        }
          // Hospital search functionality removed as requested

        /// <summary>
        /// Lấy danh sách các nhóm máu có thể hiến cho từng nhóm máu
        /// </summary>
        private Dictionary<string, List<string>> GetCompatibleDonorTypes()
        {
            return new Dictionary<string, List<string>>
            {
                { "O-", new List<string> { "O-" } },
                { "O+", new List<string> { "O-", "O+" } },
                { "A-", new List<string> { "O-", "A-" } },
                { "A+", new List<string> { "O-", "O+", "A-", "A+" } },
                { "B-", new List<string> { "O-", "B-" } },
                { "B+", new List<string> { "O-", "O+", "B-", "B+" } },
                { "AB-", new List<string> { "O-", "A-", "B-", "AB-" } },
                { "AB+", new List<string> { "O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+" } }
            };
        }
    }
}