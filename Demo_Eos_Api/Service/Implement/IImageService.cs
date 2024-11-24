using Demo_Eos_Api.DTOs;

namespace Demo_Eos_Api.Service.Implement
{
    public interface IImageService
    {
        Task<ResponseResult> SaveImageToFile(SaveImageDTO request);
    }
}
