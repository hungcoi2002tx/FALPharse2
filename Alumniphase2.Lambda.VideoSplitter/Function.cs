using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using System.Diagnostics;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Alumniphase2.Lambda.VideoSplitter;

public class Function
{
    IAmazonS3 S3Client { get; set; }

    public Function()
    {
        S3Client = new AmazonS3Client();
    }
    public class VideoProcessingPayload
    {
        public string BucketName { get; set; }
        public string Prefix { get; set; }
        public string Key { get; set; }
    }
    public async Task<Dictionary<string, string>> FunctionHandler(VideoProcessingPayload payload, ILambdaContext context)
    {
        //var eventRecords = evnt.Records ?? new List<S3Event.S3EventNotificationRecord>();
        //foreach (var record in eventRecords)
        //{
        //    var s3Event = record.S3;
        //    if (s3Event == null)
        //    {
        //        continue;
        //    }



        //    try
        //    {
        //        string bucketName = s3Event.Bucket.Name;
        //        string objectKey = s3Event.Object.Key;

        //        // Log the name of the video (objectKey)
        //        context.Logger.LogInformation($"Processing video: {objectKey}");

        //        // Get the video file
        //        using var videoStream = await this.S3Client.GetObjectStreamAsync(bucketName, objectKey, null);

        //        // Assuming you're using FFmpeg to process video into chunks
        //        string videoName = Path.GetFileNameWithoutExtension(objectKey);
        //        string outputFolder = $"{videoName}/"; // Set the prefix for output parts

        //        // Process video using FFmpeg (pseudo-code, you need to implement FFmpeg logic)
        //        List<Stream> videoParts = CutVideoIntoParts(videoStream, durationInSeconds: 10);

        //        // Upload each part back to S3
        //        int partNumber = 1;
        //        foreach (var part in videoParts)
        //        {
        //            string partKey = $"{outputFolder}part_{partNumber}.mp4";
        //            context.Logger.LogInformation($"Uploading part: {partKey}");

        //            await this.S3Client.PutObjectAsync(new PutObjectRequest
        //            {
        //                BucketName = bucketName,
        //                Key = partKey,
        //                InputStream = part
        //            });

        //            partNumber++;
        //        }

        //        // Prepare the result for Step Functions
        //        var stepFunctionResponse = new Dictionary<string, string>
        //    {
        //        { "myBucketName", bucketName },
        //        { "myPrefixName", videoName }
        //    };

        //        // Log success
        //        context.Logger.LogInformation($"Processing complete for {objectKey}");

        //        // Return the result for Step Functions
        //        return stepFunctionResponse;
        //    }
        //    catch (Exception e)
        //    {
        //        context.Logger.LogError($"Error processing video {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure the video exists and the bucket is in the same region.");
        //        context.Logger.LogError(e.Message);
        //        context.Logger.LogError(e.StackTrace);
        //        throw;
        //    }
        //}
        var stepFunctionResponse = new Dictionary<string, string>
            {
                { "myBucketName", "thang-test-1" },
                { "myPrefixName", "videoSource" }
            };
        return stepFunctionResponse;
    }


    private List<Stream> CutVideoIntoParts(Stream videoStream, int durationInSeconds)
    {
        var videoParts = new List<Stream>();
        string tempInputPath = "/tmp/inputVideo.mp4";
        string tempOutputFolder = "/tmp/output/";

        // Create the output folder
        Directory.CreateDirectory(tempOutputFolder);

        // Save the input video stream to the local file system
        using (var fileStream = new FileStream(tempInputPath, FileMode.Create, FileAccess.Write))
        {
            videoStream.CopyTo(fileStream);
        }

        // Generate FFmpeg command to cut the video into parts
        string ffmpegCommand = $"-i {tempInputPath} -c copy -map 0 -segment_time {durationInSeconds} -f segment -reset_timestamps 1 {tempOutputFolder}part_%03d.mp4";

        // Run FFmpeg command
        RunFFmpeg(ffmpegCommand);

        // Collect the output video parts as streams
        var outputFiles = Directory.GetFiles(tempOutputFolder, "part_*.mp4");
        foreach (var outputFile in outputFiles)
        {
            var outputStream = new MemoryStream(File.ReadAllBytes(outputFile));
            videoParts.Add(outputStream);
        }

        return videoParts;
    }

    // Helper method to run the FFmpeg command
    private void RunFFmpeg(string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/ffmpeg",  // Path to FFmpeg binary in your Lambda environment or local machine
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        // Check for errors
        string error = process.StandardError.ReadToEnd();
        if (process.ExitCode != 0)
        {
            throw new Exception($"FFmpeg failed with error: {error}");
        }
    }

}