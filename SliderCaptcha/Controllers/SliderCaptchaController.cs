using Microsoft.AspNetCore.Mvc;
using SliderCaptcha.Models;
using static SliderCaptcha.Models.SliderCaptchaModels;

namespace SliderCaptcha.Controllers
{
    /// <summary>
    /// Slider Captcha
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SliderCaptchaController : ControllerBase
    {
        /// <summary>
        /// Retrieves captcha images for verify
        /// </summary>
        /// <remarks>
        /// Provide limited width and height for slider captcha images, <br />
        /// can also spicify particular direction to generate result.
        /// </remarks>
        [HttpGet(Name = "GetSliderCaptcha")]
        public SliderCaptcha_Get_ResultDto Get([FromQuery] SliderCaptcha_Get_Dto request)
        {
            var sliderCaptchaModel = SliderCaptchaTool.GetSliderCaptchaImages(request.CaptchaDirection, request.WidthAndHeight?.Width, request.WidthAndHeight?.Height);

            var result = new SliderCaptcha_Get_ResultDto()
            {
                CaptchaDirection = sliderCaptchaModel.CaptchaDirection,
                ImageWidth = sliderCaptchaModel.ImageWidth,
                ImageHeight = sliderCaptchaModel.ImageHeight,
                BackgroundBase64String = sliderCaptchaModel.BackgroundBase64String ?? string.Empty,
                SliderBase64String = sliderCaptchaModel.SliderBase64String ?? string.Empty,
                SlideOffset = sliderCaptchaModel.SlideOffset, // should remove at produce mode
            };

            return result;
        }
    }
}