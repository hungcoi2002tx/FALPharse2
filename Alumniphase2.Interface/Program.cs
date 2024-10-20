using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.S3;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddTransient<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Map controllers if you are using them
app.MapControllers();

// Map Razor Pages
app.MapRazorPages();

app.Run();
