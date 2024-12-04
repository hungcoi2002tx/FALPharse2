using AuthenExamReceiveData.DTOs;

namespace AuthenExamReceiveData.Service.Implement
{
    public interface IImageService
    {
        Task<ResponseResult> SaveImageToFile(SaveImageDTO request);
    }
}
