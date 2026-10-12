using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class MultipartInputFieldResult : IMultipartComponentResult
    {
        private readonly string _name;
        private readonly List<byte> Data = new List<byte>();
        private string _finishedValue;

        public string Name { get { return _name; } }
        public string FinishedValue { get { return _finishedValue; } }

        public MultipartInputFieldResult(string name)
        {
            this._name = name;
        }

        public void GiveBinaryData(byte[] data, int start, int length)
        {
            this.Data.AddRange(data.Skip(start).Take(length).ToArray());
        }

        public void Finish()
        {
            var bytes = Data.ToArray();
            _finishedValue = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public string Fault
        {
            get { return null; }
        }

        public bool IsExpectingContent
        {
            get { return true; }
        }

        public void Dispose()
        {

        }
    }
}
