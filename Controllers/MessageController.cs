using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using TenantFlow.TenantFlow.Web.Services;
using System.Text.Json;


namespace TenantFlow.TenantFlow.Web.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessageController(
        ILogger<MessageController> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
        : ControllerBase
    {
        private readonly ILogger<MessageController> _logger = logger;

        [HttpPost]
        public async Task<IActionResult> ReceiveWhatsAppMessage(
                [FromForm] string? body,
                [FromForm(Name = "NumMedia")] string numMediaStr,
                [FromForm(Name = "MediaUrl0")] string? mediaUrl,
                [FromForm(Name = "MediaContentType0")] string? mediaContentType,
                [FromForm(Name = "From")] string from)
            {
                Console.WriteLine($"📩 Message received from: {from}");
            Console.WriteLine($"Message body: {body}");
            var reply = "";

            if (body == "run settlement")
            {
                await ExcelReader.ReadExcel();
            }

            if (!int.TryParse(numMediaStr, out int numMedia) || numMedia <= 0 || string.IsNullOrEmpty(mediaUrl))
                return Ok(reply);

            if (string.IsNullOrWhiteSpace(body))
            {
                reply = "📸 Please resend your receipt together with your unit number in the same message (e.g. send image + text at once).\n请重新发送收据，并在同一条消息中附上您的房号 (例如：B-17-13)";
                return Ok(reply);
            }

            try
            {
                var client = httpClientFactory.CreateClient();

                var sid = configuration["Twilio:AccountSid"];
                var token = configuration["Twilio:AuthToken"];
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{sid}:{token}");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var imageBytes = await client.GetByteArrayAsync(mediaUrl);

                var savePath = Path.Combine("Downloads", $"whatsapp_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                Directory.CreateDirectory("Downloads");
                await System.IO.File.WriteAllBytesAsync(savePath, imageBytes);

                var jsonResponse = await OcrSpaceReader.ExtractTextFromImageAsync(savePath);
                var tngobj = OcrHelper.ExtractAmountAndUnitFromOcrJson(jsonResponse);
                Console.WriteLine(JsonSerializer.Serialize(tngobj, new JsonSerializerOptions { WriteIndented = true }));

                if (tngobj?.Date != null)
                {
                    var folderName = tngobj.Date.Value.ToString("dd-MM-yyyy");
                    var folderPath = Path.Combine("Downloads", folderName);
                    var unitNo = ExcelReader.GetUnitByPhoneNumber(from);
                    Directory.CreateDirectory(folderPath);

                    var fileName = !string.IsNullOrEmpty(unitNo)
                        ? $"{unitNo}.jpg"
                        : $"whatsapp_{DateTime.Now:HHmmss}.jpg";

                    var finalImagePath = Path.Combine(folderPath, fileName);
                    System.IO.File.Move(savePath, finalImagePath);

                    Console.WriteLine(tngobj.Amount);
                }

                //var gmailObj = await GmailReader.ReadEmailsAsync();

                // if (tngobj != null && gmailObj != null && ReceiptValidator.IsMatch(tngobj, gmailObj))
                // {
                //     reply = $"Thank you for your payment of RM {tngobj.Amount:0.00}";
                //     //reply = $"RM {tngobj.Amount:0.00}"
                // }

                //reply = $"Thank you for your payment of RM {tngobj.Amount:0.00} for unit {tngobj.UnitNo}.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download image: {ex.Message}");
            }

            return Ok(reply);
        }
    }
}



