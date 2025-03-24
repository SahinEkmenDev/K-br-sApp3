using Microsoft.AspNetCore.Mvc;
using KıbrısApp3.DTO;


namespace KıbrısApp3.Controllers
{
    [Route("api/images")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        [HttpPost("upload-base64")]
        public IActionResult UploadBase64Image([FromBody] Base64ImageDto dto)
        {
            if (string.IsNullOrEmpty(dto.Base64))
                return BadRequest(new { message = "Resim verisi boş!" });

            try
            {
                var base64Data = dto.Base64;

                // Örn: "data:image/jpeg;base64,/9j/4AAQSk..." → bunu ayır
                var parts = base64Data.Split(',');
                if (parts.Length != 2)
                    return BadRequest(new { message = "Base64 formatı hatalı!" });

                var bytes = Convert.FromBase64String(parts[1]);

                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileName = Guid.NewGuid().ToString() + ".jpg"; // uzantı sabit olabilir
                var filePath = Path.Combine(uploadsPath, fileName);

                System.IO.File.WriteAllBytes(filePath, bytes);

                var imageUrl = $"/uploads/{fileName}";

                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Resim yüklenemedi", error = ex.Message });
            }
        }

    }
}
