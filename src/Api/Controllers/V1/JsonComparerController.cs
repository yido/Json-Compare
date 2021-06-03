using System;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{

    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("v{version:apiVersion}/diff")]
    public class JsonComparerController : ControllerBase
    {

        private IMemoryCache _cache;
        public JsonComparerController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        /// <summary>
        /// Submit the first Base64 Encoded JSON Object for Comparison (Left Obj)
        /// The value will be stored in memory under a given {id}
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /V1/diff/{id}/Left
        ///
        /// Sample response body:
        ///
        ///     id: 9245fe4a-d402-451c-b9ed-9c1a04247482
        ///     Base64String = eyJBZ2VudCI6IjEyODkiLCJCZW5lZmljaWFyeSI6eyJNYWluIjoidHJ1ZSIsIlZhbHVlcyI6WyIxIiwiMiIsIjMiLCI0IiwiNSJdfSwiQmlsbGluZ0RhdGUiOiIyMDIxLTA0LTA0VDEzOjMzOjAzLjk2OVoiLCJCaWxsaW5nSW52b2ljZSI6IjkxNDU0OCIsIkNsYWltIjp7IkN1c3RvbWVyIjoiMTEyMjMzIiwiUHJvZHVjdHMiOlsib25lIiwidHdvIiwidGhyZWUiXX19
        ///     
        ///     Actual Json:
        ///     {
        ///      "Agent":"1289",
        ///      "Beneficiary":
        ///       {
        ///         "Main":"true",
        ///         "Values":[
        ///            "1",
        ///            "2",
        ///            "3",
        ///            "4",
        ///            "5"
        ///         ]
        ///       },
        ///      "BillingDate":"2021-04-04T13:33:03.969Z",
        ///      "BillingInvoice":"914548",
        ///      "Claim":{
        ///         "Customer":"112233",
        ///         "Products":[
        ///            "one",
        ///            "two",
        ///            "three"
        ///          ]
        ///        }
        ///    }
        /// </remarks>
        /// <param name="id">ID</param>
        /// <param name="base64String">Base64 Encoded Json String</param>
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(string), Description = "Return list of WeatherForecasts")]
        [Route("{id}/Left")]
        [HttpPost]
        public async Task<IActionResult> Left(string id, [FromBody] string base64String)
        {
            //~ Save on the Cache Memory, One can use a custom singleton instance of HashTable, Dictionary, Tuples data structure ~// 
            string key = $"LEFT_{id}";
            string value;
            if (!_cache.TryGetValue<string>(id, out value))
            {
                if (value == null || !value.Equals(base64String))
                    _cache.Set<string>(key, base64String);
            }
            else
            {
                //~ Store new value for {id} ~//
                _cache.Set(id, base64String);
            }
            return Ok("Your input has been successfully saved!");
        }

        /// <summary>
        /// Submit the second Base64 Encoded JSON Object for Comparison (Right Obj)
        /// The value will be stored in memory under a given {id}
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /V1/diff/{id}/Right
        ///
        ///     id: 9245fe4a-d402-451c-b9ed-9c1a04247482 
        ///     Base64String = eyJBZ2VudCI6IjEyODkiLCJCZW5lZmljaWFyeSI6eyJNYWluIjoidHJ1ZSIsIlZhbHVlcyI6WyI1IiwiNiIsIjciXX0sIkJpbGxpbmdEYXRlIjoiMjAyMS0wNC0wNFQxMzozMzowMy45NjlaIiwiQmlsbGluZ0ludm9pY2UiOiI5MTQ1NDgiLCJDbGFpbSI6eyJDdXN0b21lciI6IjMzIiwiUHJvZHVjdHMiOlsib25lIiwidGhyZWUiLCJzaXgiXX19
        /// Sample response body: 
        ///     
        ///     Actual Json:
        ///     {
        ///        "Agent":"1289",
        ///        "Beneficiary":{
        ///           "Main":"true",
        ///           "Values":[
        ///              "5",
        ///              "6",
        ///              "7"
        ///           ]
        ///         },
        ///        "BillingDate":"2021-04-04T13:33:03.969Z",
        ///        "BillingInvoice":"914548",
        ///        "Claim":{
        ///           "Customer":"33",
        ///           "Products":[
        ///              "one",
        ///              "three",
        ///              "six"
        ///           ]
        ///         }
        ///         }
        /// </remarks>
        /// <param name="id">ID</param>
        /// <param name="base64String">Base64 Encoded Json String</param>
        [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(object), Description = "Returns a new department")]
        [Route("{id}/Right")]
        [HttpPost]
        public async Task<IActionResult> Right(string id, [FromBody] string base64String)
        {
            //~ Save on the Cache Memory, One can use a custom singleton instance of HashTable, Dictionary, Tuples data structure ~// 
            string key = $"RIGHT_{id}";
            string value;
            if (!_cache.TryGetValue<string>(id, out value))
            {
                if (value == null || !value.Equals(base64String))
                    _cache.Set<string>(key, base64String);
            }
            else
            {
                //~ Store new value for {id} ~//
                _cache.Set(id, base64String);
            }
            return Ok("Your input has been successfully saved!");
        }

        /// <summary>
        /// Get's the difference of two JSON Objects (Left & Right under a given {id})
        /// </summary>
        /// <remarks>
        /// Sample request body:
        ///
        ///     id: 9245fe4a-d402-451c-b9ed-9c1a04247482 
        /// 
        /// Sample response :
        /// 
        ///     {
        ///      "Beneficiary":{
        ///         "Values":{
        ///            "Offset":[
        ///               "1",
        ///               "2",
        ///               "3",
        ///               "4",
        ///               "6",
        ///               "7"
        ///            ],
        ///            "Length":6
        ///       }
        ///       },
        ///      "Claim":{
        ///       "Customer":{
        ///           "Offset":[
        ///               "112233",
        ///               "33"
        ///            ],
        ///            "Length":2
        ///       },
        ///         "Products":{
        ///           "Offset":[
        ///               "two",
        ///               "six"
        ///            ],
        ///            "Length":2
        ///       }
        ///       }
        ///       }
        /// 
        /// </remarks>
        /// <param name="id">ID</param>

        [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(object), Description = "Returns a new department")]
        [Route("{id}")]
        [HttpPost]
        public async Task<IActionResult> Diff(string id)
        {
            string leftBase64String, rightBase64String;

            _cache.TryGetValue<string>($"LEFT_{id}", out leftBase64String);
            _cache.TryGetValue<string>($"RIGHT_{id}", out rightBase64String);

            if (string.IsNullOrEmpty(leftBase64String) || string.IsNullOrEmpty(rightBase64String))
                return BadRequest("Either Left or Right Object is not set properly!");


            var left = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(leftBase64String)));
            var right = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(rightBase64String)));

            ComparerResponse response = null;

            try
            {
                response = new JsonComparer()
                                        .Left(left)
                                        .Right(right)
                                        .Compare();
            }
            catch (System.Exception ex)
            {
                return Ok(new { message = "Sorry, error happened during processing :( ", error = ex });
            } 
            return Ok(response);
        }
    }

}