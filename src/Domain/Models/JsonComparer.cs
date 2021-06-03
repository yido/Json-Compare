using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Domain.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Domain.Models
{
    public class JsonComparer
    {
        private protected JObject LeftObj { get; private set; }
        private protected JObject RightObj { get; private set; }
        public JsonComparer Left(JObject obj) { this.LeftObj = obj; return this; }
        public JsonComparer Right(JObject obj) { this.RightObj = obj; return this; }

        public ComparerResponse Compare()
        {
            var diffResult = new JObject();
            try
            {
                Diff(this.LeftObj, this.RightObj, ref diffResult);
            }
            catch (Exception ex)
            {
                if (ex is ComparerException)
                {
                    return new ComparerResponse()
                    {
                        Message = ex.Message,
                        Data = new {StackTrace = ex.StackTrace}
                    };
                }

                throw ex;
            }

            Func<ResponseType, string> ToString = (responseType) => (Enum.GetName(typeof(ResponseType), responseType)).Replace("_", " ");

            return new ComparerResponse
            {
                Message = diffResult.Count == 0 ? ToString(ResponseType.OBJECTS_ARE_EQUAL) : ToString(ResponseType.OBJECTS_ARE_NOT_EQUAL),
                Data = JsonConvert.DeserializeObject<ExpandoObject>(diffResult.ToString(), new ExpandoObjectConverter())
            };
        }


        //
        // Summary:
        //     Compares the values of two tokens, including the values of all descendant tokens.
        //
        // Parameters:
        //   left:
        //     The first JToken to compare.
        //
        //   right:
        //     The second JToken to compare. 
        //
        //   key:
        //     optional param, used to draw the output JObject
        //
        //   output (by ref):
        //     The diffrence detail will be copied to this JObject
        public void Diff(JToken left, JToken right, ref JObject output, string key = "")
        {
            //~ Used Newtonsoft.Json's Lib to compare Objects. One can reimplement this functionality ~// 
            //~ Locally but I assumed this is out of the scope of this Proj ~//
            //~ Check if both JToken Types are equal  ~//
            if (JToken.DeepEquals(left, right))
                return;


            //~ Check if both JToken Types are of same Type ~//
            if ((left.Type != right.Type) && (left != null && right != null))
                throw new ComparerException(ResponseType.OBJECTS_ARE_NOT_OF_SAME_SIZE);

            //~ Compare JToken Types according to thier types (note here that one of the values could be of JTokenType.Null | Null )~// 
            var type = left == null || left.Type == JTokenType.Null ? right.Type : left.Type;

            switch (type)
            {
                case JTokenType.Object:
                    CompareObjects(left, right, output, key); return;

                case JTokenType.Array:
                    CompareArrays(left, right, output, key); return;

                //~ Note: Here I am only dealing with JSON's with all values in string format (i.e {"input":"testValue"}, both key & value inside quotes ) ~//
                //~ If this condition is not met an Exception of "OBJECTS_ARE_NOT_OF_SAME_SIZE" will be thrown ~//
                case JTokenType.String:
                case JTokenType.Null:
                case JTokenType.Undefined:
                    CompareValues(left, right, output, key); return;

                default:
                    throw new ComparerException($"Hey, at the moment I could not recognize the property type : {type}!");
            }
        }

        //
        // Summary:
        //     Compares array values of two last Child tokens, if there are any items that are not in common, 
        //     They will be copied to the output JObject inside offset property 
        //     Note: Same notes on my assumption of only string key,value JSON properties, please refer 91 
        //
        private void CompareArrays(JToken left, JToken right, JObject output, string key)
        {
            //~ Check if the Array is of Literal | Object types ~//
            var type = left.Count() > 0 ? left.First.Type : right.First.Type;

            var leftArr = type == JTokenType.String ? left.Values().Select(l => l.Value<string>()) : left.Values<object>();
            var rightArr = type == JTokenType.String ? right.Values().Select(l => l.Value<string>()) : right.Values<object>();

            var left_offset = leftArr.Except(rightArr);
            var right_offset = rightArr.Except(leftArr);

            var offset = left_offset.Concat(right_offset); ;

            if (offset.Count() > 0)
                output.Add(key, JToken.FromObject(new { Offset = offset, Length = offset.Count() }));

            return;
        }

        //
        // Summary:
        //     Compares the values of two Object type JTokens.
        //     Note: this method will call back to the caller Diff();
        //     Hence making the Parent  Diff() function a Recursive function
        //
        private void CompareObjects(JToken left, JToken right, JObject output, string key)
        {
            //~ Check if both have same set of keys ~// 
            var hasSameKeys = (left as JObject).Properties().Select(_ => _.Name).OrderBy(x => x).SequenceEqual((right as JObject).Properties().Select(_ => _.Name).OrderBy(y => y));

            if ((left as JObject).Count != (right as JObject).Count || !hasSameKeys)
                throw new ComparerException(ResponseType.OBJECTS_ARE_NOT_OF_SAME_SIZE);

            foreach (JProperty property in (left as JObject).Properties())
            {
                var _left = left.SelectToken(property.Name);
                var _right = right.SelectToken(property.Name);

                if (!string.IsNullOrEmpty(key) && !output.ContainsKey(key))
                    output.Add(key, JToken.FromObject(new { }));

                var _o = !string.IsNullOrEmpty(key) ? output.SelectToken(key) as JObject : output;

                Diff(_left, _right, ref _o, property.Name);
            }

            return;
        }

        //
        // Summary:
        //     Compares the values of two last Child tokens, if the values are not the same, 
        //     both values will be copied to the output JObject inside offset property  
        //     Note: Same notes on my assumption of only string key,value JSON properties, please refer 81 
        //
        private void CompareValues(JToken left, JToken right, JObject output, string key)
        {
            var (leftStrg, rightStrg) = (left.Value<string>(), right.Value<string>());

            var offset = new List<string>();
            if (leftStrg != rightStrg)
            {
                if (!string.IsNullOrEmpty(leftStrg))
                    offset.Add(leftStrg);
                if (!string.IsNullOrEmpty(rightStrg))
                    offset.Add(rightStrg);
            }
            if (offset.Count > 0)
                output.Add(key, JToken.FromObject(new { Offset = offset, Length = offset.Count }));
            return;
        }
    }
}