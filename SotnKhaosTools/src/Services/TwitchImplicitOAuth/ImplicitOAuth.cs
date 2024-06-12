using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SotnKhaosTools.Services.TwitchImplicitOAuth
{
	public class ImplicitOAuth
	{
		string twitchAuthUrl = "https://id.twitch.tv/oauth2/authorize";
		int salt = 0;

		public delegate void UpdatedValuesEvent(string state, string token);
		public event UpdatedValuesEvent OnRevcievedValues;

		public ImplicitOAuth(int stateSalt = 42)
		{
			salt = stateSalt;
		}

		public string RequestClientAuthorization()
		{
			string authStateVerify = ((long) DateTime.UtcNow.AddYears(salt).Subtract(new DateTime(1939, 11, 30)).TotalSeconds).ToString();

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("response_type=token&client_id=");
			stringBuilder.Append(ApplicationDetails.twitchClientId);
			stringBuilder.Append("&redirect_uri=");
			stringBuilder.Append(ApplicationDetails.redirectUri);
			stringBuilder.Append("&state=");
			stringBuilder.Append(authStateVerify);
			stringBuilder.Append("&scope=");
			stringBuilder.Append(string.Join("+", Scopes.GetScopes()));

			string queryParams = stringBuilder.ToString();

			InitializeLocalWebServers();
			System.Diagnostics.Process.Start($"{twitchAuthUrl}?{queryParams}");

			return authStateVerify;
		}

		void InitializeLocalWebServers()
		{
			if (ApplicationDetails.redirectUri == null || ApplicationDetails.redirectUri.Length == 0)
			{
				Console.WriteLine("URI may not be empty!");
				return;
			}

			HttpListener redirectListener = new HttpListener();
			redirectListener.Prefixes.Add(ApplicationDetails.redirectUri);
			redirectListener.Start();
			redirectListener.BeginGetContext(new AsyncCallback(IncommingTwitchRequest), redirectListener);

			HttpListener fetchListeneer = new HttpListener();
			fetchListeneer.Prefixes.Add(ApplicationDetails.fetchUri);
			fetchListeneer.Start();
			fetchListeneer.BeginGetContext(new AsyncCallback(IncommingLocalRequest), fetchListeneer);
		}

		void IncommingLocalRequest(IAsyncResult result)
		{
			HttpListener httpListener = (HttpListener) result.AsyncState;
			HttpListenerContext httpContext = httpListener.EndGetContext(result);
			HttpListenerRequest httpRequest = httpContext.Request;

			string? jsonObjectString = null;
			var reader = new StreamReader(httpRequest.InputStream, httpRequest.ContentEncoding);
			jsonObjectString = reader.ReadToEnd();

			jsonObjectString = jsonObjectString.Replace("\\", null);
			jsonObjectString = jsonObjectString.Remove(jsonObjectString.Length - 1);
			jsonObjectString = jsonObjectString.Remove(0, 1);
			JObject jo = JObject.Parse(jsonObjectString);

			OnRevcievedValues?.Invoke(jo.GetValue("state").ToString(), jo.GetValue("access_token").ToString());

			httpListener.Stop();
		}

		void IncommingTwitchRequest(IAsyncResult result)
		{
			HttpListener httpListener = (HttpListener) result.AsyncState;
			HttpListenerContext httpContext = httpListener.EndGetContext(result);
			HttpListenerRequest httpRequest = httpContext.Request;
			HttpListenerResponse httpResponse = httpContext.Response;

			string responseString = "";

			responseString =
			@"
				<html>
					<head>
					</head>
					<body>
						<h1>
							Authentication complete!
						</h1>
						<p>You are adviced not to show this page on stream.<p>
						<p>You may close this window now.<p>

						<script>
							let values = 
							{
								access_token:""TOKEN"",
								state: ""STATE""
							};

							const url = new URLSearchParams(""?"" + window.location.hash.substring(1))
							window.history.replaceState(null, '', '/');
							values.access_token = url.get('access_token');
							values.state = url.get('state');

							jsonData = JSON.stringify(values);

							fetch('VARIABLE_FETCHURI', { method: ""POST"", body: JSON.stringify(jsonData)})
						</script>
					</body>
				</html>
			";
			responseString = responseString.Replace("VARIABLE_FETCHURI", ApplicationDetails.fetchUri);

			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
			httpResponse.ContentLength64 = buffer.Length;
			Stream output = httpResponse.OutputStream;
			output.Write(buffer, 0, buffer.Length);
			output.Close();

			httpListener.Stop();
		}
	}
}