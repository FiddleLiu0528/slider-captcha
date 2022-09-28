using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SliderCaptcha.Models
{
    /// <summary>
    /// Slider Captcha Models
    /// </summary>
    public class SliderCaptchaModels
    {
        /// <summary>
        /// Slider Captcha Get Dto
        /// </summary>
        public class SliderCaptcha_Get_Dto
        {
            /// <summary>
            /// Captcha Direction
            /// </summary>
            [DefaultValue("LeftToRight")]
            public CaptchaDirection CaptchaDirection { get; set; } = CaptchaDirection.LeftToRight;

            /// <summary>
            /// Length And Width
            /// </summary>
            public WidthAndHeight? WidthAndHeight { get; set; }
        }

        /// <summary>
        /// SliderCaptcha Get Result Dto
        /// </summary>
        public class SliderCaptcha_Get_ResultDto
        {
            /// <summary>
            /// Captcha Direction
            /// </summary>
            public CaptchaDirection CaptchaDirection { get; set; }

            /// <summary>
            /// Image Width
            /// </summary>
            public int ImageWidth { get; set; }

            /// <summary>
            /// Image Height
            /// </summary>
            public int ImageHeight { get; set; }

            /// <summary>
            /// Background Base64 String
            /// </summary>
            public string BackgroundBase64String { get; set; } = string.Empty;

            /// <summary>
            /// Slider Base64 String
            /// </summary>
            public string SliderBase64String { get; set; } = string.Empty;

            /// <summary>
            /// Slide Offset
            /// </summary>
            public int? SlideOffset { get; set; }
        }

        /// <summary>
        /// Length And Width
        /// </summary>
        public class WidthAndHeight
        {
            /// <summary>
            /// generate target image width, limit range from 128 to 2048
            /// default width 280 pixel
            /// </summary>
            [Range(128, 2048)]
            [DefaultValue(280)]
            public int? Width { get; set; }

            /// <summary>
            /// generate target image height, limit range from 128 to 2048
            /// default height 150 pixel
            /// </summary>
            [Range(128, 2048)]
            [DefaultValue(150)]
            public int? Height { get; set; }

        }
    }
}
