using Microsoft.AspNetCore.Mvc;
using Moq;
using sigatop.Controllers;
using sigatop.Model;
using sigatop.Services;
using Xunit;

namespace sigatop.Tests.Controllers;

public class BuscaEnderecoControllerTests
{
    private readonly Mock<IBuscaEnderecoService> _serviceMock;
    private readonly BuscaEnderecoController _controller;

    public BuscaEnderecoControllerTests()
    {
        _serviceMock = new Mock<IBuscaEnderecoService>();
        _controller = new BuscaEnderecoController(_serviceMock.Object);
    }

    [Fact]
    public async Task Get_ComCepValido_DeveRetornar200ComEndereco()
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

        _serviceMock
            .Setup(s => s.BuscarAsync(cep))
            .ReturnsAsync(enderecoEsperado);

        // Act
        var resultado = await _controller.Listar(cep);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var endereco = Assert.IsType<Endereco>(okResult.Value);
        Assert.Equal(enderecoEsperado.Cep, endereco.Cep);
        Assert.Equal(enderecoEsperado.Logradouro, endereco.Logradouro);
        Assert.Equal(enderecoEsperado.Bairro, endereco.Bairro);
        Assert.Equal(enderecoEsperado.Localidade, endereco.Localidade);
        Assert.Equal(enderecoEsperado.Uf, endereco.Uf);
    }

    [Fact]
    public async Task Get_ComCepInvalido_DeveRetornar404()
    {
        // Arrange
        var cep = "83000-000";

        _serviceMock
            .Setup(s => s.BuscarAsync(cep))
            .ReturnsAsync((Endereco?)null);

        // Act
        var resultado = await _controller.Listar(cep);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.Equal("CEP não encontrado ou inválido.", notFoundResult.Value);
    }

    [Theory]
    [InlineData("83005-190")]
    [InlineData("80010-000")]
    [InlineData("83010-500")]
    [InlineData("82010-120")]
    public async Task Listar_ComDiferentesCepsValidos_DeveRetornarOk(string cep)
    {
        // Arrange
        var endereco = new Endereco
        {
            Cep = cep,
            Logradouro = "Rua Teste",
            Bairro = "Bairro Teste",
            Localidade = "Cidade Teste",
            Uf = "SP",
            Erro = false
        };

        _serviceMock
            .Setup(s => s.BuscarAsync(cep))
            .ReturnsAsync(endereco);

        // Act
        var resultado = await _controller.Listar(cep);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var enderecoRetornado = Assert.IsType<Endereco>(okResult.Value);
        Assert.Equal(cep, enderecoRetornado.Cep);
    }

    [Fact]
    public async Task ListarVarios_ComCepsValidos_DeveRetornarOkComListaEnderecos()
    {
        // Arrange
        var ceps = new List<string> { "83005-190", "80010-000", "83010-500" };
        var endereco1 = new Endereco
        {
            Cep = "83005-190",
            Logradouro = "Rua Coronel Zacarias",
            Bairro = "Centro",
            Localidade = "São José dos Pinhais",
            Uf = "PR",
            Erro = false
        };
        var endereco2 = new Endereco
        {
            Cep = "80010-000",
            Logradouro = "Rua XV de Novembro",
            Bairro = "Centro",
            Localidade = "Curitiba",
            Uf = "PR",
            Erro = false
        };
        var endereco3 = new Endereco
        {
            Cep = "83010-500",
            Logradouro = "Rua Saldanha Marinho",
            Bairro = "Centro",
            Localidade = "São José dos Pinhais",
            Uf = "PR",
            Erro = false
        };

        _serviceMock.Setup(s => s.BuscarAsync("83005-190")).ReturnsAsync(endereco1);
        _serviceMock.Setup(s => s.BuscarAsync("80010-000")).ReturnsAsync(endereco2);
        _serviceMock.Setup(s => s.BuscarAsync("83010-500")).ReturnsAsync(endereco3);

        // Act
        var resultado = await _controller.ListarVarios(ceps);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var enderecos = Assert.IsType<List<Endereco?>>(okResult.Value);
        Assert.Equal(3, enderecos.Count);
        Assert.Equal("83005-190", enderecos[0]?.Cep);
        Assert.Equal("80010-000", enderecos[1]?.Cep);
        Assert.Equal("83010-500", enderecos[2]?.Cep);
    }

    [Fact]
    public async Task ListarVarios_ComCepsValidosEInvalidos_DeveRetornarListaMista()
    {
        // Arrange
        var ceps = new List<string> { "83005-190", "83000-000", "80010-000" };
        var endereco1 = new Endereco
        {
            Cep = "83005-190",
            Logradouro = "Rua Coronel Zacarias",
            Bairro = "Centro",
            Localidade = "São José dos Pinhais",
            Uf = "PR",
            Erro = false
        };
        var endereco3 = new Endereco
        {
            Cep = "80010-000",
            Logradouro = "Rua XV de Novembro",
            Bairro = "Centro",
            Localidade = "Curitiba",
            Uf = "PR",
            Erro = false
        };

        _serviceMock.Setup(s => s.BuscarAsync("83005-190")).ReturnsAsync(endereco1);
        _serviceMock.Setup(s => s.BuscarAsync("83000-000")).ReturnsAsync((Endereco?)null);
        _serviceMock.Setup(s => s.BuscarAsync("80010-000")).ReturnsAsync(endereco3);

        // Act
        var resultado = await _controller.ListarVarios(ceps);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var enderecos = Assert.IsType<List<Endereco?>>(okResult.Value);
        Assert.Equal(3, enderecos.Count);
        Assert.NotNull(enderecos[0]); // CEP válido
        Assert.Null(enderecos[1]);    // CEP inválido
        Assert.NotNull(enderecos[2]); // CEP válido
    }

    [Fact]
    public async Task ListarVarios_ComExcecaoNoService_DeveRetornarInternalServerError()
    {
        // Arrange
        var ceps = new List<string> { "83005-190" };

        _serviceMock
            .Setup(s => s.BuscarAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Erro de conexão"));

        // Act
        var resultado = await _controller.ListarVarios(ceps);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Contains("Erro ao processar a solicitação", statusCodeResult.Value?.ToString());
    }

    [Fact]
    public async Task ListarVarios_ComListaVazia_DeveRetornarOkComListaVazia()
    {
        // Arrange
        var ceps = new List<string>();

        // Act
        var resultado = await _controller.ListarVarios(ceps);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var enderecos = Assert.IsType<List<Endereco?>>(okResult.Value);
        Assert.Empty(enderecos);
    }

    [Fact]
    public async Task ListarVarios_ComUmCep_DeveRetornarListaComUmItem()
    {
        // Arrange
        var ceps = new List<string> { "83005-190" };
        var endereco = new Endereco
        {
            Cep = "83005-190",
            Logradouro = "Rua Coronel Zacarias",
            Bairro = "Centro",
            Localidade = "São José dos Pinhais",
            Uf = "PR",
            Erro = false
        };

        _serviceMock
            .Setup(s => s.BuscarAsync("83005-190"))
            .ReturnsAsync(endereco);

        // Act
        var resultado = await _controller.ListarVarios(ceps);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var enderecos = Assert.IsType<List<Endereco?>>(okResult.Value);
        Assert.Single(enderecos);
        Assert.Equal("83005-190", enderecos[0]?.Cep);
    }
}