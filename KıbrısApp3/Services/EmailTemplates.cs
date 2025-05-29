namespace KıbrısApp3.Services
{
    public static class EmailTemplates
    {
        public static string GenerateVerificationTemplate(string title, string message, string code)
        {
            return $@"
<html>
<body style='font-family: Arial; background-color: #8ADBD2; padding: 20px;'>
  <div style='background-color: #F5F5F5; max-width: 600px; margin: auto; padding: 20px; border-radius: 10px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); text-align: center;'>
    <img src='https://res.cloudinary.com/dqsqxswiu/image/upload/v1747843578/33_aw9hi2.png' alt='KıbrısAlSat Logo' style='max-width: 150px; margin-bottom: 20px;' />
    <h2 style='color: #333;'>{title}</h2>
    <p>{message}</p>
    <div style='font-size: 28px; font-weight: bold; color: #2196F3; margin: 20px 0;'>{code}</div>
    <p>Bu kod <strong>3 dakika</strong> içinde geçerliliğini yitirecektir.</p>
    <hr style='margin-top: 30px; border: none; border-top: 1px solid #ccc;'/>
    <p style='font-size: 12px; color: #888;'>Bu e-posta KıbrısAlSat sisteminden gönderilmiştir.</p>
  </div>
</body>
</html>";
        }
    }
}
