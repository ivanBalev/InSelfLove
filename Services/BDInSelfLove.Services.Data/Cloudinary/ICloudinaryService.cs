using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.CloudinaryService
{
    public interface ICloudinaryService
    {
        public Task<string> UploadPicture(IFormFile pictureFile, string fileName);
    }
}
