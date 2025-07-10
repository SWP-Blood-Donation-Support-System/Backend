using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodDonationAPI.Service
{
    public class HospitalService : IHospitalService
    {
        private readonly BloodDonationSystemContext _context;
        public HospitalService(BloodDonationSystemContext context)
        {
            _context = context;
        }

        public async Task<List<Hospital>> GetAllHospitals()
        {
            return await _context.Hospitals.ToListAsync();
        }

        public async Task<Hospital?> GetHospitalById(int id)
        {
            return await _context.Hospitals.FindAsync(id);
        }

        public async Task<Hospital> CreateHospital(Hospital hospital)
        {
            _context.Hospitals.Add(hospital);
            await _context.SaveChangesAsync();
            return hospital;
        }

        public async Task<Hospital?> UpdateHospital(int id, Hospital updatedHospital)
        {
            var hospital = await _context.Hospitals.FindAsync(id);
            if (hospital == null) return null;
            hospital.HospitalName = updatedHospital.HospitalName;
            hospital.HospitalAddress = updatedHospital.HospitalAddress;
            hospital.HospitalImage = updatedHospital.HospitalImage;
            hospital.HospitalPhone = updatedHospital.HospitalPhone;
            await _context.SaveChangesAsync();
            return hospital;
        }

        public async Task<bool> DeleteHospital(int id)
        {
            var hospital = await _context.Hospitals.FindAsync(id);
            if (hospital == null) return false;
            _context.Hospitals.Remove(hospital);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 