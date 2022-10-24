using System.Collections.Generic;
using System.Drawing;
using ZXing;

namespace Minadzuki
{
    partial class ZXingCode
    {
        #region DecoderOption
        public enum DecoderOption
        {
            DECODER_RETAIL,
            DECODER_INDUSTRAIL,
            DECODER_QRCODE,
            DECODER_LICENSE,
            DECODER_2D,
            DECODER_TEST,
            DECODER_SINGLE
        }

        public class DecoderList
        {
            public string Name { get; set; }
            public DecoderOption Code { get; set; }
        }

        public static List<DecoderList> DecoderLists = new List<DecoderList>()
        {
            new DecoderList
            {
                Name = "Retail",
                Code = DecoderOption.DECODER_RETAIL
            },
            new DecoderList
            {
                Name = "Industrail",
                Code = DecoderOption.DECODER_INDUSTRAIL
            },
            new DecoderList
            {
                Name = "QRCode",
                Code = DecoderOption.DECODER_QRCODE
            },
            new DecoderList
            {
                Name = "License",
                Code = DecoderOption.DECODER_LICENSE
            },
            new DecoderList
            {
                Name = "2D",
                Code = DecoderOption.DECODER_2D
            },
            new DecoderList
            {
                Name = "Test",
                Code = DecoderOption.DECODER_TEST
            },
            new DecoderList
            {
                Name = "Single",
                Code = DecoderOption.DECODER_SINGLE
            }
        };

        public static BarcodeReader SetDecoderFormat(DecoderOption option)
        {
            BarcodeReader reader = new BarcodeReader();

            switch (option)
            {
                case DecoderOption.DECODER_RETAIL:
                    reader.Options.PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.UPC_A,
                        BarcodeFormat.UPC_E
                    };
                    break;
                case DecoderOption.DECODER_INDUSTRAIL:
                    reader.Options.PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.CODE_128
                    };
                    break;
                case DecoderOption.DECODER_QRCODE:
                    reader.Options.PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.QR_CODE
                    };
                    break;
                case DecoderOption.DECODER_LICENSE:
                    reader.Options.PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.PDF_417
                    };
                    break;
                case DecoderOption.DECODER_2D:
                    reader.Options.PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.QR_CODE,
                        BarcodeFormat.AZTEC,
                        BarcodeFormat.DATA_MATRIX
                    };
                    break;
                case DecoderOption.DECODER_TEST:
                    reader.Options.PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.EAN_13
                    };
                    break;
                case DecoderOption.DECODER_SINGLE:
                    reader.Options.PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.EAN_13
                    };
                    break;
            }

            return reader;
        }
        #endregion

        #region Detailed Output
        public struct DecodeDetailed
        {
            public string Text;
            public Rectangle Box;
            public Bitmap ProcessedSrc;
        }

        public enum ProcessedType
        {
            IMAGE_BOXED,
            IMAGE_PROCESSED
        }

        public class ProcessedList
        {
            public string Name { get; set; }
            public ProcessedType Code { get; set; }
        }

        public static List<ProcessedList> ProcessedLists = new List<ProcessedList>()
        {
            new ProcessedList
            {
                Name = "Boxed",
                Code = ProcessedType.IMAGE_BOXED
            },
            new ProcessedList
            {
                Name = "Image Processed",
                Code = ProcessedType.IMAGE_PROCESSED
            }
        };
        #endregion
    }
}