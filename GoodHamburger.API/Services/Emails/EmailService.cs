using System.Net;
using System.Net.Mail;

namespace GoodHamburger.API.Services.Emails;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnviarRedefinicaoSenhaAsync(string email, string nome, string token, CancellationToken cancellationToken = default)
    {
        const string assunto = "Redefinição de Senha - Good Hamburger";
        var corpo = $@"
            Olá, {nome}!

            Sua conta foi inativada. Use o token abaixo para redefinir sua senha:

            Token: {token}

            Endpoint: POST /api/auth/redefinir-senha
            Body: {{ ""email"": ""{email}"", ""token"": ""<token>"", ""novaSenha"": ""<nova senha>"" }}";

        var smtpHost = _configuration["Email:SmtpHost"];
        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            _logger.LogInformation(
                "Email simulado → {Email} | Token: {Token}", email, token);
            return;
        }

        var from = _configuration["Email:From"] ?? "noreply@goodhamburger.com";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");

        using var message = new MailMessage(from, email, assunto, corpo);
        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = enableSsl,
            Credentials = new NetworkCredential(
                _configuration["Email:SmtpUser"],
                _configuration["Email:SmtpPassword"])
        };

        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("Email de redefinição enviado para {Email}", email);
    }
}
