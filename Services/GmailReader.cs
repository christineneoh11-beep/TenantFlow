using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using TenantFlow.Services;

namespace TenantFlow.TenantFlow.Web.Services
{
    class GmailReader
    {
        static readonly string[] _scopes = { GmailService.Scope.GmailReadonly };
        static string ApplicationName = "TenantFlow Gmail Reader";

        public static async Task<GmailReceipt?> ReadEmailsAsync()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    _scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }

            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var request = service.Users.Messages.List("me");
            request.LabelIds = "INBOX";
            //request.Q = "from:noreply@universal-trustee.com subject:'Payment Advice' 'Customer Reference No.'";

            request.Q = "subject:'Payment Advice -'";
            request.MaxResults = 1;

            var response = await request.ExecuteAsync();

            if (response.Messages != null)
            {
                foreach (var messageItem in response.Messages)
                {
                    var message = await service.Users.Messages.Get("me", messageItem.Id).ExecuteAsync();
                    //Console.WriteLine($"- {message.Snippet}");

                    var body = GetPlainTextBody(message);

                    if (!string.IsNullOrEmpty(body))
                    {
                        var referenceMatch = Regex.Match(body, @"Customer Reference No\. ?: ([A-Z0-9]+)");
                        var amountMatch = Regex.Match(body, @"Amount: MYR ?([\d,.]+)");
                        var dateMatch = Regex.Match(body, @"Date/Time: (\d{2}-[A-Za-z]{3}-\d{4} \d{2}:\d{2}:\d{2})");

                        var reference = referenceMatch.Success ? referenceMatch.Groups[1].Value.Trim() : string.Empty;
                        var amountStr = amountMatch.Success ? amountMatch.Groups[1].Value.Trim().Replace(",", "") : "0";
                        var dateStr = dateMatch.Success ? dateMatch.Groups[1].Value.Trim() : null;

                        decimal.TryParse(amountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount);
                        DateTime? dateTime = null;
                        if (DateTime.TryParseExact(dateStr, "dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                        {
                            dateTime = parsedDate;
                        }

                        return new GmailReceipt
                        {
                            ReferenceNumber = reference,
                            Amount = amount,
                            DateTime = dateTime
                        };

                        //Console.WriteLine(JsonSerializer.Serialize(gmailReceipt, new JsonSerializerOptions { WriteIndented = true }));
                    }
                }
            }
            
            Console.WriteLine("No messages found.");
            return null;
        }

        private static string GetPlainTextBody(Message message)
        {
            if (message.Payload.Body?.Data != null)
            {
                return DecodeBase64(message.Payload.Body.Data);
            }

            if (message.Payload.Parts != null)
            {
                foreach (var part in message.Payload.Parts)
                {
                    if (part.MimeType == "text/plain" && part.Body?.Data != null)
                    {
                        return DecodeBase64(part.Body.Data);
                    }
                }
            }

            return null;
        }

        private static string DecodeBase64(string base64)
        {
            base64 = base64.Replace("-", "+").Replace("_", "/");
            byte[] data = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(data);
        }

    }
}
