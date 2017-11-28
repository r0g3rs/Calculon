using Lime.Protocol.Serialization.Newtonsoft;
using Newtonsoft.Json.Linq;
using System;
using System.Web.Http;

namespace Calculon.Controllers
{
    public class NotificationsController : ApiController
    {
        // POST api/values
        public void Post([FromBody]JObject jsonObject)
        {
            var envelopeSerializer = new JsonNetSerializer();

            var notification = envelopeSerializer.Deserialize(jsonObject.ToString());

            Console.WriteLine("Received Notification");
        }
    }
}
