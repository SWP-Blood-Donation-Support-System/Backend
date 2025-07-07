using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateService _certificateService;


        public CertificateController(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        [HttpGet("{appointmentId}")]
        public async Task<IActionResult> GetCertificate(int appointmentId)
        {
            var certificate = await _certificateService.GetCertificateAsync(appointmentId);
            if (certificate == null)
            {
                return NotFound(new { Message = "Certificate not found." });
            }
            return Ok(certificate);
        }

        [HttpGet("{appointmentId}/pdf")]
        public async Task<IActionResult> GetCertificatePdf(int appointmentId)
        {
            var certificate = await _certificateService.GetCertificateAsync(appointmentId);
            if (certificate == null)
            {
                return NotFound(new { Message = "Certificate not found." });
            }
            var pdfBytes = _certificateService.GenerateCertificatePdf(certificate);
           var fileName = $"Certificate_{certificate.FullName}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
