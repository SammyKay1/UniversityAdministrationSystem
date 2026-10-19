
public interface IArchivable
{
    bool IsArchived { get; }

    void Archive();

    void Restore();
}