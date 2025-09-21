using sigatop.Model;

namespace sigatop.Services;

public class BuscaEnderecoService(HttpClient httpClient) : IBuscaEnderecoService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<Endereco?> BuscarAsync(string cep)
    {
        try
        {
            var url = $"https://viacep.com.br/ws/{cep}/json/";
            var endereco = await _httpClient.GetFromJsonAsync<Endereco>(url);
            if (endereco is not null && endereco.Erro)
            {
                return null; 
            }

            return endereco;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Erro ao buscar o endereço. Verifique sua conexão com a internet. ", ex);
        }
    }
}