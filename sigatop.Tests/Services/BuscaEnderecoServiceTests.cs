using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using Moq.Protected;
using sigatop.Model;
using sigatop.Services;
using Xunit;

namespace sigatop.Tests.Services;

public class BuscaEnderecoServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly BuscaEnderecoService _service;

    public BuscaEnderecoServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _service = new BuscaEnderecoService(_httpClient);
    }

    [Fact]
    public async Task BuscarAsync_ComCepValido_DeveRetornarEndereco()
    {
        // Arrange
        var cep = "83005-190";
        var enderecoEsperado = new Endereco
        {
            Cep = "83005-190",
            Logradouro = "Rua Coronel Zacarias",
            Complemento = "",
            Bairro = "Centro",
            Localidade = "São José dos Pinhais",
            Uf = "PR",
            Ibge = "4125506",
            Erro = false
        };

        var jsonResponse = JsonSerializer.Serialize(enderecoEsperado);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString() == $"https://viacep.com.br/ws/{cep}/json/"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _service.BuscarAsync(cep);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(enderecoEsperado.Cep, resultado.Cep);
        Assert.Equal(enderecoEsperado.Logradouro, resultado.Logradouro);
        Assert.Equal(enderecoEsperado.Bairro, resultado.Bairro);
        Assert.Equal(enderecoEsperado.Localidade, resultado.Localidade);
        Assert.Equal(enderecoEsperado.Uf, resultado.Uf);
        Assert.False(resultado.Erro);
    }

    [Fact]
    public async Task BuscarAsync_ComCepInvalido_DeveRetornarNull()
    {
        // Arrange
        var cep = "83000-000";
        var enderecoComErro = new Endereco
        {
            Erro = true
        };

        var jsonResponse = JsonSerializer.Serialize(enderecoComErro);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString() == $"https://viacep.com.br/ws/{cep}/json/"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _service.BuscarAsync(cep);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task BuscarAsync_ComErroDeRede_DeveLancarException()
    {
        // Arrange
        var cep = "80010-000";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Erro de rede"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.BuscarAsync(cep));
        Assert.Contains("Erro ao buscar o endereço. Verifique sua conexão com a internet.", exception.Message);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    [Fact]
    public async Task BuscarAsync_ComRespostaVazia_DeveRetornarNull()
    {
        // Arrange
        var cep = "80020-000";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString() == $"https://viacep.com.br/ws/{cep}/json/"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _service.BuscarAsync(cep);

        // Assert
        Assert.Null(resultado);
    }

    [Theory]
    [InlineData("83005-190", "Rua Coronel Zacarias", "Centro", "São José dos Pinhais", "PR")]
    [InlineData("80010-000", "Rua XV de Novembro", "Centro", "Curitiba", "PR")]
    [InlineData("83010-500", "Rua Saldanha Marinho", "Centro", "São José dos Pinhais", "PR")]
    public async Task BuscarAsync_ComDiferentesCepsValidos_DeveRetornarEnderecoCorreto(
        string cep, string logradouro, string bairro, string localidade, string uf)
    {
        // Arrange
        var enderecoEsperado = new Endereco
        {
            Cep = cep,
            Logradouro = logradouro,
            Bairro = bairro,
            Localidade = localidade,
            Uf = uf,
            Erro = false
        };

        var jsonResponse = JsonSerializer.Serialize(enderecoEsperado);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString() == $"https://viacep.com.br/ws/{cep}/json/"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _service.BuscarAsync(cep);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(enderecoEsperado.Cep, resultado.Cep);
        Assert.Equal(enderecoEsperado.Logradouro, resultado.Logradouro);
        Assert.Equal(enderecoEsperado.Bairro, resultado.Bairro);
        Assert.Equal(enderecoEsperado.Localidade, resultado.Localidade);
        Assert.Equal(enderecoEsperado.Uf, resultado.Uf);
    }


}