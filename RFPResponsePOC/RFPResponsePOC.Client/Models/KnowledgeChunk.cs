using System;

namespace RFPResponseAPP.Client.Models
{
    public class KnowledgeChunk
    {
        public string Id { get; set; }
        public string EntryTitle { get; set; }
        public string Content { get; set; }
        public string Embedding { get; set; }
    }
}
