#r "Newtonsoft.Json"

using System;
using System.Net;
using System.Globalization;
using Newtonsoft.Json;

private static readonly string AppKey = "bbf5fb8c8b5733365893886826af5724";

public static void Run(TimerInfo myTimer, out string outputEventHubMessage, TraceWriter log)
{
    DateTime start = DateTime.Now;

    log.Info($"C# time triggered function called with pattern: {myTimer.Schedule} at {DateTime.Now}");
    log.Info(myTimer.FormatNextOccurrences(3));

    // building URL
    UriBuilder ub = new UriBuilder();
    ub.Host = "api.openweathermap.org";
    ub.Path = "data/2.5/box/city";
    ub.Scheme = "http";

    // Einstellung der Rastergröße; der Bereich ist
    //     lon (-180..180) west -> east
    //     lat (90..-90)  nord -> south
    int anzStepsLon = 30;
    int anzStepsLat = 16;

    double deltaLon = 360.0 / anzStepsLon;
    double deltaLat = 180.0 / anzStepsLat;
    
    int i = 0;
    for (int x = -anzStepsLon / 2; x < anzStepsLon / 2; x++)
    {
        for (int y = anzStepsLat / 2; y > -anzStepsLat / 2; y--)
        {
            double lonLeft = deltaLon * x;
            double latTop = deltaLat * y;
            double lonRight = deltaLon * x + deltaLon;
            double latBotton = deltaLat * y - deltaLat;
            i++;

            // die box für den Querystring
            string bbox = String.Format(new CultureInfo("en-US"), "{0},{1},{2},{3}", lonLeft, latBotton, lonRight, latTop);

            ub.Query = string.Format("appid={0}&bbox={1}", AppKey, bbox);

            WebResponse wr = null;
            Stream ws = null;
            StreamReader reader = null;
            try
            {
                log.Info($"HTTP GET => {ub.Uri}");
                
                // do the GET
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ub.Uri);
                request.Method = "GET";
                wr = request.GetResponse();

                // read response
                ws = wr.GetResponseStream();
                reader = new StreamReader(ws);
                string rawJson = reader.ReadToEnd();

                var obj = JsonConvert.DeserializeObject(rawJson);
                string json = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

                log.Info($"json.length={json.Length}");
                log.Info(json);
                
                // send message
                outputEventHubMessage = json;
                
                int delay = 1000;
                log.Info($"{i}/{anzStepsLon * anzStepsLat} OK -> warte {delay/1000.0} sec");
                Thread.Sleep(delay);
            }
            catch (Exception exc)
            {
                log.Error($"Fehler in Funktion: {exc.Message}");
            }
            finally
            {
                // close objects
                if (wr != null) wr.Close();
                if (reader != null) reader.Close();
                if (ws != null) ws.Dispose();

                outputEventHubMessage = null;
            }
        }
    }
    log.Info($"One roundtrip finished in {(DateTime.Now - start).TotalMinutes} minutes");
}    
