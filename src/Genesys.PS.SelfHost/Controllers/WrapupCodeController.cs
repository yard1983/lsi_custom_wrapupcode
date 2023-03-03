using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Web.Http;
using Genesys.PS.DataAccess;

namespace Genesys.PS.SelfHost.Controllers
{
    public class WrapupCodeController : ApiController
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();   
        public class UpdateResponse
        {
            public bool success { get; set; }
            public string status { get; set; }
            public string message { get; set; }
        }

        public class UpdateRequest
        {
            public string CallIDKey { get; set; }
            public string ANI { get; set; }
            public string DNIS { get; set; }
            public string IntConnStartTime { get; set; }
            public string IntConnEndTime { get; set; }
            public string QueueStartTime { get; set; }
            public string QueueEndTime { get; set; }
            public string AgentID { get; set; }
            public string CallCode { get; set; }
            public string ClientId { get; set; }
            public string ClientName { get; set; }
            public string Skill { get; set; }
            public string QueueName { get; set; }
            public string WrapupStartTime { get; set; }
            public string WrapupEndTime { get; set; }

            public int HoldTime { get; set; }
            public string TimeStamp1 { get; set; }
            public string Location { get; set; }

        }
        
        [HttpPost]
        public UpdateResponse Update(UpdateRequest request)
        {
            _log.Info("update CallIDKey: " + request.CallIDKey);
            string formatDateFrontEnd = "yyyy-MM-ddTHH:mm:ss.fffZ";            
            string formatDateDatabase = "yyyy-MM-dd HH:mm:ss.fff";
            DateTime intConnStartTime, intConnEndTime, queueStartTime, queueEndTime, wrapupStartTime, wrapupEndTime, timeStamp1;

            Guid callIDKey;
            bool isValidCallIDKey = Guid.TryParse(request.CallIDKey, out callIDKey);

            if (!isValidCallIDKey && string.IsNullOrWhiteSpace(request.CallIDKey) || string.IsNullOrWhiteSpace(request.AgentID) || string.IsNullOrWhiteSpace(request.Skill) || string.IsNullOrWhiteSpace(request.TimeStamp1)) 
            {
                _log.Error("update Error: Missing a required field (CallIDKey | AgentID | Skill | TimeStamp1)");
                throw new ArgumentException("Missing a required field (CallIDKey | AgentID | Skill | TimeStamp1)");
            }

            DateTime.TryParseExact(request.IntConnStartTime, formatDateFrontEnd, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out intConnStartTime);
            DateTime.TryParseExact(request.IntConnEndTime, formatDateFrontEnd, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out intConnEndTime);
            DateTime.TryParseExact(request.QueueStartTime, formatDateFrontEnd, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out queueStartTime);
            DateTime.TryParseExact(request.QueueEndTime, formatDateFrontEnd, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out queueEndTime);
            DateTime.TryParseExact(request.WrapupStartTime, formatDateFrontEnd, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out wrapupStartTime);
            DateTime.TryParseExact(request.WrapupEndTime, formatDateFrontEnd, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out wrapupEndTime);
            DateTime.TryParseExact(request.TimeStamp1, formatDateFrontEnd, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out timeStamp1);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@CallIDKey", request.CallIDKey);
            parameters.Add("@ANI", request.ANI);
            parameters.Add("@DNIS", request.DNIS);
            parameters.Add("@IntConnStartTime", intConnStartTime != default(DateTime)? (object)intConnStartTime:null);
            parameters.Add("@IntConnEndTime", intConnEndTime != default(DateTime) ? (object)intConnEndTime : null);
            parameters.Add("@QueueStartTime", queueStartTime != default(DateTime) ? (object)queueStartTime : null);
            parameters.Add("@QueueEndTime", queueEndTime != default(DateTime) ? (object)queueEndTime : null);
            parameters.Add("@AgentID", request.AgentID);
            parameters.Add("@CallCode", request.CallCode);
            parameters.Add("@ClientId", request.ClientId);
            parameters.Add("@ClientName", request.ClientName);
            parameters.Add("@Skill", request.Skill);
            parameters.Add("@WrapupStartTime", wrapupStartTime != default(DateTime) ? (object)wrapupStartTime : null);
            parameters.Add("@WrapupEndTime", wrapupEndTime != default(DateTime) ? (object)wrapupEndTime : null);
            parameters.Add("@HoldTime", request.HoldTime);
            parameters.Add("@TimeStamp1", timeStamp1 != default(DateTime) ? (object)timeStamp1 : null);
            parameters.Add("@Location", request.Location);
                        
            _log.Info(string.Format("Data to save -> @CallIDKey: {0}, @ANI: {1}, @DNIS: {2}, @IntConnStartTime: {3}, @IntConnEndTime: {4}, @QueueStartTime: {5}, @QueueEndTime: {6}, @AgentID: {7}, @CallCode: {8}, @ClientId: {9}, @ClientName: {10}, @Skill: {11}, @WrapupStartTime: {12}, @WrapupEndTime: {13}, @HoldTime: {14}, @TimeStamp1: {15}, @Location: {16};"
                                    , "'" + request.CallIDKey + "'"
                                    , request.ANI != null ? "'" + request.ANI + "'" : "null"
                                    , request.DNIS != null ? "'" + request.DNIS + "'" : "null"
                                    , intConnStartTime != default(DateTime) ? "'" + intConnStartTime.ToString(formatDateDatabase) + "'" : "null"
                                    , intConnEndTime != default(DateTime) ? "'" + intConnEndTime.ToString(formatDateDatabase) + "'" : "null"
                                    , queueStartTime != default(DateTime) ? "'" + queueStartTime.ToString(formatDateDatabase) + "'" : "null"
                                    , queueEndTime != default(DateTime) ? "'" + queueEndTime.ToString(formatDateDatabase) + "'" : "null"
                                    , request.AgentID != null ? "'" + request.AgentID + "'" : "null"
                                    , request.CallCode != null ? "'" + request.CallCode + "'" : "null"
                                    , request.ClientId != null ? "'" + request.ClientId + "'" : "null"
                                    , request.ClientName != null ? "'" + request.ClientName + "'" : "null"
                                    , request.Skill != null ? "'" + request.Skill + "'" : "null"
                                    , wrapupStartTime != default(DateTime) ? "'" + wrapupStartTime.ToString(formatDateDatabase) + "'" : "null"
                                    , wrapupEndTime != default(DateTime) ? "'" + wrapupEndTime.ToString(formatDateDatabase) + "'" : "null"
                                    , request.HoldTime.ToString()
                                    , timeStamp1 != default(DateTime) ? "'" + timeStamp1.ToString(formatDateDatabase) + "'" : "null"
                                    , request.Location != null ? "'" + request.Location + "'" : "null")
            );

            var response = new UpdateResponse();

            try
            {
                var result = SqlServerProvider.RunCommand("LSI_WrapUpCodeHist_spUpdate", System.Data.CommandType.StoredProcedure, parameters);
                _log.Info("The information was updated successfully -> CallIDKey:"  + request.CallIDKey??"" + ", Code: " + request.CallCode);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Update wrapup-code error");
                response.success = false;
                response.status = ex.Message;
                return response;
            }
            
            response.success = true;
            response.status = "OK: " + request.CallIDKey;
            return response;
        }

    }
}
