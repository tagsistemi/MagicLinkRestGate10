using MagicLinkRestGate.Classes;

namespace MagicLinkRestGate.InterFaces
{
    public interface IMagoWSGateHelper
    {
        Task<BeResponseGt<string>> LoginSoap(string user, string passw, string companyname, string process);
        Task<BeResponseGt<string>> LoginSoapBySettings();
        Task<BeResponseGt<string>> LogOffSoap(string token);
        Task<BeResponseGt<string>> TBDMSGetAttachmentIDByFileNameSoap(string authenticationToken, string documentNamespace, string documentKey, string fileName);
        Task<BeResponseGt<GetAttachmentBinaryContentResponse>> TBDMSGetAttachmentBinaryContent(string authenticationToken, string attachmentID, string fileName, string bVeryLargeFile = "false");
        Task<BeResponseGt<string>> TBDMSGetOnlyAttachmentBinaryContent(string authenticationToken, string attachmentID, string fileName, string bVeryLargeFile = "false");
        Task<BeResponseGt<AttachBinaryContentInDocumentResponse>> AttachBinaryContentInDocument(string authenticationToken, string binaryContent, string sourceFileName, string description, string documentNamespace, string documentKey, string attachmentID);
        Task<BeResponseGt<List<string>>> GetListaAllegatiOrigine(string AziendaOrigine, string KeyOrigine, string NamespaceDoc);
        Task<BeResponseGt<string>> TranferDocuments(string authenticationTokenOrigine,
                                                     string authenticationTokenDestinazione,
                                                     List<string> ids,
                                                     string namespaceorigine,
                                                     string namespacedestinazione,
                                                     string destinationkey,
                                                     string description,
                                                     string bVeryLargeFile = "false");
    }
}
