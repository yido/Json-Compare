using System;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Domain.UnitTests
{
    [TestClass]
    public class JsonComparerUnitTest
    {  

        [TestMethod]
        public void Should_Compare_And_Result_The_Difference_When_Left_And_Right_JSONs_Are_Not_Equal_Size()
        {
            var left = JObject.Parse(@"{""Agent"":""1289"",""Beneficiary"":{""Main"":""true"",""Values"":[""1"",""2"",""3"",""4"",""5""]},""BillingDate"":""2021-04-04T13:33:03.969Z"",""BillingInvoice"":""914548"",""Claim"":{""Customer"":""112233"",""Products"":[""one"",""two"",""three""]}}"); 
            var unequal_size_right = JObject.Parse(@"{""Agent"":""1289"",""Beneficiary"":{""Main"":""true"",""Values"":[""1""]}}");
 
            var diff = new JObject();

            Assert.ThrowsException<ComparerException>(() => new JsonComparer().Diff(left, unequal_size_right, ref diff));

            try
            {
                new JsonComparer().Diff(left, unequal_size_right,ref diff);
            }
            catch (Exception ex)
            {
                if (ex is ComparerException)
                    Assert.AreEqual(ex.Message, "Message: OBJECTS ARE NOT OF SAME SIZE");
            }  
        }

        [TestMethod]
        public void Should_Compare_And_Result_The_Difference_When_Left_And_Right_JSONs_Are_Of_Equal_Size()
        {
            var left = JObject.Parse(@"{""Agent"":""1289"",""Beneficiary"":{""Main"":""true"",""Values"":[""1"",""2"",""3"",""4"",""5""]},""BillingDate"":""2021-04-04T13:33:03.969Z"",""BillingInvoice"":""914548"",""Claim"":{""Customer"":""112233"",""Products"":[""one"",""two"",""three""]}}");
            var right = JObject.Parse(@"{""Agent"":""1289"",""Beneficiary"":{""Main"":""true"",""Values"":[""5"",""6"",""7""]},""BillingDate"":""2021-04-04T13:33:03.969Z"",""BillingInvoice"":""914548"",""Claim"":{""Customer"":""33"",""Products"":[""one"",""three"",""six""]}}");

            var expected_diff = JObject.Parse(@"{""Beneficiary"":{""Values"":{""Offset"":[""1"",""2"",""3"",""4"",""6"",""7""],""Length"":6}},""Claim"":{""Customer"":{""Offset"":[""112233"",""33""],""Length"":2},""Products"":{""Offset"":[""two"",""six""],""Length"":2}}}");
          
            var diff = new JObject(); 

            //~ Compare Equal Size Objects ~//
            new JsonComparer().Diff(left, right, ref diff);

            Assert.AreEqual(diff.ToString(), expected_diff.ToString());
        }
        [TestMethod]
        public void Should_Compare_And_Result_Empty_Difference_When_Left_And_Right_JSONs_Are_Equal()
        {
            var left = JObject.Parse(@"{""Agent"":""1289"",""Beneficiary"":{""Main"":""true"",""Values"":[""1"",""2"",""3"",""4"",""5""]},""BillingDate"":""2021-04-04T13:33:03.969Z"",""BillingInvoice"":""914548"",""Claim"":{""Customer"":""112233"",""Products"":[""one"",""two"",""three""]}}");
             var diff = new JObject();

            //~ Compare Equal Size Objects ~//
            new JsonComparer().Diff(left, left, ref diff);

            Assert.AreEqual(diff.Count, 0);
            Assert.AreEqual(diff.ToString(), "{}");
        } 
    }
}