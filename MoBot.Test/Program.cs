using Newtonsoft.Json;
using WebSocketSharp;
using static System.Net.WebRequestMethods;

var ws = new WebSocket("ws://192.168.5.2:8489");
ws.OnMessage += (s, e) => { Console.WriteLine(e.Data); };
ws.Connect();
ws.Send(JsonConvert.SerializeObject(new
{
	action = "send_group_msg",
	@params = new
	{
		group_id = 1079464803,
		message = new[] { new { type = "text", data = new { text = "13" } } }
	},
	echo = "123465"
}));
while (true) { }