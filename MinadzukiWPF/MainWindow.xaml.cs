using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Controls.Primitives;
using Minadzuki;

namespace MinadzukiWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool processed = false;
        private readonly List<ZXingCode.ImagePR> conf = new List<ZXingCode.ImagePR>();
        private ZXingCode.DecoderOption option = ZXingCode.DecoderOption.DECODER_QRCODE;
        private ZXingCode.ProcessedType outConf = ZXingCode.ProcessedType.IMAGE_BOXED;

        public MainWindow()
        {
            InitializeComponent();

            foreach (ZXingCode.PRListItem item in ZXingCode.PRListItems)
            {
                PRListItems.Items.Add(item);

                if (item.IsSelected)
                {
                    conf.Add(item.Enum);
                }
            }

            foreach (ZXingCode.DecoderList decoder in ZXingCode.DecoderLists)
            {
                comboBoxDecoer.Items.Add(decoder.Name);
            }
            foreach (string item in comboBoxDecoer.Items)
            {
                if (item == "QRCode")
                {
                    comboBoxDecoer.SelectedValue = item;
                    break;
                }
            }

            foreach (ZXingCode.ProcessedList process in ZXingCode.ProcessedLists)
            {
                comboBoxOutput.Items.Add(process.Name);
            }
            foreach (string item in comboBoxOutput.Items)
            {
                if (item == "Boxed")
                {
                    comboBoxOutput.SelectedValue = item;
                    break;
                }
            }
        }

        private void ToggleButtonPRItemClick(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)(sender as ToggleButton).IsChecked;
            ZXingCode.PRListItem menuItem = (ZXingCode.PRListItem)(sender as FrameworkElement).DataContext;

            foreach (ZXingCode.PRListItem item in ZXingCode.PRListItems)
            {
                if (menuItem.Name == item.Name)
                {
                    if (isChecked) conf.Add(item.Enum);
                    else conf.Remove(item.Enum);

                    break;
                }
            }

            if (processed)
            {
                Bitmap src = FormatHelper.ImageSource2Bitmap(imgSrc.Source);
                ZXingCode.DecodeDetailed det = ZXingCode.ImageDecodeDetail(src, option, conf, outConf);
                textDst.Text = det.Text;
                imgDst.Source = FormatHelper.Bitmap2ImageSource(det.ProcessedSrc);
            }
        }

        private void ButtonFileClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files(*.png; *.jpg; *.jpeg; *.gif; *.bmp)|*.png; *.jpg; *.jpeg; *.gif; *.bmp"
            };
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                Bitmap src = new Bitmap(dlg.FileName);
                imgSrc.Source = FormatHelper.Bitmap2ImageSource(src);

                ZXingCode.DecodeDetailed det = ZXingCode.ImageDecodeDetail(src, option, conf, outConf);
                textDst.Text = det.Text;
                imgDst.Source = FormatHelper.Bitmap2ImageSource(det.ProcessedSrc);
                processed = true;
            }
        }

        private void ButtonClipboardClick(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                BitmapSource clip = Clipboard.GetImage();
                Bitmap src = FormatHelper.BitmapSource2Bitmap(clip);

                imgSrc.Source = FormatHelper.Bitmap2ImageSource(src);

                ZXingCode.DecodeDetailed det = ZXingCode.ImageDecodeDetail(src, option, conf, outConf);
                textDst.Text = det.Text;
                imgDst.Source = FormatHelper.Bitmap2ImageSource(det.ProcessedSrc);
                processed = true;
            }
        }

        private void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            if (processed)
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Images|*.png;*.bmp;*.jpg"
                };
                Nullable<bool> result = sfd.ShowDialog();

                if (result == true)
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imgDst.Source));
                    using (System.IO.FileStream stream = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                        encoder.Save(stream);
                }
            }
        }

        private void ComboBoxDecoerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxDecoer.SelectedItem is string content)
            {
                foreach (ZXingCode.DecoderList decoder in ZXingCode.DecoderLists)
                {
                    if (content == decoder.Name)
                    {
                        option = decoder.Code;
                        break;
                    }
                }
            }
        }

        private void ComboBoxOutSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxOutput.SelectedItem is string content)
            {
                foreach (ZXingCode.ProcessedList process in ZXingCode.ProcessedLists)
                {
                    if (content == process.Name)
                    {
                        outConf = process.Code;
                        break;
                    }
                }
            }
        }
    }
}
