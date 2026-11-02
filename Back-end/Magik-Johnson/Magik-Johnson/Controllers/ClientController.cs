using DTOs.Responses;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magik_Johnson.Controllers;

[ApiController]
[Authorize(Roles = "manager")]
public class ClientController : BaseController
{
    private readonly IClientService _clientService;

    public ClientController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetEmployeeList()
    {
        return Ok(await _clientService.ListAsync());
    }

    [HttpPost("lockAccount/{clientId}")]
    public async Task<IActionResult> LockAccount(string clientId)
    {
        var succeeded = await _clientService.LockAccountAsync(clientId);

        return Ok(succeeded);
    }

    [HttpPost("unLockAccount/{clientId}")]
    public async Task<IActionResult> UnLockAccount(string clientId)
    {
        var succeeded = await _clientService.UnlockAccountAsync(clientId);

        return Ok(succeeded);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchByEmail(
        [FromQuery] string email,
        [FromQuery] int limit = 10
    )
    {
        if (string.IsNullOrWhiteSpace(email))
            return Ok(new List<ClientResponse>());

        var clients = await _clientService.SearchByEmailAsync(email, limit);
        return Ok(clients);
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var client = await _clientService.GetByIdAsync(id);
        if (client == null)
            return NotFound();

        return Ok(client);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] ClientUpdateRequest request)
    {
        var succeeded = await _clientService.UpdateAsync(request);
        if (succeeded)
        {
            return Ok(succeeded);
        }
        return BadRequest();
    }
}
