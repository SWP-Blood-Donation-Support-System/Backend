using BloodDonationAPI.DTO;

namespace BloodDonationAPI.Service
{
    public interface ICertificateService
    {
        Task<CertificateDto?> GetCertificateAsync(int appointmentId);
    }
}
