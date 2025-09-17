namespace TenantFlow.Services
{
    public class TngReceipt
    {
        public string? ReferenceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? UnitNo { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
    }

    public class GmailReceipt
    {
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime? DateTime { get; set; }
        public decimal Amount { get; set; }
    }

    public class Receipt
    {
        public decimal Amount { get; set; }
        public DateTime? Date { get; set; }
    }

    public class SettlementRows
    {
        public int RowNumber { get; set; }
        public DateTime SettlementDate { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public bool Matched { get; set; }
    }
}
