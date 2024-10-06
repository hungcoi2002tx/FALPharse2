using FAL.Services.IServices;

namespace FAL.UnitOfWork
{
    public interface IUnitOfWork
    {
        ICollectionService CollectionService { get; }
        IDynamoDBService DynamoService { get; }
        IS3Service S3Service { get; }
        Task AddTask(Func<Task> task, Func<Task> rollbackTask);
        Task CommitAsync();
        Task RollbackAsync(int committedTaskCount);
    }
}
