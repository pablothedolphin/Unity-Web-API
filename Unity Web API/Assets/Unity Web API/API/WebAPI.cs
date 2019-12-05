using System;
using System.Collections;
using UnityEngine.Events;

namespace UnityEngine.Networking
{
	/// <summary>
	/// 
	/// </summary>
	[CreateAssetMenu (menuName = "WebAPI")]
	public class WebAPI : ScriptableObject
	{
		/// <summary>
		/// Prefixes all endpoints passed to the <c>HttpRequest</c> method.
		/// </summary>
		[SerializeField] protected string domain;

		/// <summary>
		/// 
		/// </summary>
		[Space]
		[SerializeField] protected bool useSecurityToken;

		/// <summary>
		/// 
		/// </summary>
		[SerializeField] protected string securityTokenKey;

		/// <summary>
		/// 
		/// </summary>
		[SerializeField] protected string securityTokenValue;

		/// <summary>
		/// Create a HTTP web request based on the provided arguments and invokes a callback method which you provide
		/// to handle the reponse once the download is complete.
		/// </summary>
		/// <param name="endPoint">The specific endpoint to send the request to. Appends the domain value to the begining of this string to form the full URI.</param>
		/// <param name="callback">A public method to invoke which takes the web request once the download has
		/// completed.</param>
		/// <param name="method">The type of web request to make. GET by defualt.</param>
		/// <param name="input">Whatever data to pass in as an input to converted to JSON for the endpoint. Null by default.</param>
		/// <returns>Coroutine handle which yields the web request.</returns>
		public virtual IEnumerator HttpRequest (string endPoint, UnityAction<UnityWebRequest> callback, RequestMethod method = RequestMethod.GET, object input = null)
		{
			DownloadHandler downloader = new DownloadHandlerBuffer ();
			UnityWebRequest request;
			string uri = domain + endPoint;

			switch (method)
			{
				case RequestMethod.GET:
					request = new UnityWebRequest (uri);
					request.downloadHandler = downloader;
					break;
				case RequestMethod.POST:
					byte[] rawBody = new System.Text.UTF8Encoding ().GetBytes (JsonUtility.ToJson (input));
					UploadHandler uploader = new UploadHandlerRaw (rawBody);
					request = new UnityWebRequest (uri, Enum.GetName (typeof (RequestMethod), method), downloader, uploader);
					break;
				default:
					throw new Exception (string.Format ("RequestMethod.{0} not supported. You can choose to extend this class and add your own custom support.", Enum.GetName (typeof (RequestMethod), method)));
			}

			if (useSecurityToken) 
				request.SetRequestHeader (securityTokenKey, securityTokenValue);

			request.SetRequestHeader ("Content-Type", "application/json");

			yield return request.SendWebRequest ();

			if (!IsResponsePositive (request.responseCode))
				throw new Exception (string.Format ("Web request returned negative response: {0} {1} '{2}'", request.responseCode, GetNameOfResponseCode (request.responseCode), request.downloadHandler.text));

			callback.Invoke (request);

		}

		/// <summary>
		/// Converts the numeric code into the title used to describe it for ease of debugging.
		/// </summary>
		/// <param name="code">The code to convert</param>
		/// <returns>The title of the code</returns>
		protected virtual string GetNameOfResponseCode (long code)
		{
			switch (code)
			{
				case 200:
					return "Ok";
				case 201:
					return "Created";
				case 202:
					return "Accepted";
				case 204:
					return "No Content";
				case 400:
					return "Bad Request";
				case 401:
					return "Unauthorized";
				case 404:
					return "Not Found";
				case 500:
					return "Internal Server Error";
				case 501:
					return "Not Implemented";
				default:
					return "Unexpected. Look up the number: " + code.ToString ();
			}
		}

		/// <summary>
		/// Lets you know if the request worked in a clearer way
		/// </summary>
		/// <param name="code">The code to inrepret</param>
		/// <returns>True if the code was 200-202. False for everything else.</returns>
		protected virtual bool IsResponsePositive (long code)
		{
			switch (code)
			{
				case 200:
				case 201:
				case 202:
					return true;
				default:
					return false;
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public enum RequestMethod
	{
		GET,
		POST,
		PUT,
		PATCH,
		DELETE,
		COPY,
		HEAD,
		OPTIONS,
		LINK,
		UNLINK,
		PURGE,
		LOCK,
		UNLOCK,
		PROPFIND,
		VIEW
	}
}