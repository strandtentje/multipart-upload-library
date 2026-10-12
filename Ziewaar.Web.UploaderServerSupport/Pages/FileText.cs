using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class FileText
    {
        public readonly string Text;
        private int Cursor = 0;
        public FileText(string file)
        {
            Text = File.ReadAllText(file);
        }

        public bool GoTo(string part, out int startedAt, out int count)
        {
            startedAt = Cursor;
            if (Cursor >= Text.Length) 
            {
                count = 0;
                return false;
            }

            count = Text.IndexOf(part, startedAt) - startedAt;
            if (count < 0)
            {
                count = Text.Length - startedAt;
                Cursor = Text.Length;
                return true;
            }
            else
            {
                Cursor = startedAt + count + part.Length;
                return true;
            }
        }
    }
}
