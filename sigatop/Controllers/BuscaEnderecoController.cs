using Microsoft.AspNetCore.Mvc;
using sigatop.Services;

namespace sigatop.Controllers;

public class BuscaEnderecoController(BuscaEnderecoService service) : ControllerBase
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
}
