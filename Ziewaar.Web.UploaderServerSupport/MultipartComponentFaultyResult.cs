using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class MultipartComponentFaultyResult : IMultipartComponentResult
    {
        private readonly string _fault;
        public string Name { get { return "FAULT"; } }
        public string Fault { get { return _fault; } }
        public MultipartComponentFaultyResult(string fault)
        {
            this._fault = fault;
        }

        public void GiveBinaryData(byte[] data, int start, int length)
        {

        }

        public void Finish()
        {
        }

        public void Dispose()
        {
        }
    }
}
