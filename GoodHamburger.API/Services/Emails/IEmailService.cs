namespace GoodHamburger.API.Services.Emails;

public interface IEmailService
{
    Task EnviarRedefinicaoSenhaAsync(string email, string nome, string token, CancellationToken cancellationToken = default);
}
