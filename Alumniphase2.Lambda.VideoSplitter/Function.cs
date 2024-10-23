using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using MediaToolkit.Model;
using MediaToolkit.Options;
using MediaToolkit;
using System.Diagnostics;
using System.Text.RegularExpressions;

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

    public class StepFunctionResponse
    {
        public string myBucketName { get; set; }
        public string myPrefixName { get; set; }
        public string myObjectKey { get; set; }
    }
    public async Task<StepFunctionResponse> FunctionHandler(VideoProcessingPayload payload, ILambdaContext context)
    {
        try
        {
            string bucketName = payload.BucketName;
            string objectKey = $"{payload.Prefix}/{payload.Key}";

            context.Logger.LogInformation($"Processing video: {objectKey}");

            using var videoStream = await this.S3Client.GetObjectStreamAsync(bucketName, objectKey, null);

            // Process video using FFmpeg
            string videoName = Path.GetFileNameWithoutExtension(objectKey);
            List<Stream> videoParts = await CutVideoIntoParts(videoStream, durationInSeconds: 60, context);

            // Upload each part asynchronously
            var uploadTasks = new List<Task>();
            int partNumber = 1;
            foreach (var part in videoParts)
            {
                string partKey = $"{videoName}/part_{partNumber}.mp4";
                context.Logger.LogInformation($"Uploading part: {partKey}");

                uploadTasks.Add(UploadPartToS3(bucketName, partKey, part));
                partNumber++;
            }

            await Task.WhenAll(uploadTasks);

            // Prepare the result for Step Functions
            var stepFunctionResponse = new StepFunctionResponse
            {
                myBucketName = bucketName,
                myPrefixName = videoName,
                myObjectKey = payload.Key,
            };

            // Log success
            context.Logger.LogInformation($"Processing complete for {objectKey}");

            return stepFunctionResponse;
        }
        catch (Exception e)
        {
            context.Logger.LogError($"Error processing video {payload.Key} from bucket {payload.BucketName}. Error: {e.Message}");
            throw;
        }
    }

    private async Task UploadPartToS3(string bucketName, string key, Stream part)
    {
        await this.S3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = part
        });
    }
    private async Task<List<Stream>> CutVideoIntoParts(Stream videoStream, int durationInSeconds, ILambdaContext context)
    {
        var videoParts = new List<Stream>();
        string tempInputPath = "/tmp/inputVideo.mp4";
        string tempOutputFolder = "/tmp/output/";

        // Ensure output directory exists
        if (!Directory.Exists(tempOutputFolder))
        {
            Directory.CreateDirectory(tempOutputFolder);
        }
        context.Logger.LogInformation($"Output directory created at: {tempOutputFolder}");

        // Write the videoStream to a temporary file in /tmp
        using (var fileStream = new FileStream(tempInputPath, FileMode.Create, FileAccess.Write))
        {
            await videoStream.CopyToAsync(fileStream);
        }

        context.Logger.LogInformation($"Input video file written to: {tempInputPath}");

        // Update this path to where your FFmpeg binary is stored (inside the Lambda layer)
        var ffmpegPath = "/opt/ffmpeg-layer/bin/ffmpeg-git-20240629-arm64-static/ffmpeg";   // Ensure this is the correct path for FFmpeg

        // Check if the FFmpeg binary exists
        if (!File.Exists(ffmpegPath))
        {
            context.Logger.LogError("FFmpeg binary not found at the specified path.");
            throw new FileNotFoundException("FFmpeg binary not found.");
        }

        // Get the total duration of the video file using FFmpeg
        var totalDuration = GetVideoDuration(ffmpegPath, tempInputPath, context);
        context.Logger.LogInformation($"Total duration of video: {totalDuration} seconds");

        for (int i = 0; i * durationInSeconds < totalDuration; i++)
        {
            string tempOutputPath = Path.Combine(tempOutputFolder, $"part_{i:D3}.mp4");

            // Construct your FFmpeg command
            string ffmpegArgs = $"-ss {i * durationInSeconds} -i {tempInputPath} -c copy -t {durationInSeconds} {tempOutputPath}";

            // Run FFmpeg to cut the video
            var processOutput = RunFFmpegProcess(ffmpegPath, ffmpegArgs, context);
            context.Logger.LogInformation(processOutput);

            // Convert the output part to a stream and add it to the list
            using (var tempFileStream = new FileStream(tempOutputPath, FileMode.Open, FileAccess.Read))
            {
                var outputStream = new MemoryStream();
                await tempFileStream.CopyToAsync(outputStream);
                outputStream.Position = 0;
                videoParts.Add(outputStream);
            }

            File.Delete(tempOutputPath);
            context.Logger.LogInformation($"Deleted temporary output file: {tempOutputPath}");
        }

        File.Delete(tempInputPath);
        context.Logger.LogInformation($"Deleted temporary input file: {tempInputPath}");

        return videoParts;
    }

    // Method to run the FFmpeg process
    private string RunFFmpegProcess(string ffmpegPath, string arguments, ILambdaContext context)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                context.Logger.LogError($"FFmpeg error: {error}");
                throw new Exception($"FFmpeg failed with exit code {process.ExitCode}. Error: {error}");
            }

            return output + error;
        }
    }

    // Method to get the video duration using FFmpeg
    private double GetVideoDuration(string ffmpegPath, string inputPath, ILambdaContext context)
    {
        string ffmpegArgs = $"-i {inputPath}";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = ffmpegArgs,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.Start();

            string output = process.StandardError.ReadToEnd();  // FFmpeg prints info to stderr
            process.WaitForExit();

            var match = Regex.Match(output, @"Duration: (\d+):(\d+):(\d+.\d+)");
            if (match.Success)
            {
                var hours = int.Parse(match.Groups[1].Value);
                var minutes = int.Parse(match.Groups[2].Value);
                var seconds = double.Parse(match.Groups[3].Value);

                return (hours * 3600) + (minutes * 60) + seconds;
            }

            context.Logger.LogError("Could not extract video duration.");
            throw new Exception("Failed to retrieve video duration from FFmpeg.");
        }
    }



}