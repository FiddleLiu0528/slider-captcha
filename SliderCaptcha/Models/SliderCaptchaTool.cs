using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace SliderCaptcha.Models
{
    /// <summary>
    /// Slider Captcha Tool
    /// </summary>
    public static class SliderCaptchaTool
    {
        private static string ImageSourcePath(int num) => $"./Resources/SliderCaptchaImages/{num}.jpg";

        private readonly static int bgTotalAmount = 136;

        private const int defaultWidth = 280;
        private const int defaultHeight = 150;

        /// <summary>
        /// Generate Random bool
        /// </summary>
        private static bool RandomNext()
        {
            Random random = new(Guid.NewGuid().GetHashCode());
            return random.Next(100) <= 50;
        }

        /// <summary>
        /// Generate Random int
        /// </summary>
        private static int RandomNext(int min, int max)
        {
            Random random = new(Guid.NewGuid().GetHashCode());
            return random.Next(min, max + 1);
        }

        /// <summary>
        /// Generate Random float
        /// </summary>
        private static float RandomNext(float min, float max)
        {
            Random random = new(Guid.NewGuid().GetHashCode());
            return random.NextSingle() * (max - min) + min;
        }

        /// <summary>
        /// Calculate Path
        /// </summary>
        /// <param name="minLength"></param>
        /// <param name="isDriftSquare"></param>
        /// <param name="isRotate"></param>
        /// <remarks>
        /// Slider size is about half of image's min length 
        /// </remarks>
        /// <returns></returns>
        private static IPath GenerateSliderPath(int minLength, bool isDriftSquare = true, bool isRotate = true)
        {
            // slider can't bigger then 1/2 of image min length
            // slider consist with square and max 4 bulge that extend from square
            // square plus two side bulge can't bigger then 1/2 of image min length
            // the stander jigsaw shape square consist near to 9 bulge area
            // each dirction is about 2 bulge length
            // so, make sure slider don't bigger then 3 + 2 bulge length

            var bulgeUnit = minLength / 2 / 5;
            var sliderSquareSegmentUnit = bulgeUnit * 3 / 2;
            var sliderBulgeGapSegmentUnit = sliderSquareSegmentUnit / 4;
            float sliderRuggedRadiusUnit = bulgeUnit / 2;

            var startPointX = 0;
            var startPointY = 0;

            var x = () => isDriftSquare ? (int)(startPointX * RandomNext(0.95f, 1)) : startPointX;
            var y = () => isDriftSquare ? (int)(startPointY * RandomNext(0.95f, 1)) : startPointY;
            var ruggedRadius = () => isDriftSquare ? (int)(sliderRuggedRadiusUnit * RandomNext(1f, 1.3f)) : sliderRuggedRadiusUnit;

            Point PTop1 = new(x(), y());
            Point PTop2 = new(x() + sliderSquareSegmentUnit - sliderBulgeGapSegmentUnit, y());
            Point PTop3 = new(x() + sliderSquareSegmentUnit + sliderBulgeGapSegmentUnit, y());
            Point PTop4 = new(x() + sliderSquareSegmentUnit * 2, y());

            Point PRight2 = new(x() + sliderSquareSegmentUnit * 2, y() + sliderSquareSegmentUnit - sliderBulgeGapSegmentUnit);
            Point PRight3 = new(x() + sliderSquareSegmentUnit * 2, y() + sliderSquareSegmentUnit + sliderBulgeGapSegmentUnit);
            Point PRight4 = new(x() + sliderSquareSegmentUnit * 2, y() + sliderSquareSegmentUnit * 2);

            Point PBottom2 = new(x() + sliderSquareSegmentUnit + sliderBulgeGapSegmentUnit, y() + sliderSquareSegmentUnit * 2);
            Point PBottom3 = new(x() + sliderSquareSegmentUnit - sliderBulgeGapSegmentUnit, y() + sliderSquareSegmentUnit * 2);
            Point PBottom4 = new(x(), y() + sliderSquareSegmentUnit * 2);

            Point PLeft2 = new(x(), y() + sliderSquareSegmentUnit + sliderBulgeGapSegmentUnit);
            Point PLeft3 = new(x(), y() + sliderSquareSegmentUnit - sliderBulgeGapSegmentUnit);
            Point PLeft4 = new(x(), y());

            var path = new PathBuilder();

            var shapeList = new List<Shape>() {
               (Shape)RandomNext(1,2),
               (Shape)RandomNext(1,2),
               (Shape)RandomNext(0,2),
               (Shape)RandomNext(0,2),
            }.OrderBy(a => new Random().Next())
            .ToList();

            //at least one bulge or concave, and no 3 side linear, and no linear on oppsite side
            if ((shapeList[0] == shapeList[2] && shapeList[0] == Shape.linear) ||
                (shapeList[1] == shapeList[3] && shapeList[1] == Shape.linear))
            {
                (shapeList[1], shapeList[0]) = (shapeList[0], shapeList[1]);
            }

            // first segment is linear
            for (int i = 0; i < shapeList.Count(); i++)
            {
                if (shapeList[i] == Shape.linear)
                {
                    if (i == 0)
                    {
                        path.AddLine(PTop1, PTop4);
                        continue;
                    }
                    else if (i == 1)
                    {
                        path.LineTo(PRight4);
                        continue;
                    }
                    else if (i == 2)
                    {
                        path.LineTo(PBottom4);
                        continue;
                    }
                }

                if (shapeList[i] == Shape.concave)
                {
                    if (i == 0)
                    {
                        path.AddLine(PTop1, PTop2)
                            .LineTo(PTop2)
                            .ArcTo(ruggedRadius(), ruggedRadius(), 0, true, true, PTop3)
                            .LineTo(PTop4);
                        continue;
                    }
                    else if (i == 1)
                    {
                        path.LineTo(PRight2)
                            .ArcTo(ruggedRadius(), ruggedRadius(), 0, true, true, PRight3)
                            .LineTo(PRight4);
                        continue;
                    }
                    else if (i == 2)
                    {
                        path.LineTo(PBottom2)
                            .ArcTo(ruggedRadius(), ruggedRadius(), 0, true, true, PBottom3)
                            .LineTo(PBottom4);
                        continue;
                    }
                    else
                    {
                        path.LineTo(PLeft2)
                            .ArcTo(ruggedRadius(), ruggedRadius(), 0, true, true, PLeft3);
                        continue;
                    }
                }

                if (shapeList[i] == Shape.bulge)
                {
                    if (i == 0)
                    {
                        path.AddLine(PTop1, PTop2)
                            .LineTo(PTop2)
                            .ArcTo(ruggedRadius(), ruggedRadius(), 0, true, false, PTop3)
                            .LineTo(PTop4);
                        continue;
                    }
                    else if (i == 1)
                    {
                        path.LineTo(PRight2)
                            .ArcTo(ruggedRadius(), ruggedRadius(), 0, true, false, PRight3)
                            .LineTo(PRight4);
                        continue;
                    }
                    else if (i == 2)
                    {
                        path.LineTo(PBottom2)
                            .ArcTo(ruggedRadius(), ruggedRadius(), 0, true, false, PBottom3)
                            .LineTo(PBottom4);
                        continue;
                    }
                    else
                    {
                        path.LineTo(PLeft2)
                            .ArcTo(ruggedRadius(), ruggedRadius(), 0, true, false, PLeft3);
                        continue;
                    }
                }
            }

            var resultPath = path.CloseFigure().Build();
            if (isRotate && RandomNext()) resultPath = resultPath.Rotate(RandomNext(0, 360));

            return resultPath;
        }

        /// <summary>
        /// Generate Slider Captcha Images
        /// </summary>
        /// <param name="img"></param>
        /// <param name="direction"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static (string backgroundBase64String, string sliderBase64String, int? slideOffset) GenerateSliderCaptchaImages(Image<Rgb24> img, CaptchaDirection direction, int width, int height)
        {
            try
            {
                img.Mutate(m => m.Resize(new ResizeOptions { Mode = ResizeMode.Crop, Size = new Size(width, height) }));

                var minLength = width < height ? width : height;

                var sliderPath = GenerateSliderPath(minLength);

                var pathX = sliderPath.Bounds.X;
                var pathY = sliderPath.Bounds.Y;
                var pathWidth = sliderPath.Bounds.Width;
                var pathHeight = sliderPath.Bounds.Height;

                // reset slider to start point (left-top)
                sliderPath = sliderPath.Translate(-pathX, -pathY);

                float placePathMinWidth = 0f;
                float placePathMaxWidth = 0f;

                float placePathMinHeight = 0f;
                float placePathMaxHeight = 0f;

                if (direction is CaptchaDirection.LeftToRight)
                {
                    placePathMinWidth = pathWidth / 2;
                    placePathMaxWidth = width - pathWidth;

                    placePathMinHeight = 0;
                    placePathMaxHeight = height - pathHeight;
                }
                else if (direction is CaptchaDirection.RightToLeft)
                {
                    placePathMinWidth = 0;
                    placePathMaxWidth = width - pathWidth - (pathWidth / 2);

                    placePathMinHeight = 0;
                    placePathMaxHeight = height - pathHeight;
                }
                else if (direction is CaptchaDirection.TopToBottom)
                {
                    placePathMinWidth = 0;
                    placePathMaxWidth = width - pathWidth;

                    placePathMinHeight = pathHeight / 2;
                    placePathMaxHeight = height - pathHeight;
                }
                else if (direction is CaptchaDirection.BottomToTop)
                {
                    placePathMinWidth = 0;
                    placePathMaxWidth = width - pathWidth;

                    placePathMinHeight = 0;
                    placePathMaxHeight = height - pathHeight - (pathHeight / 2);
                }

                // require use answer correct slide length
                var placeAtX = (int)RandomNext(placePathMinWidth, placePathMaxWidth); // start at half of jigsaw width
                var placeAtY = (int)RandomNext(placePathMinHeight, placePathMaxHeight);

                sliderPath = sliderPath.Translate(placeAtX, placeAtY);

                using var captchaBackground = img.CloneAs<Rgba32>();
                captchaBackground.Mutate(m => m.Fill(Color.FromRgba(255, 255, 255, 125), sliderPath).Draw(Color.FromRgba(255, 255, 255, 200), 2f, sliderPath));

                using var cloneImageForBrush = img.CloneAs<Rgba32>();
                cloneImageForBrush.Mutate(m => m
                .Draw(Color.FromRgba(255, 255, 255, 125), 4f, sliderPath)
                .Crop(new Rectangle(
                    (int)sliderPath.Bounds.X,
                    (int)sliderPath.Bounds.Y,
                    img.Width - (int)sliderPath.Bounds.X,
                    img.Height - (int)sliderPath.Bounds.Y)));
                var sliderImageBrush = new ImageBrush(cloneImageForBrush);

                using var captchaSlider = new Image<Rgba32>(width, height);



                captchaSlider.Mutate(m => m.Fill(sliderImageBrush, sliderPath)
                    .Crop(new Rectangle(
                        direction is CaptchaDirection.TopToBottom || direction is CaptchaDirection.BottomToTop ? 0 : placeAtX,
                        direction is CaptchaDirection.LeftToRight || direction is CaptchaDirection.RightToLeft ? 0 : placeAtY,
                        direction is CaptchaDirection.TopToBottom || direction is CaptchaDirection.BottomToTop ? width : Convert.ToInt16(Math.Ceiling(sliderPath.Bounds.Width)),
                        direction is CaptchaDirection.LeftToRight || direction is CaptchaDirection.RightToLeft ? height : Convert.ToInt16(Math.Ceiling(sliderPath.Bounds.Height)))
                    ));

                int? slideOffset = null;
                if (direction is CaptchaDirection.LeftToRight) { slideOffset = placeAtX; }
                else if (direction is CaptchaDirection.RightToLeft) { slideOffset = (int)(width - (pathWidth / 2) - placeAtX); }
                else if (direction is CaptchaDirection.TopToBottom) { slideOffset = placeAtY; }
                else if (direction is CaptchaDirection.BottomToTop) { slideOffset = (int)(height - (pathHeight / 2) - placeAtY); }

                return (captchaBackground.ToBase64String(PngFormat.Instance), captchaSlider.ToBase64String(PngFormat.Instance), slideOffset);
            }
            catch (Exception ex)
            {
                return (string.Empty, string.Empty, null);

                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Get Slider Captcha Images
        /// </summary>
        public static SliderCaptchaModel GetSliderCaptchaImages(CaptchaDirection? direction = CaptchaDirection.LeftToRight, int? width = defaultWidth, int? height = defaultHeight)
        {
            var d = direction ?? CaptchaDirection.LeftToRight;
            var w = width ?? defaultWidth;
            var h = height ?? defaultHeight;

            // Randomly get an image from Resources
            var randomPicNumber = RandomNext(0, bgTotalAmount);
            using var image = (Image<Rgb24>)Image.Load(ImageSourcePath(randomPicNumber));

            var (backgroundBase64String, sliderBase64String, slideOffset) = GenerateSliderCaptchaImages(image, d, w, h);

            return new SliderCaptchaModel()
            {
                CaptchaDirection = d,
                ImageWidth = w,
                ImageHeight = h,
                BackgroundBase64String = backgroundBase64String,
                SliderBase64String = sliderBase64String,
                SlideOffset = slideOffset
            };
        }
    }

    /// <summary>
    /// SliderCaptchaModel
    /// </summary>
    public class SliderCaptchaModel
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
        public string SliderBase64String { get; set; }= string.Empty;

        /// <summary>
        /// Slide Offset
        /// </summary>
        public int? SlideOffset { get; set; }
    }

    /// <summary>
    /// Captcha Direction
    /// </summary>
    public enum CaptchaDirection
    {
        /// <summary>
        /// Left to right (Landscape)
        /// </summary>
        LeftToRight,
        /// <summary>
        /// Right to left(Landscape)
        /// </summary>
        RightToLeft,
        /// <summary>
        /// Top to bottom(Portrait)
        /// </summary>
        TopToBottom,
        /// <summary>
        /// Bottom to Top(Portrait)
        /// </summary>
        BottomToTop
    }

    /// <summary>
    /// Jigsaw side shape
    /// </summary>
    enum Shape
    {
        linear,
        bulge,
        concave,
    }
}
