using ShotCall.Application.DTOs;

namespace ShotCall.Application.Interfaces;

public interface IAccountService
{
    Task<List<AccountDto>> GetAllAsync(AccountFilter filter);
    Task<AccountDto?> ApproveAsync(Guid userId);
    Task<AccountDto?> RejectAsync(Guid userId);
    Task<AccountDto?> DisableAsync(Guid userId);
    Task<AccountDto?> EnableAsync(Guid userId);
}