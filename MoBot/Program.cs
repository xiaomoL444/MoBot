using WebSocketSharp;

var ws = new WebSocket("ws://192.168.5.2:8489");
ws.OnMessage += (s, e) => { Console.WriteLine(e.Data); };
ws.Connect();
while (true) { }