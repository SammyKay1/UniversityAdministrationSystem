
public interface ICrudService<TCreateDto, TEditDto, TListDto>
{
    Task<IEnumerable<TListDto>> GetAllAsync();

    Task<TEditDto?> GetForEditAsync(int id);

    Task<OperationResult> CreateAsync(TCreateDto dto);

    Task<OperationResult> UpdateAsync(TEditDto dto);

    Task<OperationResult> ArchiveAsync(int id);
}