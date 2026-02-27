using MagicLinkRestGate.InterFaces;
using Microsoft.Extensions.Options;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace MagicLinkRestGate.Classes
{
    public class MagoWSGateHelper : IMagoWSGateHelper
    {
        private readonly MlkSettings _mlkSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MagoWSGateHelper> _logger;

        public MagoWSGateHelper(IOptions<MlkSettings> mlsettings, IHttpClientFactory httpClientFactory, ILogger<MagoWSGateHelper> logger)
        {
            _mlkSettings = mlsettings.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        private string LoginManagerUri =>
            $"http://{_mlkSettings.LoginServerMago}/{_mlkSettings.LoginInstallationName}/loginmanager/loginmanager.asmx?wsdl";

        private string TbServicesUri =>
            $"http://{_mlkSettings.LoginServerMago}/{_mlkSettings.LoginInstallationName}/TbServices/tbdms.asmx?wsdl";

        private static string Esc(string value) => SecurityElement.Escape(value) ?? string.Empty;

        private static string? GetSoapValue(XDocument soap, string responseName, string elementName)
        {
            return soap.Descendants()
                .SingleOrDefault(d => d.Name.LocalName == responseName)
                ?.Descendants()
                .SingleOrDefault(n => n.Name.LocalName == elementName)
                ?.Value;
        }

        private static XElement? GetSoapResponseNode(XDocument soap, string responseName)
        {
            return soap.Descendants()
                .SingleOrDefault(d => d.Name.LocalName == responseName);
        }

        public async Task<BeResponseGt<string>> LoginSoap(string user, string passw, string companyname, string process)
        {
            try
            {
                string loginbody =
                $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                  <soap12:Body>
                    <LoginCompact xmlns=""http://microarea.it/LoginManager/"">
                      <userName>{Esc(user)}</userName>
                      <companyName>{Esc(companyname)}</companyName>
                      <password>{Esc(passw)}</password>
                      <askingProcess>{Esc(process)}</askingProcess>
                      <overWriteLogin>true</overWriteLogin>
                    </LoginCompact>
                  </soap12:Body>
                </soap12:Envelope>";

                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(loginbody, Encoding.UTF8, "application/soap+xml");
                using var response = await client.PostAsync(LoginManagerUri, content);
                var soapResponse = await response.Content.ReadAsStringAsync();
                var soap = XDocument.Parse(soapResponse);

                var token = GetSoapValue(soap, "LoginCompactResponse", "authenticationToken");
                if (token is null)
                    return new BeResponseGt<string> { Message = "Risposta SOAP non valida: token non trovato", Action = "loginsoap", Value = null };

                return new BeResponseGt<string> { Message = "OK", Action = "loginsoap", Value = token };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in LoginSoap per utente {User}", user);
                return new BeResponseGt<string> { Message = ex.Message, Action = "loginsoap", Value = null };
            }
        }

        public async Task<BeResponseGt<string>> LogOffSoap(string token)
        {
            string logoffbody =
                $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                  <soap12:Body>
                    <LogOff xmlns=""http://microarea.it/LoginManager/"">
                        <authenticationToken>{Esc(token)}</authenticationToken>
                    </LogOff>
                  </soap12:Body>
                </soap12:Envelope>";

            try
            {
                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(logoffbody, Encoding.UTF8, "application/soap+xml");
                using var response = await client.PostAsync(LoginManagerUri, content);
                await response.Content.ReadAsStringAsync();

                return new BeResponseGt<string> { Message = "OK", Action = "LogOffSoap", Value = token };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in LogOffSoap");
                return new BeResponseGt<string> { Message = ex.Message, Action = "LogOffSoap", Value = token };
            }
        }

        public async Task<BeResponseGt<string>> LoginSoapBySettings()
        {
            try
            {
                return await LoginSoap(_mlkSettings.LoginUsername, _mlkSettings.LoginPassword, _mlkSettings.LoginCompany, "MlkRestGate");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in LoginSoapBySettings");
                return new BeResponseGt<string> { Message = ex.Message, Action = "POST", Value = null };
            }
        }

        public async Task<BeResponseGt<string>> TBDMSGetAttachmentIDByFileNameSoap(string authenticationToken, string documentNamespace, string documentKey, string fileName)
        {
            string requestbody =
            $@"<?xml version=""1.0"" encoding=""utf-8""?>
              <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                 <soap12:Body>
                   <GetAttachmentIDByFileName xmlns=""http://microarea.it/TbServices/"">
                      <authenticationToken>{Esc(authenticationToken)}</authenticationToken>
                      <documentNamespace>{Esc(documentNamespace)}</documentNamespace>
                      <documentKey>{Esc(documentKey)}</documentKey>
                      <fileName>{Esc(fileName)}</fileName>
                   </GetAttachmentIDByFileName>
                 </soap12:Body>
             </soap12:Envelope>";

            try
            {
                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(requestbody, Encoding.UTF8, "application/soap+xml");
                using var response = await client.PostAsync(TbServicesUri, content);
                var soapResponse = await response.Content.ReadAsStringAsync();
                var soap = XDocument.Parse(soapResponse);

                var idallegato = GetSoapValue(soap, "GetAttachmentIDByFileNameResponse", "GetAttachmentIDByFileNameResult");
                if (idallegato is null)
                    return new BeResponseGt<string> { Action = "TBDMSGetAttachmentIDByFileNameSoap", Message = "Risposta SOAP non valida", Value = string.Empty };

                return new BeResponseGt<string> { Action = "TBDMSGetAttachmentIDByFileNameSoap", Message = "OK", Value = idallegato };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in TBDMSGetAttachmentIDByFileNameSoap");
                return new BeResponseGt<string> { Action = "TBDMSGetAttachmentIDByFileNameSoap", Message = ex.Message, Value = string.Empty };
            }
        }

        public async Task<BeResponseGt<string>> TBDMSGetOnlyAttachmentBinaryContent(string authenticationToken, string attachmentID, string fileName, string bVeryLargeFile = "false")
        {
            string requestbody =
           $@"<?xml version=""1.0"" encoding=""utf-8""?>
              <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                 <soap12:Body>
                   <GetAttachmentBinaryContent xmlns=""http://microarea.it/TbServices/"">
                      <authenticationToken>{Esc(authenticationToken)}</authenticationToken>
                      <attachmentID>{Esc(attachmentID)}</attachmentID>
                      <binaryContent></binaryContent>
                      <fileName>{Esc(fileName)}</fileName>
                      <bVeryLargeFile>{Esc(bVeryLargeFile)}</bVeryLargeFile>
                   </GetAttachmentBinaryContent>
                 </soap12:Body>
             </soap12:Envelope>";

            try
            {
                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(requestbody, Encoding.UTF8, "application/soap+xml");
                using var response = await client.PostAsync(TbServicesUri, content);
                var soapResponse = await response.Content.ReadAsStringAsync();
                var soap = XDocument.Parse(soapResponse);

                var base64content = GetSoapValue(soap, "GetAttachmentBinaryContentResponse", "binaryContent");
                if (base64content is null)
                    return new BeResponseGt<string> { Action = "TBDMSGetAttachmentBinaryContent", Message = "Risposta SOAP non valida", Value = string.Empty };

                return new BeResponseGt<string> { Action = "TBDMSGetAttachmentBinaryContent", Message = "OK", Value = base64content };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in TBDMSGetOnlyAttachmentBinaryContent");
                return new BeResponseGt<string> { Action = "TBDMSGetAttachmentBinaryContent", Message = ex.Message, Value = string.Empty };
            }
        }

        public async Task<BeResponseGt<GetAttachmentBinaryContentResponse>> TBDMSGetAttachmentBinaryContent(string authenticationToken, string attachmentID, string fileName, string bVeryLargeFile = "false")
        {
            string requestbody =
           $@"<?xml version=""1.0"" encoding=""utf-8""?>
              <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                 <soap12:Body>
                   <GetAttachmentBinaryContent xmlns=""http://microarea.it/TbServices/"">
                      <authenticationToken>{Esc(authenticationToken)}</authenticationToken>
                      <attachmentID>{Esc(attachmentID)}</attachmentID>
                      <binaryContent></binaryContent>
                      <fileName>{Esc(fileName)}</fileName>
                      <bVeryLargeFile>{Esc(bVeryLargeFile)}</bVeryLargeFile>
                   </GetAttachmentBinaryContent>
                 </soap12:Body>
             </soap12:Envelope>";

            try
            {
                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(requestbody, Encoding.UTF8, "application/soap+xml");
                using var response = await client.PostAsync(TbServicesUri, content);
                var soapResponse = await response.Content.ReadAsStringAsync();
                var soap = XDocument.Parse(soapResponse);

                var resultDescendant = GetSoapResponseNode(soap, "GetAttachmentBinaryContentResponse");
                if (resultDescendant is null)
                    return new BeResponseGt<GetAttachmentBinaryContentResponse> { Action = "TBDMSGetAttachmentBinaryContent", Message = "Risposta SOAP non valida", Value = null };

                var res = new GetAttachmentBinaryContentResponse
                {
                    BinaryContent = resultDescendant.Descendants().SingleOrDefault(n => n.Name.LocalName == "binaryContent")?.Value ?? string.Empty,
                    FileName = resultDescendant.Descendants().SingleOrDefault(n => n.Name.LocalName == "fileName")?.Value ?? string.Empty,
                    BVeryLargeFile = resultDescendant.Descendants().SingleOrDefault(n => n.Name.LocalName == "bVeryLargeFile")?.Value == "true",
                    GetAttachmentBinaryContentResult = resultDescendant.Descendants().SingleOrDefault(p => p.Name.LocalName == "GetAttachmentBinaryContentResult")?.Value == "true"
                };

                return new BeResponseGt<GetAttachmentBinaryContentResponse> { Action = "TBDMSGetAttachmentBinaryContent", Message = "OK", Value = res };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in TBDMSGetAttachmentBinaryContent");
                return new BeResponseGt<GetAttachmentBinaryContentResponse> { Action = "TBDMSGetAttachmentBinaryContent", Message = ex.Message, Value = null };
            }
        }

        public async Task<BeResponseGt<AttachBinaryContentInDocumentResponse>> AttachBinaryContentInDocument(string authenticationToken, string binaryContent, string sourceFileName, string description, string documentNamespace, string documentKey, string attachmentID)
        {
            string requestbody = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                                <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                                  <soap12:Body>
                                    <AttachBinaryContentInDocument xmlns=""http://microarea.it/TbServices/"">
                                      <authenticationToken>{Esc(authenticationToken)}</authenticationToken>
                                      <binaryContent>{binaryContent}</binaryContent>
                                      <sourceFileName>{Esc(sourceFileName)}</sourceFileName>
                                      <description>{Esc(description)}</description>
                                      <documentNamespace>{Esc(documentNamespace)}</documentNamespace>
                                      <documentKey>{Esc(documentKey)}</documentKey>
                                      <attachmentID>{Esc(attachmentID)}</attachmentID>
                                      <result></result>
                                    </AttachBinaryContentInDocument>
                                  </soap12:Body>
                                </soap12:Envelope>";

            try
            {
                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(requestbody, Encoding.UTF8, "application/soap+xml");
                using var response = await client.PostAsync(TbServicesUri, content);
                var soapResponse = await response.Content.ReadAsStringAsync();
                var soap = XDocument.Parse(soapResponse);

                var nodomaster = GetSoapResponseNode(soap, "AttachBinaryContentInDocumentResponse");
                if (nodomaster is null)
                    return new BeResponseGt<AttachBinaryContentInDocumentResponse> { Action = "AttachBinaryContentInDocument", Message = "Risposta SOAP non valida", Value = null };

                var attachResponse = new AttachBinaryContentInDocumentResponse
                {
                    AttachBinaryContentInDocumentResult = nodomaster.Descendants().SingleOrDefault(n => n.Name.LocalName == "AttachBinaryContentInDocumentResult")?.Value == "true",
                    AttachmentID = int.TryParse(nodomaster.Descendants().SingleOrDefault(n => n.Name.LocalName == "attachmentID")?.Value, out var id) ? id : 0,
                    Result = nodomaster.Descendants().SingleOrDefault(n => n.Name.LocalName == "result")?.Value ?? string.Empty
                };

                return new BeResponseGt<AttachBinaryContentInDocumentResponse> { Action = "AttachBinaryContentInDocument", Message = "OK", Value = attachResponse };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AttachBinaryContentInDocument");
                return new BeResponseGt<AttachBinaryContentInDocumentResponse> { Action = "AttachBinaryContentInDocument", Message = ex.Message, Value = null };
            }
        }

        public async Task<BeResponseGt<List<string>>> GetListaAllegatiOrigine(string AziendaOrigine, string KeyOrigine, string NamespaceDoc)
        {
            //esempio di keyorigine : SaleDocId:126; (deve finire con il ";")
            BeResponseGt<string> loginOrigine = new();
            try
            {
                //mi connetto all'azienda origine
                loginOrigine = await LoginSoap(_mlkSettings.LoginUsername, _mlkSettings.LoginPassword, AziendaOrigine, "eatrasf");

                if (loginOrigine.Message == "OK")
                {
                    //recupero da azienda origine gli id degli allegati
                    string requestbodygetids =
                    $@"<?xml version=""1.0"" encoding=""utf-8""?>
                        <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                          <soap12:Body>
                            <SearchAttachmentsForDocument xmlns=""http://microarea.it/TbServices/"">
                              <authenticationToken>{Esc(loginOrigine.Value!)}</authenticationToken>
                              <documentNamespace>{Esc(NamespaceDoc)}</documentNamespace>
                              <documentKey>{Esc(KeyOrigine)};</documentKey>
                              <searchText></searchText>
                              <location>0</location>
                              <searchFields></searchFields>
                            </SearchAttachmentsForDocument>
                          </soap12:Body>
                        </soap12:Envelope>";

                    var client = _httpClientFactory.CreateClient();
                    var content = new StringContent(requestbodygetids, Encoding.UTF8, "application/soap+xml");
                    using var response = await client.PostAsync(TbServicesUri, content);
                    var soapResponse = await response.Content.ReadAsStringAsync();
                    var soap = XDocument.Parse(soapResponse);

                    var searchResult = GetSoapResponseNode(soap, "SearchAttachmentsForDocumentResponse")
                        ?.Descendants()
                        .SingleOrDefault(n => n.Name.LocalName == "SearchAttachmentsForDocumentResult");

                    var listaids = searchResult?.Descendants().Select(e => e.Value).ToList() ?? [];

                    await LogOffSoap(loginOrigine.Value!);
                    return new BeResponseGt<List<string>> { Action = "GetListaAllegatiOrigine", Message = "OK", Value = listaids };
                }
                else
                {
                    return new BeResponseGt<List<string>> { Action = "GetListaAllegatiOrigine", Message = "Login fallito", Value = null };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetListaAllegatiOrigine per azienda {Azienda}", AziendaOrigine);
                if (!string.IsNullOrEmpty(loginOrigine.Value)) await LogOffSoap(loginOrigine.Value);
                return new BeResponseGt<List<string>> { Action = "GetListaAllegatiOrigine", Message = ex.Message, Value = null };
            }
        }

        /// <summary>
        /// Trasferisce allegati da un documento origine a un documento destinazione
        /// </summary>
        /// <param name="authenticationTokenOrigine">token ottenuto dal login all'azienda sorgente</param>
        /// <param name="authenticationTokenDestinazione">token ottenuto dal login all'azienda destinazione</param>
        /// <param name="ids">lista degli id degli allegati del documento origine</param>
        /// <param name="namespaceorigine">Namespace del documento origine es: Document.ERP.Sales.Documents.Invoice</param>
        /// <param name="namespacedestinazione">Namespace del documento destinazione es: Document.ERP.Sales.Documents.Invoice</param>
        /// <param name="destinationkey">chiave del documento di destinazione es: SaleDocId:198; </param>
        /// <param name="description">descrizione dell'allegato </param>
        /// <param name="bVeryLargeFile"></param>
        /// <returns></returns>
        public async Task<BeResponseGt<string>> TranferDocuments(string authenticationTokenOrigine,
                                                                 string authenticationTokenDestinazione,
                                                                 List<string> ids,
                                                                 string namespaceorigine,
                                                                 string namespacedestinazione,
                                                                 string destinationkey,
                                                                 string description,
                                                                 string bVeryLargeFile = "false")
        {
            BeResponseGt<string> beresp = new() { Action = "TranferDocuments", Message = "OK", Value = "" };
            string risultato = "";

            foreach (string idstring in ids)
            {
                var res = await TBDMSGetAttachmentBinaryContent(authenticationTokenOrigine, idstring, "false");
                if (res.Message == "OK" && res.Value is not null)
                {
                    var resp = res.Value;
                    await AttachBinaryContentInDocument(authenticationTokenDestinazione,
                                                       resp.BinaryContent,
                                                       resp.FileName,
                                                       description,
                                                       namespacedestinazione,
                                                       destinationkey,
                                                       "0");

                    risultato += $"[{resp.FileName}]";
                }
                else
                {
                    risultato += $"[errore id: {idstring}]";
                }
            }
            beresp.Value = risultato;
            return beresp;
        }
    }
}
