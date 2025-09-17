using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using TenantFlow.Services;

namespace TenantFlow.TenantFlow.Web.Services
{
    public class OcrResponse
    {
        public ParsedResult[]? ParsedResults { get; set; }
    }

    public class ParsedResult
    {
        public string? ParsedText { get; set; }
    }

    enum PaymentMethod
    {
        Unknown,
        TNG,
        WeChatPay
    }

    public static class OcrHelper
    {
        public static TngReceipt? ExtractAmountAndUnitFromOcrJson(string json)
        {
            var ocrResponse = JsonSerializer.Deserialize<OcrResponse>(json);

            if (ocrResponse?.ParsedResults != null && ocrResponse.ParsedResults.Length > 0)
            {
                var parsedText = ocrResponse.ParsedResults[0].ParsedText ?? string.Empty;
                var lines = parsedText.Split('\n');

                if (parsedText.Contains("TNG", StringComparison.OrdinalIgnoreCase))
                {
                    return ExtractFromTngReceipt(lines);
                }
                else if (parsedText.Contains("WeChatPay", StringComparison.OrdinalIgnoreCase) ||
                         parsedText.Contains("WeChat", StringComparison.OrdinalIgnoreCase))
                {
                    return ExtractFromWeChatReceipt(lines);
                }
            }

            return null;
        }

        private static TngReceipt? ExtractFromTngReceipt(string[] lines)
        {
            var unitNumberPattern = new Regex(@"^[A-Za-z]+-\d+-\d+$");

            string? referenceNumber = null;
            string? amountLine = null;
            string? unitNumberLine = null;
            DateTime? date = null;

            for (int i = 0; i < lines.Length; i++)
            {
                var trimmedLine = lines[i].Trim();

                if (referenceNumber == null && trimmedLine.EndsWith("TNG") && i + 1 < lines.Length)
                {
                    var nextLine = lines[i + 1].Trim();
                    if (!string.IsNullOrEmpty(nextLine) && Regex.IsMatch(nextLine, @"^[A-Z0-9]+$"))
                    {
                        referenceNumber = trimmedLine + nextLine;
                    }
                }

                if (amountLine == null && trimmedLine.Contains("RM"))
                {
                    amountLine = trimmedLine;
                }

                if (unitNumberLine == null && unitNumberPattern.IsMatch(trimmedLine))
                {
                    unitNumberLine = trimmedLine;
                }

                if (date == null && Regex.IsMatch(trimmedLine, @"\d{2}/\d{2}/\d{4} \d{2}:\d{2}:\d{2}"))
                {
                    if (DateTime.TryParseExact(trimmedLine, "dd/MM/yyyy HH:mm:ss", null, DateTimeStyles.None, out var parsedDate))
                    {
                        date = parsedDate;
                    }
                }

                if (amountLine != null && unitNumberLine != null && referenceNumber != null)
                {
                    break;
                }
            }

            if (amountLine == null)
            {
                return null;
            }

            decimal amount = 0;
            var amountString = amountLine.Replace("RM", "", StringComparison.OrdinalIgnoreCase)
                                         .Trim()
                                         .Replace(",", "");

            if (!decimal.TryParse(amountString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount))
            {
                amount = 0;
            }

            return new TngReceipt
            {
                ReferenceNumber = referenceNumber,
                Amount = amount,
                UnitNo = unitNumberLine ?? string.Empty,
                Date = date
            };
        }

        private static TngReceipt? ExtractFromWeChatReceipt(string[] lines)
        {
            decimal amount = 0;
            DateTime? date = null;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Extract amount
                if (amount == 0 && trimmedLine.StartsWith("MYR", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 && decimal.TryParse(parts[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedAmount))
                    {
                        amount = parsedAmount;
                    }
                }

                // Extract date
                if (date == null && Regex.IsMatch(trimmedLine, @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}"))
                {
                    if (DateTime.TryParseExact(trimmedLine, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        date = parsedDate;
                    }
                }

                if (amount > 0 && date != null)
                {
                    break;
                }
            }

            if (amount == 0 || date == null)
            {
                return null;
            }

            return new TngReceipt
            {
                Amount = amount,
                Date = date
            };
        }


        // public static Receipt? GetAmountAndDateTime(string json)
        // {
        //     var ocrResponse = JsonSerializer.Deserialize<OcrResponse>(json);
        //
        //     if (ocrResponse?.ParsedResults != null && ocrResponse.ParsedResults.Length > 0)
        //     {
        //         var parsedText = ocrResponse.ParsedResults[0].ParsedText ?? string.Empty;
        //         var lines = parsedText.Split('\n');
        //
        //         string? amountLine = null;
        //         DateTime? date = null;
        //
        //         foreach (var line in lines)
        //         {
        //             var trimmedLine = line.Trim();
        //
        //             if (amountLine == null && trimmedLine.Contains("RM"))
        //             {
        //                 amountLine = trimmedLine;
        //             }
        //
        //             if (date == null && Regex.IsMatch(trimmedLine, @"\d{2}/\d{2}/\d{4} \d{2}:\d{2}:\d{2}"))
        //             {
        //                 if (DateTime.TryParseExact(trimmedLine, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        //                 {
        //                     date = parsedDate;
        //                 }
        //             }
        //
        //             if (amountLine != null && date != null)
        //                 break;
        //         }
        //
        //         if (amountLine == null)
        //             return null;
        //
        //         var amountString = amountLine.Replace("RM", "", StringComparison.OrdinalIgnoreCase)
        //                                     .Trim()
        //                                     .Replace(",", "");
        //
        //         if (!decimal.TryParse(amountString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount))
        //         {
        //             amount = 0;
        //         }
        //
        //         return new Receipt
        //         {
        //             Amount = amount,
        //             Date = date
        //         };
        //     }
        //
        //     return null;
        // }

    }
}
