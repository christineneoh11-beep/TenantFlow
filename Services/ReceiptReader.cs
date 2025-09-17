using System.Net.Http.Headers;

namespace TenantFlow.TenantFlow.Web.Services
{
    public static class OcrSpaceReader
    {
        private static readonly string ApiKey = "K83512328388957";
        private static readonly string ApiUrl = "https://api.ocr.space/parse/image";

        public static async Task<string> ExtractTextFromImageAsync(string imagePath)
        {
            using var client = new HttpClient();
            using var form = new MultipartFormDataContent();

            form.Headers.ContentType.MediaType = "multipart/form-data";
            form.Add(new StringContent(ApiKey), "apikey");
            form.Add(new StringContent("chs"), "language");
            form.Add(new StringContent("true"), "isOverlayRequired");
            form.Add(new StringContent("2"), "OCREngine");

            var imageContent = new ByteArrayContent(await File.ReadAllBytesAsync(imagePath));
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            form.Add(imageContent, "file", Path.GetFileName(imagePath));

            var response = await client.PostAsync(ApiUrl, form);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            return jsonResponse;
        }
    }
}
