using System;
using System.Net;
using System.Web.Services.Discovery;

namespace AIMS.Libraries.Scripting.ScriptControl.ReferenceDialog
{
	/// <summary>
	/// Custom DiscoveryClientProtocol that determines whether user authentication
	/// is required.
	/// </summary>
	public class WebServiceDiscoveryClientProtocol : DiscoveryClientProtocol
	{
		HttpWebResponse lastResponseReceived;
		
		public WebServiceDiscoveryClientProtocol()
		{
		}
		
		public HttpAuthenticationHeader GetAuthenticationHeader()
		{
			if (lastResponseReceived != null) {
				return new HttpAuthenticationHeader(lastResponseReceived.Headers);
			}
			return null;
		}
		
		public bool IsAuthenticationRequired {
			get {
				if (lastResponseReceived != null) {
					return lastResponseReceived.StatusCode == HttpStatusCode.Unauthorized;
				}
				return false;
			}
		}
		
		protected override WebResponse GetWebResponse(WebRequest request)
		{
			WebResponse response = base.GetWebResponse(request);
			lastResponseReceived = response as HttpWebResponse;
			return response;
		}
	}
}
