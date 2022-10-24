using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Minadzuki
{
    partial class ZXingCode
    {
        public static string ImageDecode(string imagePath, DecoderOption option, List<ImagePR> conf)
        {
            try
            {
                using (Image Dummy = Image.FromFile(imagePath))
                {
                    return ImageDecode(new Bitmap(Dummy), option, conf);
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(@"File not found.", e);
            }
        }

        public static DecodeDetailed ImageDecodeDetail(string imagePath, DecoderOption option, List<ImagePR> conf, ProcessedType outConf = ProcessedType.IMAGE_BOXED)
        {
            try
            {
                using (Image Dummy = Image.FromFile(imagePath))
                {
                    return ImageDecodeDetail(new Bitmap(Dummy), option, conf, outConf);
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(@"File not found.", e);
            }
        }
    }
}