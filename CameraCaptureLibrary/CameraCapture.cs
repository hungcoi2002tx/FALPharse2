using System;
using System.Drawing;
using System.IO;
using AForge.Video;
using AForge.Video.DirectShow;

namespace CameraCaptureLibrary
{
    public class CameraCapture
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private string captureImagePath;

        public event Action<string> OnImageCaptured;

        public void StartCamera()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                videoSource.Start();
            }
            else
            {
                throw new Exception("No webcam found.");
            }
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap image = (Bitmap)eventArgs.Frame.Clone();
            captureImagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "capture.jpg");
            image.Save(captureImagePath);
            videoSource.SignalToStop();

            OnImageCaptured?.Invoke(captureImagePath);
        }

        public void StopCamera()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }
    }
}
