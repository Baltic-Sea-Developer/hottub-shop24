namespace HotTubShop.Web.Models;

public class OrderRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal NetTotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal GrossTotal { get; set; }
    public List<CartItem> Items { get; set; } = [];
}
