using Application.Extensions;
using Application.Services.UploaderServices.DTO;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;


namespace Application.Services.UploaderServices
{
    public class UploaderService : IScopedDependency, IUploaderService
    {
        private readonly IWebHostEnvironment _WebHost;
        //supported types you can add more here...

        private string[] supportedTypes = new[] { "jpg", "jpeg", "png", "webp" };

        public UploaderService(IWebHostEnvironment webHostEnvironment)
        {
            _WebHost = webHostEnvironment;
        }

        /// <summary>
        /// upload as jpg
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public async Task<string> UploadAsJpg(UploadDTO Request)
        {
            var wwwrootPath = _WebHost.WebRootPath;

            //check if null create
            if (string.IsNullOrWhiteSpace(wwwrootPath))
            {
                wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            //get root 
            var rootpath = Path.Combine(wwwrootPath, Request.Path);

            if (!Directory.Exists(rootpath))
                Directory.CreateDirectory(rootpath);

            //check file is not empty
            if (Request.File == null || Request.File.Length == 0)
                return null;

            //make new file name
            var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileNameWithoutExtension(Request.File.FileName) +
                           ".jpg";


            //final path file 
            var systemfilepath = Path.Combine(rootpath, fileName);


            //check for supported ext
            var fileExt = Path.GetExtension(fileName).Substring(1);
            if (!supportedTypes.Contains(fileExt))
            {
                var ErrorMessage = "File Extension Is InValid - Only Upload IMAGE/VIDEO File";
                return ErrorMessage;
            }
            else
            {
                using (FileStream fs = new(systemfilepath, FileMode.Create))
                {
                    using (Image Image = Image.Load(Request.File.OpenReadStream()))
                    {
                        await Image.SaveAsync(fs, new PngEncoder());
                    }
                }

                return $"{Request.Path}/{fileName}";
            }
        }

        /// <summary>
        /// upload as jpeg
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public async Task<string> UploadAsJpeg(UploadDTO Request)
        {
            var wwwrootPath = _WebHost.WebRootPath;

            //check if null create
            if (string.IsNullOrWhiteSpace(wwwrootPath))
            {
                wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            //get root 
            var rootpath = Path.Combine(wwwrootPath, Request.Path);

            if (!Directory.Exists(rootpath))
                Directory.CreateDirectory(rootpath);

            //check file is not empty
            if (Request.File == null || Request.File.Length == 0)
                return null;

            //make new file name
            var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileNameWithoutExtension(Request.File.FileName) +
                           ".jpeg";


            //final path file 
            var systemfilepath = Path.Combine(rootpath, fileName);


            //check for supported ext
            var fileExt = Path.GetExtension(fileName).Substring(1);
            if (!supportedTypes.Contains(fileExt))
            {
                var ErrorMessage = "File Extension Is InValid - Only Upload IMAGE/VIDEO File";
                return ErrorMessage;
            }
            else
            {
                using (FileStream fs = new(systemfilepath, FileMode.Create))
                {
                    using (Image Image = Image.Load(Request.File.OpenReadStream()))
                    {
                        await Image.SaveAsync(fs, new PngEncoder());
                    }
                }

                return $"{Request.Path}/{fileName}";
            }
        }

        /// <summary>
        /// upload as png
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public async Task<string> UploadAsPng(UploadDTO Request)
        {
            var wwwrootPath = _WebHost.WebRootPath;

            //check if null create
            if (string.IsNullOrWhiteSpace(wwwrootPath))
            {
                wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            //get root 
            var rootpath = Path.Combine(wwwrootPath, Request.Path);

            if (!Directory.Exists(rootpath))
                Directory.CreateDirectory(rootpath);

            //check file is not empty
            if (Request.File == null || Request.File.Length == 0)
                return null;

            //make new file name
            var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileNameWithoutExtension(Request.File.FileName) +
                           ".png";


            //final path file 
            var systemfilepath = Path.Combine(rootpath, fileName);


            //check for supported ext
            var fileExt = Path.GetExtension(fileName).Substring(1);
            if (!supportedTypes.Contains(fileExt))
            {
                var ErrorMessage = "File Extension Is InValid - Only Upload IMAGE/VIDEO File";
                return ErrorMessage;
            }
            else
            {
                using (FileStream fs = new(systemfilepath, FileMode.Create))
                {
                    using (Image Image = Image.Load(Request.File.OpenReadStream()))
                    {
                        await Image.SaveAsync(fs, new PngEncoder());
                    }
                }

                return $"{Request.Path}/{fileName}";
            }
        }


        /// <summary>
        /// upload with webp
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public async Task<string> UploadAsWebp(UploadDTO Request)
        {
            var wwwrootPath = _WebHost.WebRootPath;

            //check if null create
            if (string.IsNullOrWhiteSpace(wwwrootPath))
            {
                wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            //get root 
            var rootpath = Path.Combine(wwwrootPath, Request.Path);

            if (!Directory.Exists(rootpath))
                Directory.CreateDirectory(rootpath);

            //check file is not empty
            if (Request.File == null || Request.File.Length == 0)
                return null;

            //make new file name
            var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileNameWithoutExtension(Request.File.FileName) +
                           ".webp";


            //final path file 
            var systemfilepath = Path.Combine(rootpath, fileName);


            //check for supported ext
            var fileExt = Path.GetExtension(fileName).Substring(1);
            if (!supportedTypes.Contains(fileExt))
            {
                var ErrorMessage = "File Extension Is InValid - Only Upload IMAGE/VIDEO File";
                return ErrorMessage;
            }
            else
            {
                using (FileStream fs = new(systemfilepath, FileMode.Create))
                {
                    using (Image Image = Image.Load(Request.File.OpenReadStream()))
                    {
                        await Image.SaveAsync(fs, new WebpEncoder
                        {
                            Quality = 70,
                            FileFormat = WebpFileFormatType.Lossy,
                        });
                    }
                }

                return $"{Request.Path}/{fileName}";
            }
        }
    }
}