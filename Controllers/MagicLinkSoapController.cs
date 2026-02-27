using MagicLinkRestGate.Classes;
using MagicLinkRestGate.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace MagicLinkRestGate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MagicLinkSoapController : ControllerBase
    {
        private readonly IMagoWSGateHelper _magoWSGateHelper;
        private readonly ILogger<MagicLinkSoapController> _logger;

        public MagicLinkSoapController(IMagoWSGateHelper magoWSGateHelper, ILogger<MagicLinkSoapController> logger)
        {
            _magoWSGateHelper = magoWSGateHelper;
            _logger = logger;
        }

        [HttpGet("GetListaAllegatiOrigine")]
        public async Task<BeResponseGt<List<string>>> GetListaAllegatiOrigine(string AziendaOrigine, string KeyOrigine, string NamespaceDoc)
        {
            return await _magoWSGateHelper.GetListaAllegatiOrigine(AziendaOrigine, KeyOrigine, NamespaceDoc);
        }

        [HttpPost("LoginSoap")]
        public async Task<BeResponseGt<string>> LoginSoap(string user, string passw, string companyname, string process)
        {
            return await _magoWSGateHelper.LoginSoap(user, passw, companyname, process);
        }

        /// <summary>
        /// Effettua il login con i parametri impostati nella web api
        /// </summary>
        [HttpPost("LoginSoapBySettings")]
        public async Task<BeResponseGt<string>> LoginSoapBySettings()
        {
            return await _magoWSGateHelper.LoginSoapBySettings();
        }

        [HttpPost("LogOffSoap")]
        public async Task<BeResponseGt<string>> LogOffSoap(string token)
        {
            return await _magoWSGateHelper.LogOffSoap(token);
        }

        [HttpPost("TBDMSGetAttachmentIDByFileNameSoap")]
        public async Task<BeResponseGt<string>> TBDMSGetAttachmentIDByFileNameSoap(string authenticationToken, string documentNamespace, string documentKey, string fileName)
        {
            return await _magoWSGateHelper.TBDMSGetAttachmentIDByFileNameSoap(authenticationToken, documentNamespace, documentKey, fileName);
        }

        [HttpPost("TBDMSGetAttachmentBinaryContent")]
        public async Task<BeResponseGt<GetAttachmentBinaryContentResponse>> TBDMSGetAttachmentBinaryContent(string authenticationToken, string attachmentID, string fileName, string bVeryLargeFile = "false")
        {
            return await _magoWSGateHelper.TBDMSGetAttachmentBinaryContent(authenticationToken, attachmentID, fileName, bVeryLargeFile);
        }

        [HttpPost("TBDMSAttachBinaryContentInDocument")]
        public async Task<BeResponseGt<AttachBinaryContentInDocumentResponse>> TBDMSAttachBinaryContentInDocument(AttachBinaryContentInDocumentRequest req)
        {
            return await _magoWSGateHelper.AttachBinaryContentInDocument(req.AuthenticationToken, req.BinaryContent, req.SourceFileName, req.Description, req.DocumentNamespace, req.DocumentKey, req.AttachmentID);
        }

        [HttpPost("TrasferisciAllegati")]
        public async Task<BeResponseGt<string>> TrasferisciAllegati(TranferDocumentsRequest req)
        {
            return await _magoWSGateHelper.TranferDocuments(req.AuthenticationTokenOrigine,
                                                            req.AuthenticationTokenDestinazione,
                                                            req.Ids,
                                                            req.NamespaceOrigine,
                                                            req.NamespaceDestinazione,
                                                            req.Destinationkey,
                                                            req.Description,
                                                            req.BVeryLargeFile);
        }
    }
}
