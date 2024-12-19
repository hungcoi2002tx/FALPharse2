using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Share.Model;

namespace FAL.DataInitial
{
    public class DefaultDataInitializer
    {
        private const string CLIENT_REQUEST_TABLE = "ClientRequests";
        private const string RESULT_INFO_TABLE = "ResultInfo";
        private const string FACE_INFO_TABLE = "FaceInfo";
        private const string ACCOUNTS_TABLE = "Accounts";
        private const string ROLES_TABLE = "Roles";
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly IAmazonDynamoDB _dynamoDbClient;

        public DefaultDataInitializer(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB dynamoDbClient)
        {
            _dynamoDbContext = dynamoDbContext;
            _dynamoDbClient = dynamoDbClient;
        }

        public async Task SeedDefaultDataAsync()
        {
            // Check and create the table if it does not exist
            await EnsureTablesExistAsync();

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
        private async Task EnsureTablesExistAsync()
        {
            var tableTasks = new List<Task>
            {
                CreateTableIfNotExistsAsync(CLIENT_REQUEST_TABLE),
                CreateTableIfNotExistsAsync(FACE_INFO_TABLE),
                CreateTableIfNotExistsAsync(RESULT_INFO_TABLE),
                CreateTableIfNotExistsAsync(ACCOUNTS_TABLE),
                CreateTableIfNotExistsAsync(ROLES_TABLE)
            };

            await Task.WhenAll(tableTasks);
        }
        private async Task CreateTableIfNotExistsAsync(string tableName)
        {
            try
            {
                // Check if the table already exists
                var response = await _dynamoDbClient.DescribeTableAsync(new DescribeTableRequest
                {
                    TableName = tableName
                });
                Console.WriteLine($"The table '{tableName}' already exists.");
            }
            catch (ResourceNotFoundException)
            {
                // If the table does not exist, create a new one
                Console.WriteLine($"The table '{tableName}' does not exist. Creating the table...");

                var createTableRequest = new CreateTableRequest
                {
                    TableName = tableName,
                    KeySchema = [],
                    AttributeDefinitions = [],
                    BillingMode = BillingMode.PAY_PER_REQUEST
                };

                // Configure schema and definitions based on the table name
                switch (tableName)
                {
                    case ACCOUNTS_TABLE:
                        createTableRequest.KeySchema.Add(new KeySchemaElement("Username", KeyType.HASH));
                        createTableRequest.AttributeDefinitions.Add(new AttributeDefinition("Username", ScalarAttributeType.S));
                        createTableRequest.GlobalSecondaryIndexes =
                        [
                            new() {
                                IndexName = "SystemNameIndex",
                                KeySchema =
                                [
                                    new KeySchemaElement("SystemName", KeyType.HASH),
                                ],
                                Projection = new Projection
                                {
                                    ProjectionType = ProjectionType.ALL
                                }
                            }
                        ];
                        break;

                    case ROLES_TABLE:
                        createTableRequest.KeySchema.Add(new KeySchemaElement("RoleId", KeyType.HASH));
                        createTableRequest.AttributeDefinitions.Add(new AttributeDefinition("RoleId", ScalarAttributeType.N));
                        break;

                    case CLIENT_REQUEST_TABLE:
                        createTableRequest.KeySchema.Add(new KeySchemaElement("SystemName", KeyType.HASH));
                        createTableRequest.KeySchema.Add(new KeySchemaElement("CreateDate", KeyType.RANGE));
                        createTableRequest.AttributeDefinitions.Add(new AttributeDefinition("SystemName", ScalarAttributeType.S));
                        createTableRequest.AttributeDefinitions.Add(new AttributeDefinition("CreateDate", ScalarAttributeType.S));
                        break;

                    case RESULT_INFO_TABLE:
                        createTableRequest.KeySchema.Add(new KeySchemaElement("FileName", KeyType.HASH));
                        createTableRequest.KeySchema.Add(new KeySchemaElement("SystemName", KeyType.RANGE));
                        createTableRequest.AttributeDefinitions.Add(new AttributeDefinition("FileName", ScalarAttributeType.S));
                        createTableRequest.AttributeDefinitions.Add(new AttributeDefinition("SystemName", ScalarAttributeType.S));
                        break;

                    case FACE_INFO_TABLE:
                        createTableRequest.KeySchema.Add(new KeySchemaElement("UserId", KeyType.HASH));
                        createTableRequest.KeySchema.Add(new KeySchemaElement("FaceId", KeyType.RANGE));
                        createTableRequest.AttributeDefinitions.Add(new AttributeDefinition("UserId", ScalarAttributeType.S));
                        createTableRequest.AttributeDefinitions.Add(new AttributeDefinition("FaceId", ScalarAttributeType.S));
                        createTableRequest.AttributeDefinitions.Add(new AttributeDefinition("SystemName", ScalarAttributeType.S));

                        createTableRequest.GlobalSecondaryIndexes =
                        [
                            new() {
                                IndexName = "SystemNameIndex",
                                KeySchema =
                                [
                                    new KeySchemaElement("SystemName", KeyType.HASH),
                                    new KeySchemaElement("UserId", KeyType.RANGE)
                                ],
                                Projection = new Projection
                                {
                                    ProjectionType = ProjectionType.ALL
                                }
                            }
                        ];
                        break;

                    default:
                        throw new ArgumentException($"Unsupported table name: {tableName}");
                }

                await _dynamoDbClient.CreateTableAsync(createTableRequest);
                Console.WriteLine($"The table '{tableName}' has been created.");

                // Wait until the table becomes ACTIVE
                await WaitForTableToBecomeActiveAsync(tableName);
            }
        }

        private DefaultData LoadDefaultData(string filePath)
        {
            // Kiểm tra nếu file dữ liệu không tồn tại thì báo lỗi
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Data file not found: {filePath}");
            }

            // Đọc dữ liệu từ file JSON và deserialize thành object DefaultData
            var jsonData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<DefaultData>(jsonData);
        }

        private async Task AddAccountIfNotExistsAsync(Account account)
        {
            // Kiểm tra xem tài khoản đã tồn tại trong bảng chưa
            var existingAccount = await _dynamoDbContext.LoadAsync<Account>(account.Username);
            if (existingAccount == null)
            {
                // Nếu chưa tồn tại thì thêm tài khoản mới vào bảng
                await _dynamoDbContext.SaveAsync(account);
                Console.WriteLine($"Added account: {account.Username}");
            }
        }

        private async Task AddRoleIfNotExistsAsync(Role role)
        {
            // Kiểm tra xem vai trò đã tồn tại trong bảng chưa
            var existingRole = await _dynamoDbContext.LoadAsync<Role>(role.RoleId);
            if (existingRole == null)
            {
                // Nếu chưa tồn tại thì thêm vai trò mới vào bảng
                await _dynamoDbContext.SaveAsync(role);
                Console.WriteLine($"Added role: {role.RoleId}");
            }
        }

        private async Task WaitForTableToBecomeActiveAsync(string tableName)
        {
            Console.WriteLine($"Waiting for the table '{tableName}' to become ACTIVE...");

            // Vòng lặp kiểm tra trạng thái bảng đến khi nó chuyển sang trạng thái ACTIVE
            while (true)
            {
                var response = await _dynamoDbClient.DescribeTableAsync(new DescribeTableRequest
                {
                    TableName = tableName
                });

                if (response.Table.TableStatus == TableStatus.ACTIVE)
                {
                    Console.WriteLine($"The table '{tableName}' is ready.");
                    break;
                }

                // Đợi 1 giây trước khi kiểm tra lại
                await Task.Delay(2000);
            }
        }
    }
}
