using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SliderCaptcha.Models;
using System.Text.RegularExpressions;

namespace SliderCaptcha.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test_ExportCalImage_CorrectExportInFolder()
        {
            var dir = "../../../testImage/0.jpg";

            using var image = (Image<Rgb24>)Image.Load(dir);

            var captchaDirection = CaptchaDirection.BottomToTop;
            var width = 280;
            var height = 150;

            var (backgroundBase64String, sliderBase64String, slideOffset) = SliderCaptchaTool.GenerateSliderCaptchaImages(image, captchaDirection, width, height);

            string background = Regex.Replace(backgroundBase64String, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
            var backgroundBytes = Convert.FromBase64String(background);
            Image.Load(backgroundBytes).SaveAsPng("../../../testImage/result/bg.png");

            string slider = Regex.Replace(sliderBase64String, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
            var sliderBytes = Convert.FromBase64String(slider);
            Image.Load(sliderBytes).SaveAsPng("../../../testImage/result/slider.png");
        }
    }
}