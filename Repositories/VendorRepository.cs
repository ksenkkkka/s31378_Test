using APBD_TEST_TEMPLATE.DTO;


    using Microsoft.Data.SqlClient;

    using APBD_TEST_TEMPLATE.Repositories;

    using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;

    namespace APBD_TEST_TEMPLATE.Repositories.APBD_TEST_TEMPLATE.Repositories
{
    public class SqlConnection : IAsyncDisposable
    {
        public SqlConnection(string connectionString)
        {
            throw new NotImplementedException();
        }

        private void ReleaseUnmanagedResources()
        {
           
        }

        public async ValueTask DisposeAsync()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~SqlConnection()
        {
            ReleaseUnmanagedResources();
        }
    }
}


public class VendorRepository(IConfiguration configuration) : IVendorRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' not found.");

    public async Task<IEnumerable<VendorGetDto>> GetVendorsAsync(string? name)
    {
        const string sql = """
                           SELECT v.Code, v.Name, p.Id AS ProductId, p.Name AS ProductName,
                                  vp.Amount, vp.PricePerUnit
                           FROM Vendors v
                           LEFT JOIN VendorProducts vp ON v.Code = vp.VendorCode
                           LEFT JOIN Products       p  ON vp.ProductId = p.Id
                           WHERE (@name IS NULL OR v.Name LIKE '%' + @name + '%')
                           ORDER BY v.Code
                           """;
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        var vendors = new Dictionary<string, VendorGetDto>();

        while (await reader.ReadAsync())
        {
            var code = reader.GetString(0);

            if (!vendors.TryGetValue(code, out var vendor))
            {
                vendor = new VendorGetDto
                {
                    Code = code,
                    Name = reader.GetString(1)
                };
                vendors[code] = vendor;
            }

            if (!reader.IsDBNull(2))
            {
                vendor.Products.Add(new VendorProductGetDto
                {
                    Id           = reader.GetInt32(2),
                    Name         = reader.GetString(3),
                    Amount       = reader.GetInt32(4),
                    PricePerUnit = reader.GetDecimal(5)
                });
            }
        }

        return vendors.Values;
    }

    public async Task<bool> VendorExistsAsync(string code)
    {
        const string sql = "SELECT 1 FROM Vendors WHERE Code = @code";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@code", code);

        await connection.OpenAsync();
        return await command.ExecuteScalarAsync() is not null;
    }

    public async Task<bool> ProductExistsAsync(int productId)
    {
        const string sql = "SELECT 1 FROM Products WHERE Id = @id";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", productId);

        await connection.OpenAsync();
        return await command.ExecuteScalarAsync() is not null;
    }

    public async Task AddVendorAsync(VendorPostDto dto)
    {
        const string insertVendor = """
                                    INSERT INTO Vendors (Code, Name) VALUES (@code, @name)
                                    """;

        const string insertVendorProduct = """
                                           INSERT INTO VendorProducts (ProductId, VendorCode, Amount, PricePerUnit)
                                           VALUES (@productId, @vendorCode, @amount, @pricePerUnit)
                                           """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            await using var vendorCmd = new SqlCommand(insertVendor, connection, transaction);
            vendorCmd.Parameters.AddWithValue("@code", dto.Code);
            vendorCmd.Parameters.AddWithValue("@name", dto.Name);
            await vendorCmd.ExecuteNonQueryAsync();

            foreach (var product in dto.Products)
            {
                await using var productCmd = new SqlCommand(insertVendorProduct, connection, transaction);
                productCmd.Parameters.AddWithValue("@productId",    product.Id);
                productCmd.Parameters.AddWithValue("@vendorCode",   dto.Code);
                productCmd.Parameters.AddWithValue("@amount",       product.Amount);
                productCmd.Parameters.AddWithValue("@pricePerUnit", product.PricePerUnit);
                await productCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
