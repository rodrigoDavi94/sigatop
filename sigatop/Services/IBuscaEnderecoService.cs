using sigatop.Model;

namespace sigatop.Services;

public interface IBuscaEnderecoService
{
    Task<Endereco?> BuscarAsync(string cep);
}