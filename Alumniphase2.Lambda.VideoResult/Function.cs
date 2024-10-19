using Alumniphase2.Lambda.Models;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Alumniphase2.Lambda.VideoResult;

public class Function
{
    
   
    public string FunctionHandler(List<FaceDetectionResult> input, ILambdaContext context)
    {
        return "";
    }
}
