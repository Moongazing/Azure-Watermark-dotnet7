using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAO.AzureStorage.Model
{
    public class PictureWatermarkQueue
    {
        public string UserId { get; set; }
        public string City { get; set; }
        public List<string> Pictures { get; set; }
        public string ConnectionId { get; set; }
        public string WatermarkText { get; set; }
    }



}
