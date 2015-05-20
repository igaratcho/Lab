using UnityEngine;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;

public class HttpServer
{
	HttpListener listener;
	
	public HttpServer()
	{
	}
	
	public void Start()
	{
		Debug.Log("Start HttpServer");
		if (listener == null)
		{
			listener = new HttpListener();
			listener.Prefixes.Add(string.Format("http://{0}:{1}/", "localhost", 8080));
		}
		listener.Start();
		
		listener.BeginGetContext(OnGetContext, listener);
	}
	
	public void Stop()
	{
		listener.Stop();
		Debug.Log("Stop HttpServer");
	}
	
	private void OnGetContext(IAsyncResult ar)
	{
		try
		{
			HttpListenerContext context = ((HttpListener)ar.AsyncState).EndGetContext(ar);
			OnRequest(context);
			listener.BeginGetContext(OnGetContext, listener);
		}
		catch (Exception e)
		{
			Debug.Log(e);
		}
	}
	
	private void OnRequest(HttpListenerContext context)
	{
		Debug.Log(context.Request.RawUrl); // 加工されてないアドレス
		Debug.Log(context.Request.UserHostAddress); // どこから接続されたか
		
		HttpWebRequest webRequest = WebRequest.Create("http://localhost") as HttpWebRequest; // そのまま取得
		// ftp:// とかのリクエストにも対応できるはず

		Debug.Log("0");

		// リクエストラインとヘッダの設定。ある程度しかしていない。
		webRequest.Method = context.Request.HttpMethod;
		webRequest.ProtocolVersion = context.Request.ProtocolVersion;
		webRequest.KeepAlive = context.Request.KeepAlive;
		if (context.Request.UrlReferrer != null)
			webRequest.Referer = context.Request.UrlReferrer.OriginalString;
		webRequest.UserAgent = context.Request.UserAgent;
		webRequest.CookieContainer = new CookieContainer();
		webRequest.CookieContainer.Add(webRequest.RequestUri, context.Request.Cookies);

		Debug.Log("1");

		byte[] buffer = new byte[8192];
		// ボディあったら送受信
		if (context.Request.HasEntityBody)
		{
			Debug.Log("Request.HasEntityBody" +  context.Request.RawUrl);
			webRequest.ContentType = context.Request.ContentType;
			if (context.Request.ContentLength64 >= 0)
				webRequest.ContentLength = context.Request.ContentLength64;
			// transfer-encoding: chunked のことは考えていない
			Stream input = context.Request.InputStream;
			Stream output = webRequest.GetRequestStream();
			
			while (true)
			{
				int read = input.Read(buffer, 0, buffer.Length);
				if (read == 0)
					break;
				output.Write(buffer, 0, read);
			}
			
			output.Flush();
			input.Close();
			output.Close();
		}
		Debug.Log("2");
		
		// レスポンス取得
		HttpWebResponse webResponse = null;
		try
		{
			webResponse = webRequest.GetResponse() as HttpWebResponse;
		}
		catch (Exception e)
		{
			Debug.Log(e);
		}
		
		// だめだった時の処理。てきとう
		if (webResponse == null)
		{
			context.Response.ProtocolVersion = HttpVersion.Version11;
			context.Response.StatusCode = 503;
			context.Response.StatusDescription = "Response Error";
			context.Response.ContentLength64 = 0;
			context.Response.Close();
			return;
		}
		
		// ブラウザへ返すレスポンスの設定。あるていど。
		context.Response.ProtocolVersion = webResponse.ProtocolVersion;
		context.Response.StatusCode = (int)webResponse.StatusCode;
		context.Response.StatusDescription = webResponse.StatusDescription;
		
		context.Response.AddHeader("Server", webResponse.Server);
		context.Response.ContentType = webResponse.ContentType;
		if (webResponse.ContentLength >= 0)
		{
			Debug.Log("Content-Length=" + webResponse.ContentLength +  context.Request.RawUrl);
			context.Response.ContentLength64 = webResponse.ContentLength;
		}
		if (webResponse.GetResponseHeader("Transfer-Encoding").ToLower().Equals("chunked"))
		{
			Debug.Log("chunked" +  context.Request.RawUrl);
			context.Response.SendChunked = true;
		}
		context.Response.KeepAlive = context.Request.KeepAlive;
		
		// ボディの送受信
		Stream instream = webResponse.GetResponseStream();
		Stream outstream = context.Response.OutputStream;
		
		try
		{
			// これでうまくいくんだ・・・。ブロッキングせずにちゃんと 0 が返ってくるな。
			// 切断まで 0 は返ってこないからブロッキングするとおもってたけど・・・。
			while (true)
			{
				int read = instream.Read(buffer, 0, buffer.Length);
				Debug.Log(read + " bytes received " + context.Request.RawUrl);

				if (read == 0)
					break;

				outstream.Write(buffer, 0, read);

				Debug.Log(ASCIIEncoding.ASCII.GetString(buffer));
			}
			
		}
		catch (Exception e)
		{
			Debug.Log(e);
		}
		
		try
		{
			webResponse.Close();
			context.Response.Close();
		}
		catch (InvalidOperationException e)
		{
			Debug.Log(e);
		}
		catch (HttpListenerException he)
		{
			Debug.Log(he);
		}
	}
}

/*
public class Proxy 
{
	class ServerListerner
	{
		private int listenPort;
		private TcpListener listener;
		
		public ServerListerner(int port)
		{
			this.listenPort = port;
			this.listener = new TcpListener(IPAddress.Any, this.listenPort);
		}
		
		public void StartServer()
		{
			this.listener.Start();
		}
		
		public void AcceptConnection()
		{
			Socket newClient = this.listener.AcceptSocket();
			ClientConnection client = new ClientConnection(newClient);
			client.StartHandling();
		}
	}
	
	class ClientConnection
	{
		private Socket clientSocket;
		
		public ClientConnection(Socket client)
		{
			this.clientSocket = client;
		}
		
		public void StartHandling()
		{
			Thread handler = new Thread(Handler);
			//			handler.Priority = ThreadPriority.AboveNormal;
			handler.Start();
		}
		
		private void Handler()
		{
			bool recvRequest = true;
			string EOL = "\r\n";
			
			string requestPayload       = "";
			string requestTempLine      = "";
			List<string> requestLines   = new List<string>();
			byte[] requestBuffer        = new byte[1];
			byte[] responseBuffer       = new byte[1];
			
			requestLines.Clear();
			
			try
			{
				//State 0: Handle Request from Client
				while (recvRequest)
				{
					this.clientSocket.Receive(requestBuffer);
					string fromByte = ASCIIEncoding.ASCII.GetString(requestBuffer);
					requestPayload += fromByte;
					requestTempLine += fromByte;
					
					if (requestTempLine.EndsWith(EOL))
					{
						requestLines.Add(requestTempLine.Trim());
						requestTempLine = "";
					}
					
					if (requestPayload.EndsWith(EOL + EOL))
					{
						recvRequest = false;
					}
				}
				Debug.Log("Raw Request Received...");
				Debug.Log(requestPayload);
				
				//State 1: Rebuilding Request Information and Create Connection to Destination Server
				//				string remoteHost = requestLines[0].Split(' ')[1].Replace("http://", "").Split('/')[0];
				//				string requestFile = requestLines[0].Replace("http://", "").Replace(remoteHost, "");
				//				requestLines[0] = requestFile;
				
				requestPayload = "";
				foreach (string line in requestLines)
				{
					requestPayload += line;
					requestPayload += EOL;
				}
				
				Socket destServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				destServerSocket.Connect("127.0.0.1", 80);
				
				//State 2: Sending New Request Information to Destination Server and Relay Response to Client
				destServerSocket.Send(ASCIIEncoding.ASCII.GetBytes(requestPayload));
				
				//Console.WriteLine("Begin Receiving Response...");
				while (destServerSocket.Receive(responseBuffer) != 0)
				{
					Debug.Log(ASCIIEncoding.ASCII.GetString(responseBuffer));
					
					//Console.Write(ASCIIEncoding.ASCII.GetString(responseBuffer));
					this.clientSocket.Send(responseBuffer);
				}
				
				destServerSocket.Disconnect(false);
				//				destServerSocket.Dispose();
				this.clientSocket.Disconnect(false);
				//				this.clientSocket.Dispose();
			}
			catch (Exception e)
			{
				Debug.Log("Error Occured: " + e.Message);
				Debug.Log(e.StackTrace);
				//Console.WriteLine(e.StackTrace);
			}
		}	
	}

	public void Start() 
	{
		ServerListerner simpleHttpProxyServer = new ServerListerner(9000);
		simpleHttpProxyServer.StartServer();
		simpleHttpProxyServer.AcceptConnection();
	}

	public static void Log(string str)
	{
		Debug.Log (str);
	}



/*
	private Thread _proxyThread;
	private TcpListener _listener;
	
	public void Start() 
	{
		this._proxyThread = new Thread(StartListener);
		this._proxyThread.Start();
	}
	
	protected void StartListener(object data)
	{
		this._listener = new TcpListener(IPAddress.Any, 1234);
		this._listener.Start();
		
		while (true)
		{
			TcpClient client = this._listener.AcceptTcpClient();
			
			while (client.Connected)
			{
				NetworkStream stream = client.GetStream();
				
				StringBuilder request = new StringBuilder();
				
				byte[] bytes = new byte[1024];
				
				while (stream.DataAvailable && stream.CanRead)
				{   
					int i = stream.Read(bytes, 0, bytes.Length);
					request.Append(System.Text.Encoding.ASCII.GetString(bytes, 0, i));
				}
				
				if (stream.CanWrite)
				{
					byte[] response = System.Text.Encoding.Default.GetBytes("HTTP/1.1 200 OK" + Environment.NewLine + "Content-length: 4\r\n\r\n" + "ABCD");
					stream.Write(response, 0, response.Length);
				}
				
				client.Close();
			}
		}
	}
*/
//}

