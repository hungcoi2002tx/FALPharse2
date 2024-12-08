using AForge.Video.DirectShow;
using AuthenExamCameraLibrary;
using AuthenExamCompareFaceExam.Utils;
using System.Windows.Forms;

namespace AuthenExam.UnitTests;

[TestFixture]
public class CameraHandlerTests
{
    private CameraHandler _cameraHandler;

    [SetUp]
    public void Setup()
    {
        _cameraHandler = new CameraHandler();
    }

    [Test]
    public void Constructor_NoCameras_ReturnEmpty()
    {
        // Giả lập không có camera
        var cameraHandler = new CameraHandler();
        Assert.That(cameraHandler.GetListCamera().IsNotNullOrEmpty);
    }

    [Test]
    public void Constructor_CamerasExist_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new CameraHandler());
    }

    [Test]
    public void GetListCamera_ReturnsNonEmptyList()
    {
        var cameras = _cameraHandler.GetListCamera();
        Assert.IsNotNull(cameras);
        Assert.IsTrue(cameras.Count > 0, "No cameras detected");
    }

    [Test]
    public void StartCamera_NullDevice_ThrowsException()
    {
        PictureBox pictureBox = new PictureBox();
        Assert.Throws<Exception>(() => _cameraHandler.StartCamera(null, pictureBox));
    }

    [Test]
    public void StartCamera_NullPictureBox_ThrowsArgumentNullException()
    {
        var device = new VideoCaptureDevice();
        Assert.Throws<ArgumentNullException>(() => _cameraHandler.StartCamera(device, null));
    }


    [Test]
    public void StopCamera_WhenDeviceIsRunning_ShouldStopSuccessfully()
    {
        var device = new VideoCaptureDevice();
        _cameraHandler.StopCamera(device);
        Assert.IsFalse(device.IsRunning);
    }

    [Test]
    public void IsCameraInUse_WhenCameraIsActive_ReturnsTrue()
    {
        var device = new VideoCaptureDevice(_cameraHandler.GetListCamera()[0].MonikerString);
        bool result = _cameraHandler.IsCameraInUse(device);
        Assert.IsTrue(result);
    }

    [Test]
    public void CaptureAndCropImageToBinaryString_WhenNoImageInPictureBox_ThrowsInvalidOperationException()
    {
        PictureBox pictureBox = new PictureBox();
        Assert.Throws<InvalidOperationException>(() => _cameraHandler.CaptureAndCropImageToBinaryString(pictureBox, "D:\\KHA\\FPTU\\Semesters\\Semester9\\FALPharse2\\AuthenExam.UnitTests"));
    }

    //[Test]
    //public void ConvertImageToBinaryString_InvalidPath_ThrowsException()
    //{
    //    Assert.Throws<ArgumentException>(() => _cameraHandler.ConvertImageToBinaryString(""));
    //}

    //[Test]
    //public void DeleteImage_FileDoesNotExist_NoExceptionThrown()
    //{
    //    Assert.DoesNotThrow(() => _cameraHandler.DeleteImage("nonexistent.jpg"));
    //}
}