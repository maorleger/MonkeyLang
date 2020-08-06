using Microsoft.Language.Xml;
using System;
using System.Collections.Concurrent;

namespace MonkeyLang.LangServer
{
    internal class BufferManager
    {
        private readonly ConcurrentDictionary<string, Microsoft.Language.Xml.Buffer> buffers = new ConcurrentDictionary<string, Microsoft.Language.Xml.Buffer>();

        public void UpdateBuffer(string documentPath, Microsoft.Language.Xml.Buffer buffer)
        {
            buffers.AddOrUpdate(documentPath, buffer, (k, v) => buffer);
        }

        public Microsoft.Language.Xml.Buffer GetBuffer(string documentPath)
        {
            return buffers.TryGetValue(documentPath, out var buffer) ? buffer : null;
        }

        internal void RemoveBuffer(string documentPath)
        {
            buffers.TryRemove(documentPath, out _);
        }
    }
}