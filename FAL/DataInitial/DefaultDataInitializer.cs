using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Share.Model;
namespace FAL.DataInitial
{
    public class DefaultDataInitializer
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly IAmazonDynamoDB _dynamoDbClient;

        public DefaultDataInitializer(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB dynamoDbClient)
        {
            _dynamoDbContext = dynamoDbContext;
            _dynamoDbClient = dynamoDbClient;
        }

        public async Task SeedDefaultDataAsync()
        {
            // Kiểm tra và tạo bảng nếu chưa có
            await CreateTableIfNotExistsAsync("Accounts");
            await CreateTableIfNotExistsAsync("Roles");

            var defaultData = LoadDefaultData("DataInitial/defaultData.json");

            // Seed Accounts
            foreach (var account in defaultData.Accounts)
            {
                await AddAccountIfNotExistsAsync(account);
            }

            // Seed Roles
            foreach (var role in defaultData.Roles)
            {
                await AddRoleIfNotExistsAsync(role);
            }
        }

        private async Task CreateTableIfNotExistsAsync(string tableName)
        {
            try
            {
                // Kiểm tra xem bảng đã tồn tại hay chưa
                var response = await _dynamoDbClient.DescribeTableAsync(new DescribeTableRequest
                {
                    TableName = tableName
                });
                Console.WriteLine($"Bảng '{tableName}' đã tồn tại.");
            }
            catch (ResourceNotFoundException)
            {
                // Nếu bảng không tồn tại, tạo mới
                Console.WriteLine($"Bảng '{tableName}' chưa tồn tại. Đang tạo bảng...");

                var createTableRequest = new CreateTableRequest
                {
                    TableName = tableName,
                    KeySchema = new List<KeySchemaElement>(),
                    AttributeDefinitions = new List<AttributeDefinition>(),
                    ProvisionedThroughput = new ProvisionedThroughput(5, 5)
                };

                // Cấu hình schema và định nghĩa theo tên bảng
                if (tableName == "Accounts")
                {
                    createTableRequest.KeySchema.Add(new KeySchemaElement("Username", KeyType.HASH));
                    createTableRequest.AttributeDefinitions.Add(new AttributeDefinition("Username", ScalarAttributeType.S));
                }
                else if (tableName == "Roles")
                {
                    createTableRequest.KeySchema.Add(new KeySchemaElement("RoleId", KeyType.HASH));
                    createTableRequest.AttributeDefinitions.Add(new AttributeDefinition("RoleId", ScalarAttributeType.N));
                }

                await _dynamoDbClient.CreateTableAsync(createTableRequest);
                Console.WriteLine($"Bảng '{tableName}' đã được tạo.");

                // Chờ cho đến khi bảng chuyển sang trạng thái ACTIVE
                await WaitForTableToBecomeActiveAsync(tableName);
            }
        }


        private DefaultData LoadDefaultData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Không tìm thấy file dữ liệu: {filePath}");
            }

            var jsonData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<DefaultData>(jsonData);
        }

        private async Task AddAccountIfNotExistsAsync(Account account)
        {
            var existingAccount = await _dynamoDbContext.LoadAsync<Account>(account.Username);
            if (existingAccount == null)
            {
                await _dynamoDbContext.SaveAsync(account);
                Console.WriteLine($"Đã thêm tài khoản: {account.Username}");
            }
        }

        private async Task AddRoleIfNotExistsAsync(Role role)
        {
            var existingRole = await _dynamoDbContext.LoadAsync<Role>(role.RoleId);
            if (existingRole == null)
            {
                await _dynamoDbContext.SaveAsync(role);
                Console.WriteLine($"Đã thêm vai trò: {role.RoleId}");
            }
        }
        private async Task WaitForTableToBecomeActiveAsync(string tableName)
        {
            Console.WriteLine($"Đang chờ bảng '{tableName}' chuyển sang trạng thái ACTIVE...");

            while (true)
            {
                var response = await _dynamoDbClient.DescribeTableAsync(new DescribeTableRequest
                {
                    TableName = tableName
                });

                if (response.Table.TableStatus == TableStatus.ACTIVE)
                {
                    Console.WriteLine($"Bảng '{tableName}' đã sẵn sàng.");
                    break;
                }

                await Task.Delay(1000); // Chờ 5 giây trước khi kiểm tra lại
            }
        }
    }


}
