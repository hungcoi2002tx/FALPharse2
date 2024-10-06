using Amazon.DynamoDBv2;
using FAL.Services.IServices;

namespace FAL.UnitOfWork.Implement
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDynamoDBService _dynamoDBService;
        private readonly IS3Service _s3Service;
        private readonly ICollectionService _collectionService;
        private readonly List<Func<Task>> _tasks;
        private readonly List<Func<Task>> _rollbackTasks;

        public UnitOfWork(IDynamoDBService dynamoDBService, IS3Service s3Service, ICollectionService collectionService)
        {
            _dynamoDBService = dynamoDBService;
            _s3Service = s3Service;
            _collectionService = collectionService;
            _tasks = new List<Func<Task>>();
            _rollbackTasks = new List<Func<Task>>();
            _collectionService = collectionService;
        }

        public ICollectionService CollectionService => _collectionService;

        public IDynamoDBService DynamoService => _dynamoDBService;

        public IS3Service S3Service => _s3Service;

        public async Task AddTask(Func<Task> task, Func<Task> rollbackTask)
        {
            _tasks.Add(task);
            _rollbackTasks.Add(rollbackTask);
        }

        public async Task CommitAsync()
        {
            int taskIndex = 0; // This will keep track of the index of the successfully committed task
            try
            {
                for (; taskIndex < _tasks.Count; taskIndex++)
                {
                    await _tasks[taskIndex](); // Execute each task
                }
                Console.WriteLine("All tasks committed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during commit at task {taskIndex}: {ex.Message}");
                await RollbackAsync(taskIndex); // Rollback only up to the last successfully committed task
                throw; // Re-throw the exception after rollback
            }
        }

        public async Task RollbackAsync(int committedTaskCount)
        {
            for (int i = committedTaskCount - 1; i >= 0; i--) // Rollback only the tasks that were successfully committed
            {
                await _rollbackTasks[i]();
            }
            Console.WriteLine("Rollback complete for committed tasks.");
        }
    }

}
