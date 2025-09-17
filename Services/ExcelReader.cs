using System.Globalization;
using ClosedXML.Excel;
using TenantFlow.Services;
using System.Text.Json;


namespace TenantFlow.TenantFlow.Web.Services
{
    public static class ExcelReader
    {
        public static async Task ReadExcel()
        {
            var results = new List<SettlementRows>();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads", "MDSDO_P_20250702_EP745124.xlsx");

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.First();
            int row = 11;

            while (true)
            {
                var noCell = worksheet.Cell(row, 1);
                var settlementDateCell = worksheet.Cell(row, 2);
                var transactionDateCell = worksheet.Cell(row, 3);
                var amountCell = worksheet.Cell(row, 9);

                if (noCell.IsEmpty() || string.IsNullOrWhiteSpace(noCell.GetString()))
                    break;

                if (DateTime.TryParseExact(settlementDateCell.GetString(), "dd/MM/yyyy", null, DateTimeStyles.None, out var settlementDate) &&
                    DateTime.TryParseExact(transactionDateCell.GetString(), "dd/MM/yyyy HH:mm:ss", null,DateTimeStyles.None, out var transactionTime) &&
                    decimal.TryParse(amountCell.GetString(), null, out var amount))
                {
                    results.Add(new SettlementRows
                    {
                        RowNumber = row,
                        SettlementDate = settlementDate,
                        TransactionDate = transactionTime,
                        Amount = amount
                    });
                }

                row++;
            }

            await MatchExcelWithReceiptImages(results);

            worksheet.Cell(10, 15).Value = "Verified";

            foreach (var item in results)
            {
                worksheet.Cell(item.RowNumber, 15).Value = item.Matched ? "Yes" : "No";
            }

            workbook.Save();
        }

        private static async Task MatchExcelWithReceiptImages(List<SettlementRows> excelData)
        {
            Console.WriteLine("hello there");
            var firstSettlementDate = excelData.First().SettlementDate;
            var folderName = firstSettlementDate.AddDays(-1).ToString("dd-MM-yyyy");

            var imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads", folderName);

            if (!Directory.Exists(imageFolderPath))
            {
                Console.WriteLine("Image folder does not exist.");
                return;
            }

            var imageFiles = Directory.GetFiles(imageFolderPath, "*.jpg");

            foreach (var imagePath in imageFiles)
            {
                var jsonResponse = await OcrSpaceReader.ExtractTextFromImageAsync(imagePath);
                var receiptObject = OcrHelper.ExtractAmountAndUnitFromOcrJson(jsonResponse);

                var matchedRow = excelData.FirstOrDefault(row =>
                    Math.Abs(row.Amount - receiptObject.Amount) < 0.01m);

                if (matchedRow != null)
                {
                    matchedRow.Matched = true;
                }
            }

            Console.WriteLine("\n=== Match Summary ===");
            foreach (var row in excelData)
            {
                Console.WriteLine($"Date: {row.TransactionDate}, Amount: {row.Amount}, Match: {row.Matched}");
            }
        }

        public static string? GetUnitByPhoneNumber(string phoneNumber)
        {
            var senderNumber = phoneNumber?.Replace("whatsapp:+", "");
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TenantDirectory", "tenant_data.xlsx");
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Tenant data file not found.");
                return null;
            }

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var contactNumber = row.Cell(8).GetString().Trim();
                if (!string.IsNullOrEmpty(contactNumber) && contactNumber == senderNumber)
                {
                    return row.Cell(3).GetString();
                }
            }

            return null;
        }

    }
}


