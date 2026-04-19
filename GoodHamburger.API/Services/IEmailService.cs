namespace GoodHamburger.API.Services;

public interface IEmailService
{
    Task EnviarRedefinicaoSenhaAsync(string email, string nome, string token, CancellationToken cancellationToken = default);
}
