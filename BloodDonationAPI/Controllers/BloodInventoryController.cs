using BloodDonationAPI.DTO.BloodInventory;
using BloodDonationAPI.DTOs.BloodInventory;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace BloodDonationAPI.Controllers;

[ApiController]
[Route("api/blood-inventory")]
public class BloodInventoryController : ControllerBase
{
    private readonly IBloodInventoryService _bloodInventoryService;
    private readonly ILogger<BloodInventoryController> _logger;

    public BloodInventoryController(IBloodInventoryService bloodInventoryService, ILogger<BloodInventoryController> logger)
    {
        _bloodInventoryService = bloodInventoryService;
        _logger = logger;
    }
    /// <summary>
    /// Lấy tổng lượng máu trong kho
    /// </summary>
    [Authorize(Roles = "Staff,Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(BloodInventoryResponseDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<BloodInventoryResponseDTO>> GetBloodInventory()
    {
        var result = await _bloodInventoryService.GetBloodInventoryAsync();
        return Ok(result);
    }
    /// <summary>
    /// Lấy chi tiết lượng máu trong kho
    /// </summary>
    [Authorize(Roles = "Staff,Admin")]
    [HttpGet("blood-bank")]
    public async Task<ActionResult<List<BloodBankDTO>>> GetAllBloodBank()
    {
        try
        {
            var result = await _bloodInventoryService.GetAllBloodBankAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving blood bank data." });
        }
    }
    /// <summary>
    /// Thêm lượng máu ngoài vào kho
    /// </summary>
    [Authorize(Roles = "Staff,Admin")]
    [HttpPatch]
    [ProducesResponseType(typeof(BloodBankDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BloodBankDTO>> UpdateInventory([FromBody] UpdateBloodInventoryRequestDTO request)
    {
        try
        {
            var result = await _bloodInventoryService.UpdateBloodInventoryAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while adding blood to inventory." });
        }
    }
    /// <summary>
    /// Chuyển lượng máu đến các bệnh viện hoặc người cần máu 
    /// </summary>
    [Authorize(Roles = "Staff,Admin")]
    [HttpPost("use")]
    public async Task<ActionResult<UseBloodResponseDTO>> UseBlood([FromBody] UseBloodRequestDTO request)
    {
        try
        {
            var result = await _bloodInventoryService.UseBloodAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing blood usage." });
        }
    }
    /// <summary>
    /// Đánh dấu hết hạn cho máu
    /// </summary>
    [Authorize(Roles = "Staff,Admin")]
    [HttpPatch("expire")]
    public async Task<ActionResult<BloodBankDTO>> ExpireBlood([FromBody] ExpireBloodRequestDTO request)
    {
        try
        {
            var result = await _bloodInventoryService.ExpireBloodAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while marking blood as expired." });
        }
    }
}
