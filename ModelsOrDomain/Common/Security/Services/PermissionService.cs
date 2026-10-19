
using Microsoft.Extensions.Caching.Memory;

public class PermissionService : IPermissionService
{

    private readonly IPermissionRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMemoryCache _cache;
    private readonly IUnitOfWork _unitOfWork;



    public PermissionService(IPermissionRepository repository, ICurrentUserService currentUserService, IMemoryCache cache, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUser = currentUserService;
        _cache = cache;
        _unitOfWork = unitOfWork;
    }


    public async Task<bool> HasPermissionAsync(string permission)
    {

        var permissions = await GetCurrentUserPermissionsAsync();

        return permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);

    }

    public async Task<List<string>> GetCurrentUserPermissionsAsync()
    {
        if (!_currentUser.IsAuthenticated)
            return [];

        var cacheKey =  $"permissions:{_currentUser.UserId}";

        if (_cache.TryGetValue(cacheKey,   out List<string>? permissions))
        {
            return permissions!;
        }

        permissions =  await _repository.GetPermissionNamesForUserAsync( _currentUser.UserId!);

        _cache.Set(cacheKey, permissions,   TimeSpan.FromMinutes(30));

        return permissions;
    }

    public async Task AssignPermissionAsync(string roleId, int permissionId)
    {
        await _repository.AssignPermissionAsync(roleId,   permissionId);

        await _unitOfWork.SaveChangesAsync();

        _cache.Remove($"permissions:{roleId}");
    }


    public async Task RemovePermissionAsync( string roleId,  int permissionId)
    {
        await _repository.RemovePermissionAsync(roleId, permissionId);

        await _unitOfWork.SaveChangesAsync();

        _cache.Remove($"permissions:{roleId}");
    }

}