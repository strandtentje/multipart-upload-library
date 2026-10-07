using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ziewaar.Web.UploaderServerSupport;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing.Text;

namespace Ziewaar.Web.GdiProgressGrapher
{
    public class GdiProgressGrapher : IProgressGrapher
    {
        public readonly int Width, Height;
        private readonly byte[]
            BareBoundaryBytes,
            CrlfCrlfBoundaryBytesCrlf,
            CrlfBytes,
            PartHeaderBytes;
        public GdiProgressGrapher(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.BareBoundaryBytes = Encoding.ASCII.GetBytes("--PROGRESS");
            this.CrlfCrlfBoundaryBytesCrlf = Encoding.ASCII.GetBytes(
                "\r\n\r\n--PROGRESS\r\n");
            this.CrlfBytes = Encoding.ASCII.GetBytes("\r\n");
            this.PartHeaderBytes = Encoding.ASCII.GetBytes(
                "Content-Type: image/png\r\nContent-Length: ");
        }

        private static readonly SolidBrush
            Bar = new SolidBrush(Color.DodgerBlue),
            Text = new SolidBrush(Color.Black);
        private static readonly Font
            ProgressFont = new Font(
                FontFamily.GenericSansSerif, 11, FontStyle.Bold);

        public void Run(UploadBox selectedUploadBox, HttpListenerResponse rsp)
        {
            byte[] copyBuffer = new byte[2048];
            using (var targetBmp = new Bitmap(this.Width, this.Height, 
                PixelFormat.Format32bppArgb))
            using (var ms = new MemoryStream())
            using (var gfx = Graphics.FromImage(targetBmp))
            {
                rsp.ContentType = string.Format(
                    "multipart/x-mixed-replace; boundary=PROGRESS");
                rsp.AddHeader("Cache-Control", "no-cache");
                rsp.KeepAlive = true;

                rsp.OutputStream.Write(BareBoundaryBytes, 0, BareBoundaryBytes.Length);
                rsp.OutputStream.Write(CrlfBytes, 0, CrlfBytes.Length);
                gfx.InterpolationMode = InterpolationMode.High;
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.TextRenderingHint = TextRenderingHint.AntiAlias;

                string message;
                SizeF textSize;
                float textX, textY;
                long imageContentLength;
                byte[] contentLengthNumberBytes;

                while (!selectedUploadBox.TryWaitForFinish(out message))
                {
                    gfx.Clear(Color.Transparent);

                    if (selectedUploadBox.ContentLengthAnnounced > 0)
                    {
                        message = string.Format(
                            "{0}% , {1}/s, {2}",
                            selectedUploadBox.Percentage,
                            selectedUploadBox.GetNormalizedSpeed(),
                            selectedUploadBox.GetNormalizedSize());

                        gfx.FillRectangle(Bar, 0, 0,
                            (selectedUploadBox.Percentage * Width) / 100,
                            Height);
                    }
                    else
                    {
                        message = "waiting...";
                    }

                    textSize = gfx.MeasureString(message, ProgressFont);
                    textX = ((float)Width / 2f) - (float)textSize.Width / 2f;
                    textY = ((float)Height / 2f) - (float)textSize.Height / 2f;

                    gfx.DrawString(message, ProgressFont, Text, textX, textY);
                    gfx.Flush(FlushIntention.Sync);
                    gfx.Save();

                    ms.Position = 0;
                    targetBmp.Save(ms, ImageFormat.Png);
                    imageContentLength = ms.Position;
                    ms.Position = 0;

                    rsp.OutputStream.Write(PartHeaderBytes,
                        0, PartHeaderBytes.Length);
                    contentLengthNumberBytes =
                        Encoding.ASCII.GetBytes(imageContentLength.ToString());
                    rsp.OutputStream.Write(contentLengthNumberBytes, 0,
                        contentLengthNumberBytes.Length);
                    rsp.OutputStream.Write(CrlfBytes, 0, CrlfBytes.Length);
                    rsp.OutputStream.Write(CrlfBytes, 0, CrlfBytes.Length);

                    for (var copyCount = ms.Read(copyBuffer, 0, copyBuffer.Length);
                        copyCount > 0;
                        copyCount = ms.Read(copyBuffer, 0, copyBuffer.Length))
                        rsp.OutputStream.Write(copyBuffer, 0, copyCount);

                    rsp.OutputStream.Write(CrlfCrlfBoundaryBytesCrlf, 0,
                        CrlfCrlfBoundaryBytesCrlf.Length);
                }

                gfx.Clear(Color.Transparent);
                gfx.FillRectangle(Bar, 0, 0, Width, Height);

                textSize = gfx.MeasureString(message, ProgressFont);
                textX = ((float)Width / 2f) - (float)textSize.Width;
                textY = ((float)Height / 2f) - (float)textSize.Height;

                gfx.DrawString(message, ProgressFont, Text, textX, textY);
                gfx.Flush(FlushIntention.Sync);
                gfx.Save();

                ms.Position = 0;
                targetBmp.Save(ms, ImageFormat.Png);
                imageContentLength = ms.Position;
                ms.Position = 0;

                rsp.OutputStream.Write(PartHeaderBytes,
                    0, PartHeaderBytes.Length);
                contentLengthNumberBytes =
                    Encoding.ASCII.GetBytes(imageContentLength.ToString());
                rsp.OutputStream.Write(contentLengthNumberBytes, 0,
                    contentLengthNumberBytes.Length);
                rsp.OutputStream.Write(CrlfBytes, 0, CrlfBytes.Length);
                rsp.OutputStream.Write(CrlfBytes, 0, CrlfBytes.Length);

                for (var copyCount = ms.Read(copyBuffer, 0, copyBuffer.Length);
                    copyCount > 0;
                    copyCount = ms.Read(copyBuffer, 0, copyBuffer.Length))
                    rsp.OutputStream.Write(copyBuffer, 0, copyCount);

                rsp.OutputStream.Write(CrlfCrlfBoundaryBytesCrlf, 0,
                    CrlfCrlfBoundaryBytesCrlf.Length);
            }
        }
    }
}
