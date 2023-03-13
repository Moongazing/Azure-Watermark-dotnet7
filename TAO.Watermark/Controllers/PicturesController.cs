using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using TAO.AzureStorage.Enums;
using TAO.AzureStorage.Model;
using TAO.AzureStorage.Services.Abstract;
using TAO.AzureStorage.Services.Concrete;
using TAO.Watermark.Models;

namespace TAO.Watermark.Controllers
{
    public class PicturesController : Controller
    {   //Because of this properties ;
        //I don't have identity server for this. I am going to build with my last project. 
        public string UserId { get; set; } = "12345";
        public string City { get; set; } = "Kocaeli";

        private readonly INoSqlStorage<UserPicture> _noSqlStorage;
        private readonly IBlobStorage _blobStorage;

        public PicturesController(INoSqlStorage<UserPicture> noSqlStorage, IBlobStorage blobStorage)
        {
            _noSqlStorage = noSqlStorage;
            _blobStorage = blobStorage;
        }


        public async Task<IActionResult> Index()
        {
            ViewBag.UserId = UserId;
            ViewBag.City = City;

            List<FileBlob> fileBlobs = new List<FileBlob>();

            var user = await _noSqlStorage.Get(UserId, City);



            if (user != null)
            {
                user.Paths.ForEach(x =>
                {
                    fileBlobs.Add(new FileBlob
                    {
                        Name = x,
                        Url = $"{_blobStorage.BlobUrl}/{EContainerName.pictures}/{x}"
                    });
                });
            }
            ViewBag.fileBlobs = fileBlobs;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Index(IEnumerable<IFormFile> pictures)
        {
            List<string> pictureList = new List<string>();
            foreach (var item in pictures)
            {
                var newPictureName = $"{Guid.NewGuid()}+{Path.GetExtension(item.FileName)}";

                await _blobStorage.UploadAsync(item.OpenReadStream(), newPictureName, EContainerName.pictures);

                pictureList.Add(newPictureName);
            }

            var isUser = await _noSqlStorage.Get(UserId, City);

            if (isUser != null)
            {
                pictureList.AddRange(isUser.Paths);
            }
            else
            {
                isUser = new UserPicture();
                isUser.RowKey = UserId;
                isUser.PartitionKey = City;
                isUser.Paths = pictureList;

            }

            await _noSqlStorage.Add(isUser);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AddWatermark(PictureWatermarkQueue pictureWatermarkQueue)
        {
            var jsonString = JsonConvert.SerializeObject(pictureWatermarkQueue);

            string jsonStringBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));

            AzQueue azQueue = new("watermarkqueue");

            await azQueue.SendMessageAsync(jsonStringBase64);

            return Ok();
          
            
            
        }


    }
}
