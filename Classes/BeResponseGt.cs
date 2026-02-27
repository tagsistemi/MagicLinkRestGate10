namespace MagicLinkRestGate.Classes
{
    public class BeResponseGt<T>
    {
        public string Message { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public T? Value { get; set; }
    }

    public class GetAttachmentBinaryContentResponse
    {
        public bool GetAttachmentBinaryContentResult { get; set; }
        public string BinaryContent { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public bool BVeryLargeFile { get; set; }
    }

    public class AttachBinaryContentInDocumentResponse
    {
        public bool AttachBinaryContentInDocumentResult { get; set; }
        public int AttachmentID { get; set; }
        public string Result { get; set; } = string.Empty;
    }
}
