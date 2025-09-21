using Microsoft.AspNetCore.Mvc;
using sigatop.Model;
using sigatop.Services;

namespace sigatop.Controllers;

public class BuscaEnderecoController(IBuscaEnderecoService service) : ControllerBase
{
    [HttpGet("api/Cep/{cep}")]
    public async Task<IActionResult> Listar([FromRoute] string cep)
    {
        var endereco = await service.BuscarAsync(cep);

        if (endereco == null)
        {
            return NotFound("CEP não encontrado ou inválido.");
        }

        return Ok(endereco);
    }

    [HttpPost("api/VariosCep")]
    public async Task<IActionResult> ListarVarios([FromBody] List<string> ceps)
    {
        try
        {
            var resultados = new List<Endereco?>();
            foreach (var cep in ceps)
            {
                var endereco = await service.BuscarAsync(cep);
                resultados.Add(endereco);
            }
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao processar a solicitação: {ex.Message}");
        }

        }

}
