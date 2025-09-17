using System;
using TenantFlow.Services;
using TenantFlow.TenantFlow.Web.Services;

namespace TenantFlow.TenantFlow.Web.Services
{
    public static class ReceiptValidator
    {
        public static bool IsMatch(TngReceipt ocr, GmailReceipt email)
        {
            return ocr.ReferenceNumber == email.ReferenceNumber
                && ocr.Amount == email.Amount
                && ocr.Date == email.DateTime;
        }
    }
}
