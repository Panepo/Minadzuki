using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using ZXing;

namespace Minadzuki
{
    partial class ZXingCode
    {
        public static string ImageDecode(Bitmap image, DecoderOption option, List<ImagePR> conf)
        {
            BarcodeReader reader = SetDecoderFormat(option);

            using (var t = new ResourcesTracker())
            {
                Mat src = t.T(BitmapConverter.ToMat(image));
                Mat enhanced = ImageEnhance(src, conf);

                using (Bitmap bmp = BitmapConverter.ToBitmap(enhanced))
                {
                    Result result = reader.Decode(bmp);
                    if (result == null) return "not detected";

                    return result.Text;
                }
            }
        }

        public static DecodeDetailed ImageDecodeDetail(Bitmap image, DecoderOption option, List<ImagePR> conf, ProcessedType outConf = ProcessedType.IMAGE_BOXED)
        {
            BarcodeReader reader = SetDecoderFormat(option);
            DecodeDetailed output = new DecodeDetailed();

            using (var t = new ResourcesTracker())
            {
                Mat src = t.T(BitmapConverter.ToMat(image));
                Mat enhanced = ImageEnhance(src, conf);

                using (Bitmap bmp = BitmapConverter.ToBitmap(enhanced))
                {
                    Result result = reader.Decode(bmp);
                    if (result == null)
                    {
                        output.Text = "not detected";
                        output.Box = new Rectangle();
                        output.ProcessedSrc = bmp;
                        return output;
                    }
                    
                    output.Text = result.Text;

                    System.Drawing.Point loc = new System.Drawing.Point();
                    System.Drawing.Size siz = new System.Drawing.Size();

                    GetBoundingBox(result.ResultPoints, out loc, out siz);
                    output.Box = new Rectangle(loc, siz);

                    switch (outConf)
                    {
                        case ProcessedType.IMAGE_BOXED:
                            if (conf.Contains(ImagePR.IMAGE_RESIZE))
                            {
                                if (image.Width <= 300 || image.Height <= 300)
                                {
                                    float scale;

                                    if (image.Width > image.Height) scale = 300 / image.Height;
                                    else scale = 300 / image.Width;

                                    output.ProcessedSrc = DrawBoundingBox(ResizeImage(image, (int)(image.Width * scale), (int)(image.Height * scale)), output.Box);
                                }
                                else output.ProcessedSrc = DrawBoundingBox(image, output.Box);
                            }
                            else output.ProcessedSrc = DrawBoundingBox(image, output.Box);
                            break;
                        case ProcessedType.IMAGE_PROCESSED:
                            output.ProcessedSrc = DrawBoundingBox(bmp, output.Box);
                            break;
                    }


                    return output;
                }
            }
        }

        private static void GetBoundingBox(ResultPoint[] points, out System.Drawing.Point loc, out System.Drawing.Size siz)
        {
            float width = points[1].X - points[2].X;
            float height = points[0].Y - points[1].Y;

            if (height < 0)
            {
                if (width < 0)
                {
                    loc = new System.Drawing.Point((int)(points[0].X - width), (int)(points[0].Y - height));
                    siz = new System.Drawing.Size((int)Math.Abs(width), (int)Math.Abs(height));
                }
                else
                {
                    loc = new System.Drawing.Point((int)points[0].X, (int)(points[0].Y - height));
                    siz = new System.Drawing.Size((int)width, (int)Math.Abs(height));
                }
            }
            else
            {
                if (width < 0)
                {
                    loc = new System.Drawing.Point((int)(points[0].X - width), (int)points[0].Y);
                    siz = new System.Drawing.Size((int) Math.Abs(width), (int)height);
                }
                else
                {
                    loc = new System.Drawing.Point((int)points[0].X, (int)points[0].Y);
                    siz = new System.Drawing.Size((int)width, (int)height);
                }
            }
        }

        private static Bitmap DrawBoundingBox(Bitmap src, Rectangle box)
        {
            Bitmap dst = new Bitmap(src);
            using (Graphics graphic = Graphics.FromImage(dst))
            {
                Pen pen = new Pen(Color.Red, 2);
                graphic.DrawRectangle(pen, box);

                return dst;
            }
        }

        private static Bitmap ResizeImage(Bitmap src, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(src, destRect, 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}