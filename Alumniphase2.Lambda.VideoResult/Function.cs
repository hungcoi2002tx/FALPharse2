using Alumniphase2.Lambda.Models;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Alumniphase2.Lambda.VideoResult;

public class Function
{
    public class FaceDetectionInput
    {
        public string BucketName { get; set; } = null!;
        public string PrefixName { get; set; } = null!;
        public string ObjectKey { get; set; } = null!;
        public List<FaceDetectionResult> FaceDetectionResults { get; set; } = null!;
    }

    public string FunctionHandler(FaceDetectionInput input, ILambdaContext context)
    {
        FaceDetectionResult result = ConsolidateFaceData(input);
    }

    public FaceDetectionResult ConsolidateFaceData(List<FaceData> inputList)
    {
        var consolidatedResult = new FaceDetectionResult
        {
            FileName = inputList.First().FileName,  
            RegisteredFaces = new List<FaceRecognitionResponse>(),
            UnregisteredFaces = new List<FaceRecognitionResponse>(), 
            Width = 0, 
            Height = 0, 
            Key = string.Empty  
        };

        var registeredFaceDictionary = new Dictionary<(string FaceId, string? UserId), FaceRecognitionResponse>();

        foreach (var faceData in inputList)
        {
            // Combine RegisteredFaces by ensuring unique FaceId and UserId
            foreach (var face in faceData.RegisteredFaces)
            {
                var key = (face.FaceId, face.UserId);

                if (!registeredFaceDictionary.ContainsKey(key))
                {
                    // If this FaceId and UserId combination is not yet added, add it
                    registeredFaceDictionary[key] = face;
                }
            }
        }

        // Populate the consolidated result with the unique registered faces
        consolidatedResult.RegisteredFaces = registeredFaceDictionary.Values.ToList();

        return consolidatedResult;
    }
}
