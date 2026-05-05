using Microsoft.AspNetCore.Mvc;
using APBD_TEST_TEMPLATE.DTO;
using APBD_TEST_TEMPLATE.Repositories;

namespace APBD_TEST_TEMPLATE.Controllers;

[ApiController]
[Route("api/vendors")]
public class VendorsControl(IVendorRepository vendorRepository) : ControllerBase
{
    // GET /api/vendors
    // GET /api/vendors?name=Ama
    [HttpGet]
    public async Task<IActionResult> GetVendors([FromQuery] string? name)
    {
        var vendors = await vendorRepository.GetVendorsAsync(name);
        return Ok(vendors);
    }

    // POST /api/vendors
    [HttpPost]
    public async Task<IActionResult> CreateVendor([FromBody] VendorPostDto dto)
    {
        if (await vendorRepository.VendorExistsAsync(dto.Code))
            return Conflict($"Vendor with code '{dto.Code}' already exists.");

        foreach (var product in dto.Products)
        {
            if (!await vendorRepository.ProductExistsAsync(product.Id))
                return NotFound($"Product with id '{product.Id}' does not exist.");
        }

        await vendorRepository.AddVendorAsync(dto);

        return Created($"api/vendors/{dto.Code}", null);
    }
}
