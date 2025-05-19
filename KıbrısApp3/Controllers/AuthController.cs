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

            var verification = new EmailVerification
            {
                Email = model.Email,
                Code = code,
                ExpireAt = DateTime.UtcNow.AddSeconds(180)
            };

            _context.EmailVerifications.Add(verification);
            await _context.SaveChangesAsync();

            var emailSender = new EmailSender(_configuration);
            string htmlBody = $@"
<html>
<body style='font-family: Arial; background-color: #f7f7f7; padding: 20px;'>
  <div style='background-color: white; max-width: 600px; margin: auto; padding: 20px; border-radius: 10px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);'>
    <h2 style='color: #333;'>KıbrısApp Kayıt Doğrulama</h2>
    <p>Merhaba,</p>
    <p>Kayıt işlemini tamamlamak için aşağıdaki kodu kullanınız:</p>
    <div style='font-size: 28px; font-weight: bold; color: #2196F3; margin: 20px 0;'>{code}</div>
    <p>⚠️ Bu kod <strong>3 dakika</strong> içinde geçerliliğini yitirir.</p>
    <hr style='margin-top: 30px; border: none; border-top: 1px solid #ccc;'/>
    <p style='font-size: 12px; color: #888;'>Bu e-posta KıbrısApp’e kayıt olmaya çalışan biri tarafından gönderilmiştir.</p>
  </div>
</body>
</html>";


            await emailSender.SendEmailAsync(model.Email, "KıbrısApp Kayıt Doğrulama", htmlBody);


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
