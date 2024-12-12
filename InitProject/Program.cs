
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using System.Security.Principal;
using Amazon.DynamoDBv2.DataModel;

namespace InitProject
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await CreateTableFaceInfoAsync();
                await CreateTableResultInfoASync();
                await CreateTableClientRequestsASync();
                await CreateTableAccountAsync();
                await CreateTableRoleAsync();
                await CreateRoleRecordAsync();
                await AddAccountIfNotExistsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task CreateRoleRecordAsync()
        {
            await Task.Delay(10000);
            // Tên bảng cụ thể để thêm records
            string tableName = "Roles2";

            // Khởi tạo Amazon DynamoDB Client
            var client = new AmazonDynamoDBClient();

            // Danh sách các records cần thêm vào bảng Roles
            var roles = new List<Dictionary<string, AttributeValue>>
        {
            // Record cho Role USER
            new Dictionary<string, AttributeValue>
            {
                { "RoleId", new AttributeValue { N = "2" } }, // Partition Key
                { "RoleName", new AttributeValue { S = "USER" } },
                { "Permissions", new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new AttributeValue
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    { "Resource", new AttributeValue { S = "Detect" } },
                                    { "Actions", new AttributeValue { L = new List<AttributeValue> { new AttributeValue { S = "POST" } } } }
                                }
                            },
                            new AttributeValue
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    { "Resource", new AttributeValue { S = "Train" } },
                                    { "Actions", new AttributeValue { L = new List<AttributeValue> { new AttributeValue { S = "POST" } } } }
                                }
                            },
                            new AttributeValue
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    { "Resource", new AttributeValue { S = "Users" } },
                                    { "Actions", new AttributeValue { L = new List<AttributeValue> { new AttributeValue { S = "PUT" } } } }
                                }
                            }
                        }
                    }
                }
            },

            // Record cho Role ADMIN
            new Dictionary<string, AttributeValue>
            {
                { "RoleId", new AttributeValue { N = "1" } }, // Partition Key
                { "RoleName", new AttributeValue { S = "ADMIN" } },
                { "Permissions", new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new AttributeValue
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    { "Resource", new AttributeValue { S = "Accounts" } },
                                    { "Actions", new AttributeValue { L = new List<AttributeValue>
                                        {
                                            new AttributeValue { S = "DELETE" },
                                            new AttributeValue { S = "GET" },
                                            new AttributeValue { S = "POST" },
                                            new AttributeValue { S = "PUT" }
                                        }}
                                    }
                                }
                            },
                            new AttributeValue
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    { "Resource", new AttributeValue { S = "Compare" } },
                                    { "Actions", new AttributeValue { L = new List<AttributeValue> { new AttributeValue { S = "POST" } } } }
                                }
                            }
                        }
                    }
                }
            }
        };

            try
            {
                // Thêm từng record vào bảng Roles
                foreach (var roleRecord in roles)
                {
                    var putItemRequest = new PutItemRequest
                    {
                        TableName = tableName, // Chỉ định rõ tên bảng
                        Item = roleRecord
                    };

                    await client.PutItemAsync(putItemRequest);
                    Console.WriteLine($"Added record with RoleId: {roleRecord["RoleId"].N} to table {tableName}.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error adding record: {e.Message}");
            }
        }

        private static async Task AddAccountIfNotExistsAsync()
        {
            try
            {
                await Task.Delay(10000);
                var client = new AmazonDynamoDBClient();

                string tableName = "Accounts2"; // Tên bảng của bạn

                var record = new Dictionary<string, AttributeValue>
                {
                    { "Username", new AttributeValue { S = "admin" } },               // Partition Key
                    { "Email", new AttributeValue { S = "admin@fu.edu.vn" } },
                    { "Password", new AttributeValue { S = "$2a$11$JsVZzIQ5YlxHHiJi0qAJr.cqX1r6TtGHbPz2da1Q7pla3iAZKty4S" } },        // Mật khẩu hash
                    { "RoleId", new AttributeValue { N = "1" } },                   // Role dưới dạng số
                    { "Status", new AttributeValue { S = "Active" } },
                    { "SystemName", new AttributeValue { S = "N/A" } },
                    { "WebhookSecretKey", new AttributeValue { S = "N/A" } },
                    { "WebhookUrl", new AttributeValue { S = "https://test" } }
                };

                try
                {
                    var putItemRequest = new PutItemRequest
                    {
                        TableName = tableName,
                        Item = record
                    };

                    await client.PutItemAsync(putItemRequest);
                    Console.WriteLine("Record added successfully.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error adding record: {e.Message}");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static async Task CreateTableRoleAsync()
        {
            try
            {

                var client = new AmazonDynamoDBClient();

                var createTableRequest = new CreateTableRequest
                {
                    TableName = "Roles2",
                    BillingMode = BillingMode.PAY_PER_REQUEST,
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement { AttributeName = "RoleId", KeyType = "HASH" }
                    },
                                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition { AttributeName = "RoleId", AttributeType = "N" }
                    },
                };
                var response = await client.CreateTableAsync(createTableRequest);
                Console.WriteLine($"Table {response.TableDescription.TableName} created successfully. Status: {response.TableDescription.TableStatus}");
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static async Task CreateTableAccountAsync()
        {
            try
            {
                var client = new AmazonDynamoDBClient();

                var createTableRequest = new CreateTableRequest
                {
                    TableName = "Accounts2",
                    BillingMode = BillingMode.PAY_PER_REQUEST,
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("Username", KeyType.HASH) // Primary Key
                    },
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition("Username", ScalarAttributeType.S)
                    },                 
                };
                var response = await client.CreateTableAsync(createTableRequest);
                Console.WriteLine($"Table {response.TableDescription.TableName} created successfully. Status: {response.TableDescription.TableStatus}");
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static async Task CreateTableClientRequestsASync()
        {
            try
            {
                var client = new AmazonDynamoDBClient();

                var createTableRequest = new CreateTableRequest
                {
                    TableName = "ClientRequests2", // Tên bảng
                    BillingMode = BillingMode.PAY_PER_REQUEST,
                    KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = "SystemName", // Partition Key
                    KeyType = "HASH"
                },
                new KeySchemaElement
                {
                    AttributeName = "CreateDate", // Sort Key
                    KeyType = "RANGE"
                }
            },
                    AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition
                {
                    AttributeName = "SystemName",
                    AttributeType = "S" // String
                },
                new AttributeDefinition
                {
                    AttributeName = "CreateDate",
                    AttributeType = "S" // String
                }
            },
                };
                // Gửi yêu cầu tạo bảng
                var response = await client.CreateTableAsync(createTableRequest);
                Console.WriteLine($"Table {response.TableDescription.TableName} created successfully. Status: {response.TableDescription.TableStatus}");
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static async Task CreateTableResultInfoASync()
        {
            try
            {
                var client = new AmazonDynamoDBClient();

                var createTableRequest = new CreateTableRequest
                {
                    TableName = "ResultInfo2", // Tên bảng
                    BillingMode = BillingMode.PAY_PER_REQUEST, // Sử dụng chế độ PAY_PER_REQUEST
                    KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = "FileName", // Partition Key
                    KeyType = "HASH"
                },
                new KeySchemaElement
                {
                    AttributeName = "SystemName", // Sort Key
                    KeyType = "RANGE"
                }
            },
                    AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition
                {
                    AttributeName = "FileName",
                    AttributeType = "S" // String
                },
                new AttributeDefinition
                {
                    AttributeName = "SystemName",
                    AttributeType = "S" // String
                }
            }
                };

                // Gửi yêu cầu tạo bảng
                var response = await client.CreateTableAsync(createTableRequest);
                Console.WriteLine($"Table {response.TableDescription.TableName} created successfully. Status: {response.TableDescription.TableStatus}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating table: {ex.Message}");
                throw;
            }
        }


        private static async Task CreateTableFaceInfoAsync()
        {
            try
            {
                var client = new AmazonDynamoDBClient();
                // Tạo đối tượng CreateTableRequest
                var createTableRequest = new CreateTableRequest
                {
                    TableName = "FaceInfo2", // Tên bảng
                    BillingMode = BillingMode.PAY_PER_REQUEST,
                    KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = "UserId", // Partition Key
                    KeyType = "HASH"
                },
                new KeySchemaElement
                {
                    AttributeName = "FaceId", // Sort Key
                    KeyType = "RANGE"
                }
            },
                    AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition
                {
                    AttributeName = "UserId",
                    AttributeType = "S" // String
                },
                new AttributeDefinition
                {
                    AttributeName = "FaceId",
                    AttributeType = "S" // String
                },
                new AttributeDefinition
                {
                    AttributeName = "SystemName",
                    AttributeType = "S" // String (cho index)
                }
            },
                    GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new GlobalSecondaryIndex
                {
                    IndexName = "SystemNameIndex", // Tên GSI
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "SystemName", // Partition Key cho index
                            KeyType = "HASH"
                        },
                        new KeySchemaElement
                        {
                            AttributeName = "UserId", // Sort Key cho index
                            KeyType = "RANGE"
                        }
                    },
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL // GSI chứa tất cả thuộc tính
                    }
                }
            }
                };

                // Gửi yêu cầu tạo bảng
                var response = await client.CreateTableAsync(createTableRequest);
                Console.WriteLine($"Table {response.TableDescription.TableName} created successfully. Status: {response.TableDescription.TableStatus}");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
