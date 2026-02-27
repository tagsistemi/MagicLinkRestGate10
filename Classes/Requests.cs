namespace MagicLinkRestGate.Classes
{
    public class AttachBinaryContentInDocumentRequest
    {
        public string AuthenticationToken { get; set; } = string.Empty;
        public string BinaryContent { get; set; } = string.Empty;
        public string SourceFileName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DocumentNamespace { get; set; } = string.Empty;
        public string DocumentKey { get; set; } = string.Empty;
        public string AttachmentID { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
    }

    public class TranferDocumentsRequest
    {
        public string AuthenticationTokenOrigine { get; set; } = string.Empty;
        public string AuthenticationTokenDestinazione { get; set; } = string.Empty;
        public List<string> Ids { get; set; } = [];
        public string NamespaceOrigine { get; set; } = string.Empty;
        public string NamespaceDestinazione { get; set; } = string.Empty;
        public string Destinationkey { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BVeryLargeFile { get; set; } = "false";
    }
}
