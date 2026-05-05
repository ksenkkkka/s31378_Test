namespace APBD_TEST_TEMPLATE.DTO;

public class VendorGetDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<VendorProductGetDto> Products { get; set; } = [];
}

public class VendorProductGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Amount { get; set; }
    public decimal PricePerUnit { get; set; }
}