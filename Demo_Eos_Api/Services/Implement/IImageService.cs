using AuthenExamReceiveData.Dtos;

namespace AuthenExamReceiveData.Service.Implement
{
    public interface IImageService
    {
        Task<ResponseResultDto> SaveImageToFile(SaveImageDTO request);
    }
}
