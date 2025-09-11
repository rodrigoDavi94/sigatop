using System.Text.Json.Serialization;

namespace sigatop.Model;

public class Endereco
{
    [JsonPropertyName("cep")]
    public string? Cep { get; set; }

    [JsonPropertyName("logradouro")]
    public string? Logradouro { get; set; }

    [JsonPropertyName("complemento")]
    public string? Complemento { get; set; }

    [JsonPropertyName("bairro")]
    public string? Bairro { get; set; }

    [JsonPropertyName("localidade")]
    public string? Localidade { get; set; } // Cidade

    [JsonPropertyName("uf")]
    public string? Uf { get; set; } // Estado

    [JsonPropertyName("ibge")]
    public string? Ibge { get; set; }

    // Propriedade para verificar se o CEP não foi encontrado
    [JsonPropertyName("erro")]
    public bool Erro { get; set; }
}
