using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GoogleVisionApi
{
    public class AnnotateImageRequest
    {

        private class Image
        {
            [JsonProperty("content")]
            String content;

            public Image()
            {
                content = "";
            }

          
            public Image(Byte [] imageData) : this()
			{
				content = Convert.ToBase64String(imageData);
			}
          
         
        }
        private class Feature
        {
            [JsonProperty("type")]
            string type;
            [JsonProperty("maxResults")]
            int maxResults;

            public Feature()
            {
                type = "TEXT_DETECTION";
                maxResults = 20;
            }
        }

        [JsonProperty("image")]
        Image image;

        [JsonProperty("features")]
        Feature[] features;

        public AnnotateImageRequest(byte [] imageData)
        {
            image = new Image(imageData);
            features = new Feature[1];
            features[0] = new Feature();
       }


    }

    public class GoogleVisionApiRequest
    {
        [JsonProperty("requests")]
        AnnotateImageRequest[] requests;

		private void SetRequests(Byte [] imageData)
		{
            requests = new AnnotateImageRequest[1] { new AnnotateImageRequest(imageData) };
		}

		private void SetRequests(Stream stream)
		{
			byte [] imageData;
			imageData = new byte[stream.Length];
			stream.Read(imageData, 0, imageData.Length);
			SetRequests(imageData);
		}

		public GoogleVisionApiRequest(Byte [] imageData)
		{
			SetRequests(imageData);
		}


		public GoogleVisionApiRequest(Stream stream)
		{
			SetRequests(stream);
		}

		public GoogleVisionApiRequest(string filename)
		{
			using(FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				SetRequests(fs);
				fs.Close();
			}

		}

        public String ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

  

	}

	public class GoogleVisionApiResult
	{

		public class FullTextAnnotation
		{
            [JsonProperty("text")]
            public String Text;       
		}

		public class Response
		{
			[JsonProperty("fullTextAnnotation")]
			public FullTextAnnotation FullTextAnnotation;
			
		}

		public class ErrorDescription
		{
			[JsonProperty("code")]
			public string Code;
			[JsonProperty("message")]
			public string Message;
			[JsonProperty("status")]
			public string Status;
		}

		[JsonProperty("responses")]
		public Response [] Responses;
		 
		[JsonProperty("error")]
		public ErrorDescription Error;

		public bool Succeeded { get { return HttpStatusCode == HttpStatusCode.OK; } }

		[JsonIgnore]
		public HttpStatusCode HttpStatusCode;

		[JsonIgnore]
		public readonly string HttpErrorMessage;
		
		public GoogleVisionApiResult(string msg, HttpStatusCode httpStatus)
		{
			this.HttpErrorMessage = msg;
			this.HttpStatusCode = httpStatus;
		}
			

		public static GoogleVisionApiResult FromJson(String json, HttpStatusCode httpStatus = HttpStatusCode.OK)
		{
			try {
				GoogleVisionApiResult inst = JsonConvert.DeserializeObject<GoogleVisionApiResult>(json);
				inst.HttpStatusCode = httpStatus;
				return inst;
			}
			catch {
				return new GoogleVisionApiResult(json, httpStatus);
			}
		}

		public String GetText()
		{
			return Responses[0].FullTextAnnotation.Text;
		}
	}

	public class GoogleVisionApiSession
	{

		private HttpResponseMessage httpResponse;
		public HttpResponseMessage HttpResponse { get { return httpResponse; } }
        private const string uri = @"https://vision.googleapis.com/v1/images:annotate?key=";
		private String ApiKey;
        private HttpClient client;
		private GoogleVisionApiRequest request;


		public GoogleVisionApiSession(String apiKey)
		{
			ApiKey = apiKey;
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public GoogleVisionApiSession(GoogleVisionApiRequest req, String apiKey) : this(apiKey)
		{
			request = req;
		}

		public  GoogleVisionApiSession(Byte [] imageData, String apiKey) : this(new GoogleVisionApiRequest(imageData), apiKey) {}

		public  GoogleVisionApiSession(String filename, String apiKey) : this(new GoogleVisionApiRequest(filename), apiKey) {}
		
        public async Task PostAsync()
        {
			httpResponse = null;
            httpResponse = await client.PostAsync(uri + ApiKey, new StringContent(request.ToJson()));
        }

		public async Task<GoogleVisionApiResult> GetResult()
		{
			if (httpResponse == null)
				return null;
			if (httpResponse.StatusCode != HttpStatusCode.OK) {
				String errdesc = await HttpResponse.Content.ReadAsStringAsync();
				return GoogleVisionApiResult.FromJson(errdesc, httpResponse.StatusCode);
			}

			String json = await HttpResponse.Content.ReadAsStringAsync();
			return GoogleVisionApiResult.FromJson(json);
		}

    }
}
