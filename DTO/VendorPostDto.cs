namespace APBD_TEST_TEMPLATE.DTO;

public class VendorPostDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<VendorProductPostDto> Products { get; set; } = [];
}

public class VendorProductPostDto
{
    public int Id { get; set; }
    public int Amount { get; set; }
    public decimal PricePerUnit { get; set; }
}