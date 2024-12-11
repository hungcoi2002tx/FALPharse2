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

                // Tạo bảng cho Accounts
                if (tableName == "Accounts")
                {
                    var createTableRequest = new CreateTableRequest
                    {
                        TableName = "Accounts",
                        KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("Username", KeyType.HASH) // Primary Key
                    },
                        AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition("Username", ScalarAttributeType.S)
                    },
                        ProvisionedThroughput = new ProvisionedThroughput(5, 5)
                    };
                    await _dynamoDbClient.CreateTableAsync(createTableRequest);
                    Console.WriteLine("Bảng 'Accounts' đã được tạo.");
                }

                // Tạo bảng cho Roles
                if (tableName == "Roles")
                {
                    var createTableRequest = new CreateTableRequest
                    {
                        TableName = "Roles",
                        KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("RoleId", KeyType.HASH) // Primary Key
                    },
                        AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition("RoleId", ScalarAttributeType.N)
                    },
                        ProvisionedThroughput = new ProvisionedThroughput(5, 5)
                    };
                    await _dynamoDbClient.CreateTableAsync(createTableRequest);
                    Console.WriteLine("Bảng 'Roles' đã được tạo.");
                }
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
    }


}
