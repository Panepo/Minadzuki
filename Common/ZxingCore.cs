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
                        output.ProcessedSrc = bmp;
                        return output;
                    }
                    
                    output.Text = result.Text;

                    switch (outConf)
                    {
                        case ProcessedType.IMAGE_BOXED:
                            output.ProcessedSrc = DrawInstruction(src, result.ResultPoints, result.BarcodeFormat, conf);
                            break;
                        case ProcessedType.IMAGE_PROCESSED:
                            output.ProcessedSrc = DrawInstruction(enhanced, result.ResultPoints, result.BarcodeFormat, conf);
                            break;
                    }

                    return output;
                }
            }
        }

        private static Mat ResizeMat(Mat src, List<ImagePR> conf)
        {
            Mat outputMat = src.Clone();

            double scale = 1;
            if (conf.Contains(ImagePR.IMAGE_RESIZE))
            {
                if (src.Width <= 300 || src.Height <= 300)
                {
                    if (src.Width > src.Height) scale = 300 / src.Height;
                    else scale = 300 / src.Width;
                }

                Cv2.Resize(src, outputMat, new OpenCvSharp.Size(), scale, scale, InterpolationFlags.Cubic);
            }

            return outputMat;
        }

        private static Bitmap DrawInstruction(Mat src, ResultPoint[] points, BarcodeFormat format, List<ImagePR> conf)
        {
            string type = format.ToString();
            int areaScanExtend;

            Mat overlay = ResizeMat(src, conf);
            Mat outputMat = ResizeMat(src, conf);

            foreach (string x in format1d)
            {
                if (type.Contains(x))
                {
                    OpenCvSharp.Point outLeft = new OpenCvSharp.Point((int)points[0].X, (int)points[0].Y);
                    OpenCvSharp.Point outRight = new OpenCvSharp.Point((int)points[1].X, (int)points[1].Y);
                    areaScanExtend = 20;

                    OpenCvSharp.Point rectPoint = new OpenCvSharp.Point(outLeft.X - areaScanExtend, outLeft.Y - areaScanExtend);
                    OpenCvSharp.Size rectSize = new OpenCvSharp.Size((outRight.X - outLeft.X) + areaScanExtend * 2, areaScanExtend * 2);
                    OpenCvSharp.Rect rect = new OpenCvSharp.Rect(rectPoint, rectSize);

                    OpenCvSharp.Cv2.Rectangle(overlay, rect, Scalar.LightGreen, 15);
                    OpenCvSharp.Cv2.AddWeighted(src, 0.7, overlay, 0.3, 0, outputMat);
                    OpenCvSharp.Cv2.Rectangle(outputMat, rect, Scalar.LightGreen, 2);

                    return BitmapConverter.ToBitmap(outputMat);
                }
            }

            foreach (string x in format1dx)
            {
                if (type.Contains(x))
                {
                    OpenCvSharp.Point outLeft = new OpenCvSharp.Point((int)points[0].X, (int)points[0].Y);
                    OpenCvSharp.Point outRight = new OpenCvSharp.Point((int)points[0].X, (int)points[0].Y);

                    foreach (ResultPoint pts in points)
                    {
                        if ((int)pts.X < outLeft.X)
                        {
                            outLeft.X = (int)pts.X;
                            outLeft.Y = (int)pts.Y;
                        }

                        if ((int)pts.X > outRight.X)
                        {
                            outRight.X = (int)pts.X;
                            outRight.Y = (int)pts.Y;
                        }
                    }

                    areaScanExtend = 20;
                    OpenCvSharp.Point rectPoint = new OpenCvSharp.Point(outLeft.X - areaScanExtend * 2, outLeft.Y - areaScanExtend);
                    OpenCvSharp.Size rectSize = new OpenCvSharp.Size((outRight.X - outLeft.X) + areaScanExtend * 4, areaScanExtend * 2);
                    OpenCvSharp.Rect rect = new OpenCvSharp.Rect(rectPoint, rectSize);

                    OpenCvSharp.Cv2.Rectangle(overlay, rect, Scalar.LightGreen, 15);
                    OpenCvSharp.Cv2.AddWeighted(src, 0.7, overlay, 0.3, 0, outputMat);
                    OpenCvSharp.Cv2.Rectangle(outputMat, rect, Scalar.LightGreen, 2);

                    return BitmapConverter.ToBitmap(outputMat);
                }
            }

            foreach (string x in format2d)
            {
                if (type.Contains(x) && points.Length <= 4)
                {
                    OpenCvSharp.Point[] outpt = new OpenCvSharp.Point[4];

                    for (int i = 0; i < points.Length; i += 1)
                        outpt[i] = new OpenCvSharp.Point(points[i].X, points[i].Y);

                    if (points.Length == 3)
                    {
                        float xdiv = outpt[0].X - outpt[1].X;
                        float ydiv = outpt[0].Y - outpt[1].Y;

                        outpt[3] = new OpenCvSharp.Point(points[2].X + xdiv, points[2].Y + ydiv);
                    }

                    RotatedRect rrec = Cv2.MinAreaRect(outpt);
                    OpenCvSharp.Point2f[] pointfs = rrec.Points();

                    for (int j = 0; j < pointfs.Length; j += 1)
                        Cv2.Line(overlay, new OpenCvSharp.Point((int)pointfs[j].X, (int)pointfs[j].Y), new OpenCvSharp.Point((int)pointfs[(j + 1) % 4].X, (int)pointfs[(j + 1) % 4].Y), Scalar.LightGreen, 15);

                    OpenCvSharp.Cv2.AddWeighted(src, 0.7, overlay, 0.3, 0, outputMat);

                    for (int j = 0; j < pointfs.Length; j += 1)
                        Cv2.Line(outputMat, new OpenCvSharp.Point((int)pointfs[j].X, (int)pointfs[j].Y), new OpenCvSharp.Point((int)pointfs[(j + 1) % 4].X, (int)pointfs[(j + 1) % 4].Y), Scalar.LightGreen, 2);

                    return BitmapConverter.ToBitmap(outputMat);
                }
            }

            foreach (string x in formatTest)
            {
                if (type.Contains(x))
                {
                    for (int i = 0; i < points.Length; i += 1)
                    {
                        OpenCvSharp.Point temPoint = new OpenCvSharp.Point((int)points[i].X, (int)points[i].Y);
                        Cv2.Circle(overlay, temPoint, 20, Scalar.LightGreen, 15);
                    }

                    Cv2.AddWeighted(src, 0.7, overlay, 0.3, 0, outputMat);

                    for (int i = 0; i < points.Length; i += 1)
                    {
                        OpenCvSharp.Point temPoint = new OpenCvSharp.Point((int)points[i].X, (int)points[i].Y);
                        Cv2.Circle(outputMat, temPoint, 20, Scalar.LightGreen, 2);
                    }

                    return BitmapConverter.ToBitmap(outputMat);
                }
            }

            return BitmapConverter.ToBitmap(outputMat);
        }
    }
}