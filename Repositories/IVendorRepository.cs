using APBD_TEST_TEMPLATE.DTO;
namespace APBD_TEST_TEMPLATE.Repositories;

public interface IVendorRepository
{
    Task<IEnumerable<VendorGetDto>> GetVendorsAsync(string? name);
    Task<bool> VendorExistsAsync(string code);
    Task<bool> ProductExistsAsync(int productId);
    Task AddVendorAsync(VendorPostDto dto);
}