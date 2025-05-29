using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KıbrısApp3.Models;
using KıbrısApp3.DTO;
using KıbrısApp3.Services;
using KıbrısApp3.Data;
using Microsoft.AspNetCore.Authorization;

namespace KıbrısApp3.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        // ✅ 1. E-posta'ya 6 haneli kod gönder
        [HttpPost("start-register")]
        public async Task<IActionResult> StartRegister([FromBody] StartRegisterModel model)
        {
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
                return BadRequest(new { message = "Bu e-posta zaten kayıtlı." });

            var code = new Random().Next(100000, 999999).ToString();

            _context.EmailVerifications.Add(new EmailVerification
            {
                Email = model.Email,
                Code = code,
                ExpireAt = DateTime.UtcNow.AddMinutes(3)
            });

            await _context.SaveChangesAsync();

            var htmlBody = EmailTemplates.GenerateVerificationTemplate(
                "KIBRIS AL SAT Kayıt Doğrulama",
                "Kayıt işlemini tamamlamak için aşağıdaki kodu kullanınız:",
                code
            );

            var emailSender = new EmailSender(_configuration);
            await emailSender.SendEmailAsync(model.Email, "Kıbrıs Al Sat Kayıt Doğrulama", htmlBody);

            return Ok(new { message = "Kod gönderildi. Lütfen gelen kodu girin." });
        }


        // ✅ 2. Kod kontrolü (true / false döner)
        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto dto)
        {
            var codeRecord = await _context.EmailVerifications
                .Where(x => x.Email == dto.Email && x.Code == dto.Code && x.ExpireAt > DateTime.UtcNow)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            return Ok(codeRecord != null);
        }

        // ✅ 3. Kod doğruysa kullanıcıyı kaydet
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CompleteRegisterModel model)
        {
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
                return BadRequest(new { message = "Bu e-posta zaten kayıtlı." });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var token = GenerateJwtToken(user);
            return Ok(new { message = "Kayıt başarılı.", token });
        }

        // ✅ 4. Kullanıcı giriş yapar
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized();

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
                return Unauthorized();

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        [HttpPost("forgot-password/send-code")]
        public async Task<IActionResult> SendPasswordResetCode([FromBody] EmailOnlyDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Ok(new { message = "Kod gönderildi." }); // güvenlik için

            var code = new Random().Next(100000, 999999).ToString();

            _context.EmailVerifications.Add(new EmailVerification
            {
                Email = dto.Email,
                Code = code,
                ExpireAt = DateTime.UtcNow.AddMinutes(3)
            });

            await _context.SaveChangesAsync();

            var htmlBody = EmailTemplates.GenerateVerificationTemplate(
                "KIBRIS AL SAT Şifre Sıfırlama",
                "Şifrenizi sıfırlamak için aşağıdaki kodu kullanınız:",
                code
            );

            var emailSender = new EmailSender(_configuration);
            await emailSender.SendEmailAsync(dto.Email, "Kıbrıs Al Sat Şifre Sıfırlama", htmlBody);

            return Ok(new { message = "Kod gönderildi. Lütfen e-postanızı kontrol edin." });
        }


        [HttpPost("forgot-password/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { message = "Kullanıcı bulunamadı." });

            var codeMatch = await _context.EmailVerifications
                .Where(x => x.Email == dto.Email && x.Code == dto.Code && x.ExpireAt > DateTime.UtcNow)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (codeMatch == null)
                return BadRequest(new { message = "Kod geçersiz veya süresi dolmuş." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Şifre başarıyla sıfırlandı." });
        }

        [HttpPost("delete-account/send-code")]
        [Authorize]
        public async Task<IActionResult> SendDeleteAccountCode()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var code = new Random().Next(100000, 999999).ToString();

            _context.EmailVerifications.Add(new EmailVerification
            {
                Email = user.Email,
                Code = code,
                ExpireAt = DateTime.UtcNow.AddMinutes(3)
            });

            await _context.SaveChangesAsync();

            var htmlBody = EmailTemplates.GenerateVerificationTemplate(
                "KIBRIS AL SAT Hesap Silme Onayı",
                "Hesabınızı silmek için aşağıdaki kodu kullanınız:",
                code
            );

            var sender = new EmailSender(_configuration);
            await sender.SendEmailAsync(user.Email, "Kıbrıs Al Sat Hesap Silme", htmlBody);

            return Ok(new { message = "Onay kodu gönderildi. Lütfen e-posta kutunuzu kontrol edin." });
        }





        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccountWithCode([FromBody] DeleteAccountWithCodeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var codeMatch = await _context.EmailVerifications
                .Where(x => x.Email == user.Email && x.Code == dto.Code && x.ExpireAt > DateTime.UtcNow)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (codeMatch == null)
                return BadRequest(new { message = "Kod geçersiz veya süresi dolmuş." });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Hesap başarıyla silindi." });
        }



        // ✅ JWT üretici
        private string GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim("name", user.FullName ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
