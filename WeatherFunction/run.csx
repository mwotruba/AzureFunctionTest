using System;
using System.Net;

private static readonly string AppKey = "bbf5fb8c8b5733365893886826af5724";

public static void Run(TimerInfo myTimer, string cities, out string outputEventHubMessage, TraceWriter log)
{
    log.Info($"C# time triggered function called with pattern: {myTimer.Schedule} at {DateTime.Now}");
    log.Info(myTimer.FormatNextOccurrences(3));

//    if(cities != null)
//    {
//        log.Info($"cities.Length={cities.Length}");
//        log.Info($"cities #cities={cities.Split(',').Length}");
//    }
    
    // building URL
    UriBuilder ub = new UriBuilder();
    ub.Host = "api.openweathermap.org";
    //ub.Path = "data/2.5/group";
    ub.Path = "data/2.5/box/city";
    ub.Scheme = "http";
    //ub.Query = string.Format("appid={0}&id={1}", AppKey, cities);
    ub.Query = string.Format("appid={0}&bbox=-180,-90,180,90", AppKey);
    
    WebResponse wr = null;
    Stream ws = null;
    StreamReader reader = null;
    try
    {
        // do the GET
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ub.Uri);
        request.Method = "GET";
        wr = request.GetResponse();

        // read response
        ws =  wr.GetResponseStream();
        reader = new StreamReader(ws);
        string json = reader.ReadToEnd();
        log.Info(json);
        
        // send message
        outputEventHubMessage = json;
    } catch(Exception exc)
    {
      log.Error("Fehler in Funktion: {exc.Message}
    }
    finally
    {            
        // close objects
        if(wr != null) wr.Close();
        if(reader != null) reader.Close();
        if(ws !=null) ws.Dispose();    
    }
}    