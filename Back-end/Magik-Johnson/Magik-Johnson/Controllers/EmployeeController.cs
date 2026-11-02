using FluentValidation;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magik_Johnson.Controllers;

[ApiController]
[Authorize(Roles = "manager")]
public class EmployeeController : BaseController
{
    private readonly IEmployeeService _employeeService;
    private readonly IValidator<EmployeeRegistrationRequest> _employeeRegistrationRequestValidator;
    private readonly IValidator<EmployeeUpdateRequest> _employeeUpdateRequestValidator;

    public EmployeeController(
        IEmployeeService employeeService,
        IValidator<EmployeeRegistrationRequest> employeeRegistrationRequestValidator,
        IValidator<EmployeeUpdateRequest> employeeUpdateRequestValidator
    )
    {
        _employeeService = employeeService;
        _employeeRegistrationRequestValidator = employeeRegistrationRequestValidator;
        _employeeUpdateRequestValidator = employeeUpdateRequestValidator;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetEmployeeList()
    {
        return Ok(await _employeeService.ListAsync());
    }

    [HttpGet("get/{employeeId}")]
    public async Task<IActionResult> GetEmployeeById(string employeeId)
    {
        var employee = await _employeeService.GetByIdAsync(employeeId);
        if (employee is null)
        {
            return NotFound();
        }
        return Ok(employee);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateEmployee(EmployeeUpdateRequest employee)
    {
        await _employeeUpdateRequestValidator.ValidateAndThrowAsync(employee);

        var succeeded = await _employeeService.UpdateAsync(employee);

        return Ok(succeeded);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddEmployee(EmployeeRegistrationRequest employee)
    {
        await _employeeRegistrationRequestValidator.ValidateAndThrowAsync(employee);

        var succeeded = await _employeeService.AddAsync(employee);

        return Ok(succeeded);
    }

    [HttpPost("lockAccount/{employeeId}")]
    public async Task<IActionResult> LockAccount(string employeeId)
    {
        var succeeded = await _employeeService.LockAccountAsync(employeeId);

        return Ok(succeeded);
    }

    [HttpPost("unLockAccount/{employeeId}")]
    public async Task<IActionResult> UnLockAccount(string employeeId)
    {
        var succeeded = await _employeeService.UnlockAccountAsync(employeeId);

        return Ok(succeeded);
    }
}
