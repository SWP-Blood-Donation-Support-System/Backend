using BloodDonationAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Service
{
    public class CertificateService : ICertificateService
    {
        private readonly BloodDonationSystemContext _context;

        public CertificateService(BloodDonationSystemContext context)
        {
            _context = context;
        }
        public async Task<CertificateDto> GetCertificateAsync(int appointmentId)
        {
            var certificate = await _context.Certificates
                .Include(c => c.Appointment)
                .FirstOrDefaultAsync(c => c.AppointmentId == appointmentId);

            if (certificate == null) return null;

            return new CertificateDto
            {
                CertificateId = certificate.CertificateId,
                AppointmentId = certificate.AppointmentId,
                FullName = certificate.FullName,
                DateOfBirth = certificate.DateOfBirth,
                Address = certificate.Address,
                HospitalName = certificate.HospitalName,
                BloodAmount = certificate.BloodAmount,
                DonationDate = certificate.DonationDate,
                CertificateCode = certificate.CertificateCode,
                IssueDate = certificate.IssueDate,

            };
        }
    }
   
}
