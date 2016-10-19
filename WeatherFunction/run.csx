using System;
using System.Net;

private static readonly string Ratingen = "707860, 519188, 1283378, 1270260, 708546, 1283710, 529334, 1269750, 1283240, 703363, 3632308, 473537, 384848, 569143, 713514, 2878044, 464176, 295582" ;

private static readonly string AppKey = "bbf5fb8c8b5733365893886826af5724";

public static void Run(TimerInfo myTimer, out string outputEventHubMessage, TraceWriter log)
{
    log.Info($"C# time triggered function called with pattern: {myTimer.Schedule} at {DateTime.Now}");
    log.Info(myTimer.FormatNextOccurrences(3));
    
    // building URL
    UriBuilder ub = new UriBuilder();
    ub.Host = "api.openweathermap.org";
    ub.Path = "data/2.5/group";
    ub.Scheme = "http";
    ub.Query = string.Format("appid={0}&id={1}", AppKey, Ratingen);

    // do the GET
    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ub.Uri);
    request.Method = "GET";
    WebResponse wr = request.GetResponse();

    // read response
    Stream ws =  wr.GetResponseStream();
    StreamReader reader = new StreamReader(ws);
    string json = reader.ReadToEnd();
    log.Info(json);
    
    // send message
    outputEventHubMessage = json;
            
            
    // close objects
    wr.Close();
    reader.Close();
    ws.Dispose();    
}