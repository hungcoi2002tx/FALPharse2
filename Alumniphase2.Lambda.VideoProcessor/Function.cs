using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Alumniphase2.Lambda.VideoProcessor;

public class Function
{
    
    public string FunctionHandler(dynamic input, ILambdaContext context)
    {
        return "lala";
        //dynamic? payload = JsonConvert.DeserializeObject(input);
        //string? bucketName = payload?.bucketName;
        //string? prefixName = payload?.prefixName;
        //string? key = payload?.key;
        //return $"Processed: {payload}";
    }
}
