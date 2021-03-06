using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System;
using System.Configuration;
using Microsoft.WindowsAzure.MediaServices.Client;
using System.Net.Http;
using System.IO;

namespace DetectionApp
{
    public static class RedactorProcessing
    {
        [FunctionName("RedactorProcessing")]
        public static async Task Run([EventGridTrigger]JObject eventGridEvent, TraceWriter log, ExecutionContext executionContext)
        {
            Guid newReqId = Guid.NewGuid();
            bool result = true;
            string reqId = string.Empty;
            try
            {
                log.Info($"Start RedactorProcessing requestId: {newReqId} {eventGridEvent.ToString(Formatting.Indented)} ticks: {DateTime.Now.Ticks}");
                var eventGrid = eventGridEvent.ToObject<Event<VideoAssetData>>();
                reqId = eventGrid.Data.RequestId;
                IAsset asset = FaceHelper.GetAsset(eventGrid.Data.AssetName);
                if (asset != null)
                {
                    string assetName = eventGrid.Data.JobName + "_" + eventGrid.Data.Step;
                    string configFile = Directory.GetParent(executionContext.FunctionDirectory).FullName + "\\config.json";
                    await FaceHelper.RunFaceRedactionJob(asset, assetName, configFile, log);
                    log.Info($"request {newReqId} for {reqId} started redactor process send event grid to start copy process ticks: {DateTime.Now.Ticks}");                                        
                }
                else
                {
                    log.Info($"request {newReqId} for {reqId} didn't start redactor no related asset name {eventGrid.Data.AssetName} ticks: {DateTime.Now.Ticks}");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Exception Message: {ex.Message}, requestId: {newReqId} for {reqId}, ticks: {DateTime.Now.Ticks}", ex);
                result = false;
            }
            string succeed = result ? "" : "unsuccessful";
            log.Info($"Finished {succeed} RedactorProcessing requestId: {newReqId} for {reqId} ticks: {DateTime.Now.Ticks}");
        }
    }
}
