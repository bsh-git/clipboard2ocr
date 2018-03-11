using Microsoft.VisualStudio.TestTools.UnitTesting;
using GoogleVisionApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace GoogleVisionApi.Tests
{
    [TestClass()]
    public class GoogleVisionApiRequestTests
    {
        [TestMethod()]
        public void ConstructorTest0()
        {
            var req = new GoogleVisionApiRequest(new byte[] { 0, 1, 2, 3, 4, 5 });
        }

        [TestMethod()]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ConstructorTest1()
        {
            var req = new GoogleVisionApiRequest("nonexistent.png");
        }

        [TestMethod()]
        [DeploymentItem(@"..\..\TestData\small.png")]
        public void ConstructorTest2()
        {
            var req = new GoogleVisionApiRequest("small.png");
        }


        [TestMethod()]
        [DeploymentItem(@"..\..\TestData\small.png")]
        public void ToJasonTest()
        {
            ToJasonTest(new GoogleVisionApiRequest("small.png"));
		}

		[TestMethod()]
        [DeploymentItem(@"..\..\TestData\small.png")]
        public void ToJasonTestMem()
        {
			using(FileStream fs = new FileStream("small.png", FileMode.Open, FileAccess.Read))
			{
				byte [] imageData;
				imageData = new byte[fs.Length];
				fs.Read(imageData, 0, imageData.Length);

				using(MemoryStream ms = new MemoryStream(imageData)) {
					ToJasonTest(new GoogleVisionApiRequest(ms));
				}
			}
		}

		private void ToJasonTest(GoogleVisionApiRequest req)
		{
            String json = req.ToJson();

            Debug.WriteLine(json);

			String base64encoded = String.Concat( new String[] {
					"iVBORw0KGgoAAAANSUhEUgAAAHIAAABUCAIAAADRSvQBAAAAAXNSR0IArs4c6QAAAARnQU1BAACx",
					"jwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAKcSURBVHhe7drRkeowDIVh6qIg6qGaNEMxrCXi",
					"RJYUsxtyMgl7/icwjoO/9XDvDFyeDBBZIZEVElkhkRUSWSGRFRJZIZEVElkhkRUSWSGRFRJZIZEV",
					"ElkhkRUSWSGRFRJZIZEVElkhkRUSWSGRFRJZIZEVElkhIViH26V0G8anY69R2/X+GF+rPe7X8TXN",
					"L3GeAKzVz6nJsHVSQjOgl9lrwsCJ2pxVtIqFmLQknlVHpilxvjbcySqNquZBLbDaGdH83G3Maqi8",
					"q5ezz79NdWPWhtK5Cp2PrL/JQbZPI528/ppA1k7ZeeyfSIHVsW9z3ZA1odHzOI5lcvOYPDIHvcb/",
					"CaQHzrjG12VkHtJnVnb+jDhfm7GmqqVpXNnaglk756Sk0qb/ZLEaWSGRFRJZIZEVElkhkRUSWSGR",
					"FRJZIZEVElkhkRUSWSGRFRJZIZEVElkhkRUSWSFhWYdb87Xh9LOATysLHfv7QwBr8hVrnhGO13TZ",
					"EtayQvZF7sIy7u+9eUc9rfojgVI+PXo9SuF3BR28P7yVNUFYy3ZMutPKNLVwjPqFVSSro3euA326",
					"+KfZMBRrfcf1YbvFdktqsVC6867I/GK55/K0d+qfdQTWNLmgTBoecVrGlRm9uwvQ9ZgfArLA4oRU",
					"Yw2rXNSfsLrjndYuaSm/NmN9r1bWwhzXPVmb0g0LaW1hv1bCTi+FK+zclfAr24nV7clMmFL4OpoZ",
					"SO3KDi3SxjfStnSbj9vns9W/fb/HhlQrM+J+y7RpkltivIPcORnWcYjfQujTKnlUN0G2HPZsBdNk",
					"0XmGeeav1OWb97NDENamhlC2r9UhGciP0YtjKpk0LSbteRbfh2f9l5EVElkhkRUSWSGRFRJZIZEV",
					"ElkhkRUSWSGRFdDz+QOx+Ho/Gya2SgAAAABJRU5ErkJggg=="
				});

            Assert.IsTrue(json.IndexOf(base64encoded) > 0);

            Debug.WriteLine(String.Format("Current directory={0}", Environment.CurrentDirectory));
			using(StreamWriter ws = new StreamWriter("test.json")) {
                Debug.WriteLine("write JSON to a file");
				ws.Write(json);
                ws.Write("xxx");
                ws.Close();
			}
        }



    }

	
    [TestClass()]
    public class GoogleVisionApiResultTests
    {
		[TestMethod()]
        [DeploymentItem(@"..\..\TestData\result.json")]
		public void FromJsonTest()
		{
			String json;
			using(StreamReader rs = new StreamReader("result.json")) {
				json = rs.ReadToEnd();
			}
			GoogleVisionApiResult res = GoogleVisionApiResult.FromJson(json);
            Debug.WriteLine(res.Responses[0].FullTextAnnotation.Text);
            Assert.IsTrue(res.Responses[0].FullTextAnnotation.Text.IndexOf("ABC") == 0);
        }
	}		
	
    [TestClass()]
	[DeploymentItem(@"..\..\TestData\")]
    public class GoogleVisionApiSessionTests
	{
		private readonly string ApiKey;

		public GoogleVisionApiSessionTests()
		{
			using(StreamReader rs = new StreamReader("api.key")) {
				ApiKey = rs.ReadLine();
			}

		}

        [TestMethod()]
        public void ConstructorTest3()
        {
            var sess = new GoogleVisionApiSession(new GoogleVisionApiRequest("small.png"), ApiKey);
        }

        [TestMethod()]
        public void PostTest()
        {
            var response = PostTest2();
            response.Wait();

            var str = response.Result.Content.ReadAsStringAsync();
            str.Wait();
            Debug.WriteLine(str.Result);
            Assert.AreEqual(HttpStatusCode.OK, response.Result.StatusCode);
        }

        private async Task<HttpResponseMessage> PostTest2()
        {
            Debug.WriteLine("posttest");
            var sess = new GoogleVisionApiSession("small.png", ApiKey);
            await sess.PostAsync();
            Debug.WriteLine("status={0}", new Object[] {sess.HttpResponse.StatusCode});
            return sess.HttpResponse;
        }

		[TestMethod()]
		public void ResultTest()
		{
			var task = ResultTest2();
			task.Wait();

			GoogleVisionApiResult result = task.Result;

			//Assert.IsTrue(result.Succeeded);
            Debug.WriteLine(result.Responses[0].FullTextAnnotation.Text);
			Assert.IsTrue(result.Responses[0].FullTextAnnotation.Text.IndexOf("ABC") == 0);
            Assert.AreEqual(HttpStatusCode.OK, result.HttpStatusCode);

		}

		private async Task<GoogleVisionApiResult> ResultTest2(string apikey = null)
		{
			if (apikey == null)
				apikey = ApiKey;

			var sess = new GoogleVisionApiSession("small.png", apikey);
			await sess.PostAsync();
            Debug.WriteLine("status={0}", new Object[] {sess.HttpResponse.StatusCode});
			GoogleVisionApiResult result = await sess.GetResult();
            return result;
		}

		[TestMethod()]
		public void PostErrorTest()
		{
			var task = ResultTest2("BadKey");
			task.Wait();

			GoogleVisionApiResult result = task.Result;
			//Assert.IsFalse(result.Succeeded);
            Assert.AreNotEqual(HttpStatusCode.OK, result.HttpStatusCode);
			if (result.Error != null) {
				Debug.WriteLine("error code={0} status={2} message={1}",
								new string[] { result.Error.Code, result.Error.Message, result.Error.Status });

			}
			Debug.WriteLine(result.HttpErrorMessage);
		}

	}
}
