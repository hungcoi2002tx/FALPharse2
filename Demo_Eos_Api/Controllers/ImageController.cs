﻿using AuthenExamReceiveData.Dtos;
using AuthenExamReceiveData.Service.Implement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthenExamReceiveData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("SaveFile")]
        public async Task<IActionResult> SaveImageFile(SaveImageDTO request)
        {
            var res = await _imageService.SaveImageToFile(request);
            return StatusCode((int)res.StatusCode!, res);
        } 
    }
}
