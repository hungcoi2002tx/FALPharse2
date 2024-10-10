using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CloudWatchLogger
{
    private static readonly string logGroupName = "/aws/lambda/thang-test";
    private static readonly string logStreamName = "thang-test";
    private readonly AmazonCloudWatchLogsClient _cloudWatchClient;

    public CloudWatchLogger()
    {
        _cloudWatchClient = new AmazonCloudWatchLogsClient();
    }

    public async Task LogMessageAsync(string message)
    {
        await CreateLogGroupAsync();
        await CreateLogStreamAsync();

        var timestamp = DateTime.UtcNow;
        var sequenceTokenResponse = await _cloudWatchClient.DescribeLogStreamsAsync(new DescribeLogStreamsRequest
        {
            LogGroupName = logGroupName,
            LogStreamNamePrefix = logStreamName
        });

        var logStream = sequenceTokenResponse.LogStreams[0];
        var sequenceToken = logStream.UploadSequenceToken;

        var logEvent = new InputLogEvent
        {
            Message = message,
            Timestamp = timestamp
        };

        var putLogEventsRequest = new PutLogEventsRequest
        {
            LogGroupName = logGroupName,
            LogStreamName = logStreamName,
            LogEvents = new List<InputLogEvent> { logEvent },
            SequenceToken = sequenceToken
        };

        await _cloudWatchClient.PutLogEventsAsync(putLogEventsRequest);
    }

    private async Task CreateLogGroupAsync()
    {
        try
        {
            await _cloudWatchClient.CreateLogGroupAsync(new CreateLogGroupRequest { LogGroupName = logGroupName });
        }
        catch (ResourceAlreadyExistsException)
        {
            // Log Group đã tồn tại, bỏ qua lỗi này
        }
    }

    private async Task CreateLogStreamAsync()
    {
        try
        {
            await _cloudWatchClient.CreateLogStreamAsync(new CreateLogStreamRequest
            {
                LogGroupName = logGroupName,
                LogStreamName = logStreamName
            });
        }
        catch (ResourceAlreadyExistsException)
        {
            // Log Stream đã tồn tại, bỏ qua lỗi này
        }
    }
}
