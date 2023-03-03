using System;
using System.Net;
using System.Threading.Tasks;
using ININ.ICWS;
using ININ.ICWS.Connection;
using ININ.WebServices.Core;

namespace ININ.PSO.DataAccess
{
    class IcwsConnectionProvider
    {
        private readonly WebServiceUtility _webServiceUtility;

        public IcwsConnectionProvider()
        {
            _webServiceUtility = new WebServiceUtility("Application Name")
            {
                Port = 8019,
                Server = "ServerName",
                IsHttps = true
            };    
        }

        private void ConnectSession()
        {
            // TODO: Implement Switchover: https://help.inin.com/developer/cic/docs/icws/webhelp/ConceptualContent/GettingStarted_Connecting.htm#alternateHosts

            var connectionResource = new ConnectionResource(_webServiceUtility);

            var requestParameters = new ConnectionResource.CreateConnectionRequestParameters
            {
                Accept_Language = "en_US",
                Include = "features"
            };

            var dataContract = new IcAuthConnectionRequestSettingsDataContract
            {
                ApplicationName = "Example",
                UserID = "username",
                Password = "password"
            };

            var createConnectionTask = connectionResource.CreateConnection(requestParameters, dataContract);
            createConnectionTask.ContinueWith(t =>
            {
                var createConnectionResponses = t.Result;
                createConnectionResponses.PerformIfResponseIs201(
                    response => HandleConnection201(response, createConnectionResponses.Set_Cookie));
                createConnectionResponses.PerformOnFailureResponse(HandleError);
                createConnectionResponses.PerformDefault(HandleDefault);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            createConnectionTask.ContinueWith(t => HandleError(t.Exception),
                TaskContinuationOptions.OnlyOnFaulted);
        }
        
        private void HandleConnection201(ConnectionResponseDataContract response, string cookie)
        {
            _webServiceUtility.SessionParameters = new AuthenticationParameters()
            {
                SessionId = response.SessionId,
                Cookie = cookie,
                ININ_ICWS_CSRF_Token = response.CsrfToken
            };
        }
        private void HandleDefault(HttpStatusCode obj)
        {
            throw new NotImplementedException();
        }

        private void HandleError(HttpStatusCode obj)
        {
            throw new NotImplementedException();
        }

        private void HandleError(Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}
