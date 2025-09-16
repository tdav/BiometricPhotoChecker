using BiometricPhotoChecker;
using BiometricPhotoChecker.Service;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

public class BiometricPhotoValidationService : IBiometricPhotoValidationService
{
    private static readonly string faceFileName = Path.Combine(Directory.GetParent(System.Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades", "haarcascade_frontalface_default.xml");
    private static readonly string profileFaceFileName = Path.Combine(Directory.GetParent(System.Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades", "haarcascade_profileface.xml");
    private static readonly string smileFileName = Path.Combine(Directory.GetParent(System.Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades", "haarcascade_smile.xml");
    private static readonly string eyeFileName = Path.Combine(Directory.GetParent(System.Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades", "haarcascade_eye.xml");
    private static readonly string eyeGlassesFileName = Path.Combine(Directory.GetParent(System.Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades", "haarcascade_eye_tree_eyeglasses.xml");
    private static readonly string mouthFileName = Path.Combine(Directory.GetParent(System.Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades", "haarcascade_mcs_mouth.xml");
    private static readonly string noseFileName = Path.Combine(Directory.GetParent(System.Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades", "haarcascade_mcs_nose.xml");
    private const int MIN_WIDTH = 35;
    private const int MIN_HEIGHT = 45;

    public CheckInfo ValidateBiometricPhoto(byte[] imageBytes)
    {
        using ImageAnalysisContext context = AnalyzeImage(imageBytes);

        CheckInfo checkInfo = new CheckInfo
        {
            IsImageValid = IsImageValid(context),
            IsSmileValid = !context.HasSmile,
            IsMouthValid = context.PrimaryMouth.HasValue,
            IsNoseValid = context.PrimaryNose.HasValue,
            IsEyesValid = context.PrimaryEyes.Length >= 2,
            IsHeadAndShouldersVisible = IsHeadAndShouldersVisible(context),
            IsBackgroundValid = IsBackgroundValid(context),
            IsProperLighting = IsProperLighting(context),
            IsFacePositionValid = IsFacePositionValid(context),
            IsEyePositionValid = IsEyePositionValid(context)
        };

        bool isBlinking = IsBlinking(context);
        bool isMouthOpen = IsMouthOpen(context);
        bool isLookingSideways = IsLookingSideways(context);
        bool isFaceTurnedSideways = IsFaceTurnedSideways(context);
        bool isLookingUpOrDown = IsLookingUpOrDown(context);
        bool hasRedEye = HasRedEyeEffect(context);
        bool hasGlassesGlare = HasGlassesGlare(context);

        double faceTextureScore = ComputeFaceTextureScore(context);
        bool isFaceSkinClear = faceTextureScore > 12.0;
        bool isSkinTextureNatural = faceTextureScore > 20.0;

        double imageSharpnessScore = ComputeImageSharpnessScore(context);
        bool isImageSharp = imageSharpnessScore > 15.0;

        double averageSaturation = ComputeAverageSaturation(context);
        bool areColorsFaded = averageSaturation < 25.0;
        bool isSaturationAdequate = averageSaturation >= 30.0;

        bool isPixelated = IsPixelated(context);

        checkInfo.IsFaceDetected = context.HasFace;
        checkInfo.IsExpressionNeutral = !context.HasSmile && !isMouthOpen && !isBlinking;
        checkInfo.HasGlasses = context.HasEyeglasses;
        checkInfo.IsBlinking = isBlinking;
        checkInfo.IsMouthOpen = isMouthOpen;
        checkInfo.IsLookingSideways = isLookingSideways;
        checkInfo.HasRedEyeEffect = hasRedEye;
        checkInfo.IsFaceIlluminated = IsFaceIlluminated(context);
        checkInfo.IsFaceSkinClear = isFaceSkinClear;
        checkInfo.AreColorsFaded = areColorsFaded;
        checkInfo.IsPixelated = isPixelated;
        checkInfo.IsSkinTextureNatural = isSkinTextureNatural;
        checkInfo.HasGlassesGlare = hasGlassesGlare;
        checkInfo.IsOverExposed = IsOverExposed(context);
        checkInfo.IsFaceTurnedSideways = isFaceTurnedSideways;
        checkInfo.IsLookingUpOrDown = isLookingUpOrDown;
        checkInfo.IsTooCloseToCamera = IsTooClose(context);
        checkInfo.IsTooFarFromCamera = IsTooFar(context);
        checkInfo.IsCameraAboveFace = IsCameraAbove(context);
        checkInfo.IsCameraBelowFace = IsCameraBelow(context);
        checkInfo.IsCameraShiftedLeft = IsCameraShiftedLeft(context);
        checkInfo.IsImageSharp = isImageSharp;
        checkInfo.IsSaturationAdequate = isSaturationAdequate;
        checkInfo.IsBackgroundUniform = IsBackgroundUniform(context);

        return checkInfo;
    }

    private bool IsImageValid(ImageAnalysisContext context)
    {
        return context.Width >= MIN_WIDTH && context.Height >= MIN_HEIGHT;
    }

    private bool IsFacePositionValid(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue)
        {
            return false;
        }

        Rectangle face = context.PrimaryFace.Value;
        double faceCenterX = face.X + face.Width / 2.0;
        double faceCenterY = face.Y + face.Height / 2.0;
        double thresholdX = context.Width * 0.1;
        double thresholdY = context.Height * 0.1;

        return Math.Abs(faceCenterX - context.Width / 2.0) <= thresholdX &&
               Math.Abs(faceCenterY - context.Height / 2.0) <= thresholdY;
    }

    private bool IsEyePositionValid(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue)
        {
            return false;
        }

        Rectangle[] eyes = context.PrimaryEyes;
        if (eyes.Length < 2)
        {
            return false;
        }

        Rectangle[] orderedEyes = eyes.OrderBy(e => e.X).ToArray();
        Rectangle leftEye = orderedEyes[0];
        Rectangle rightEye = orderedEyes[orderedEyes.Length - 1];

        double leftEyeCenterY = leftEye.Y + leftEye.Height / 2.0;
        double rightEyeCenterY = rightEye.Y + rightEye.Height / 2.0;
        double thresholdY = context.Height * 0.05;

        if (Math.Abs(leftEyeCenterY - rightEyeCenterY) > thresholdY)
        {
            return false;
        }

        double faceWidth = context.PrimaryFace.Value.Width;
        double eyeDistance = Math.Abs((rightEye.X + rightEye.Width / 2.0) - (leftEye.X + leftEye.Width / 2.0));

        return eyeDistance >= faceWidth * 0.3;
    }

    private bool IsHeadAndShouldersVisible(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue)
        {
            return false;
        }

        Rectangle face = context.PrimaryFace.Value;
        double marginX = Math.Min(face.X, context.Width - face.Right);
        double marginY = Math.Min(face.Y, context.Height - face.Bottom);

        return marginX > context.Width * 0.05 && marginY > context.Height * 0.05;
    }

    private bool IsBackgroundValid(ImageAnalysisContext context)
    {
        if (context.ColorBitmap == null)
        {
            return false;
        }

        int rectWidth = Math.Max(5, context.Width / 10);
        int rectHeight = Math.Max(5, context.Height / 10);

        Rectangle topLeft = new Rectangle(5, 5, rectWidth, rectHeight);
        Rectangle topRight = new Rectangle(context.Width - rectWidth - 5, 5, rectWidth, rectHeight);

        Color colorTopLeft = CalculateAverageColor(context.ColorBitmap, topLeft);
        Color colorTopRight = CalculateAverageColor(context.ColorBitmap, topRight);

        Color average = Color.FromArgb((colorTopLeft.R + colorTopRight.R) / 2, (colorTopLeft.G + colorTopRight.G) / 2, (colorTopLeft.B + colorTopRight.B) / 2);

        return average.R >= 200 && average.G >= 200 && average.B >= 200;
    }

    private bool IsBackgroundUniform(ImageAnalysisContext context)
    {
        if (context.ColorBitmap == null)
        {
            return false;
        }

        int rectWidth = Math.Max(5, context.Width / 12);
        int rectHeight = Math.Max(5, context.Height / 12);

        Rectangle topLeft = new Rectangle(5, 5, rectWidth, rectHeight);
        Rectangle topRight = new Rectangle(context.Width - rectWidth - 5, 5, rectWidth, rectHeight);
        Rectangle bottomLeft = new Rectangle(5, context.Height - rectHeight - 5, rectWidth, rectHeight);
        Rectangle bottomRight = new Rectangle(context.Width - rectWidth - 5, context.Height - rectHeight - 5, rectWidth, rectHeight);

        List<Color> samples = new List<Color>
        {
            CalculateAverageColor(context.ColorBitmap, topLeft),
            CalculateAverageColor(context.ColorBitmap, topRight),
            CalculateAverageColor(context.ColorBitmap, bottomLeft),
            CalculateAverageColor(context.ColorBitmap, bottomRight)
        };

        double averageDistance = 0.0;
        int comparisons = 0;
        for (int i = 0; i < samples.Count; i++)
        {
            for (int j = i + 1; j < samples.Count; j++)
            {
                averageDistance += GetColorDistance(samples[i], samples[j]);
                comparisons++;
            }
        }

        if (comparisons == 0)
        {
            return false;
        }

        averageDistance /= comparisons;
        return averageDistance < 25.0;
    }

    private bool IsProperLighting(ImageAnalysisContext context)
    {
        if (context.ColorBitmap == null)
        {
            return false;
        }

        double brightness = GetAverageBrightness(context.ColorBitmap, new Rectangle(0, 0, context.Width, context.Height));
        return brightness >= 80 && brightness <= 240;
    }

    private bool IsFaceIlluminated(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue || context.ColorBitmap == null)
        {
            return false;
        }

        Rectangle face = context.PrimaryFace.Value;
        double brightness = GetAverageBrightness(context.ColorBitmap, face);
        return brightness >= 90;
    }

    private bool IsOverExposed(ImageAnalysisContext context)
    {
        if (context.ColorBitmap == null)
        {
            return false;
        }

        double brightness = GetAverageBrightness(context.ColorBitmap, new Rectangle(0, 0, context.Width, context.Height));
        return brightness > 230;
    }

    private bool IsBlinking(ImageAnalysisContext context)
    {
        Rectangle[] eyes = context.PrimaryEyes;
        if (eyes.Length < 2)
        {
            return false;
        }

        double ratioSum = 0.0;
        foreach (Rectangle eye in eyes)
        {
            if (eye.Width == 0)
            {
                continue;
            }

            ratioSum += (double)eye.Height / eye.Width;
        }

        double averageRatio = ratioSum / eyes.Length;
        return averageRatio < 0.22;
    }

    private bool IsMouthOpen(ImageAnalysisContext context)
    {
        if (!context.PrimaryMouth.HasValue)
        {
            return false;
        }

        Rectangle mouth = context.PrimaryMouth.Value;
        if (mouth.Width == 0)
        {
            return false;
        }

        double ratio = (double)mouth.Height / mouth.Width;
        return ratio > 0.35;
    }

    private bool IsLookingSideways(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue)
        {
            return false;
        }

        if (!context.PrimaryNose.HasValue)
        {
            return context.ProfileFaces.Length > 0;
        }

        Rectangle face = context.PrimaryFace.Value;
        Rectangle nose = context.PrimaryNose.Value;
        double faceCenterX = face.X + face.Width / 2.0;
        double noseCenterX = nose.X + nose.Width / 2.0;
        return Math.Abs(noseCenterX - faceCenterX) > face.Width * 0.15;
    }

    private bool IsFaceTurnedSideways(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue)
        {
            return false;
        }

        Rectangle[] eyes = context.PrimaryEyes;
        if (eyes.Length < 2)
        {
            return context.ProfileFaces.Length > 0;
        }

        Rectangle face = context.PrimaryFace.Value;
        Rectangle[] orderedEyes = eyes.OrderBy(e => e.X).ToArray();
        Rectangle leftEye = orderedEyes[0];
        Rectangle rightEye = orderedEyes[orderedEyes.Length - 1];

        double leftDistance = (leftEye.X + leftEye.Width / 2.0) - face.X;
        double rightDistance = face.Right - (rightEye.X + rightEye.Width / 2.0);
        return Math.Abs(leftDistance - rightDistance) > face.Width * 0.2 || context.ProfileFaces.Length > 0;
    }

    private bool IsLookingUpOrDown(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue)
        {
            return false;
        }

        Rectangle[] eyes = context.PrimaryEyes;
        if (eyes.Length < 2)
        {
            return false;
        }

        Rectangle face = context.PrimaryFace.Value;
        double eyesCenterY = eyes.Average(e => e.Y + e.Height / 2.0);
        double faceCenterY = face.Y + face.Height / 2.0;
        return Math.Abs(eyesCenterY - faceCenterY) > face.Height * 0.15;
    }

    private bool HasRedEyeEffect(ImageAnalysisContext context)
    {
        if (context.ColorBitmap == null)
        {
            return false;
        }

        Rectangle[] eyes = context.PrimaryEyes;
        if (eyes.Length == 0)
        {
            return false;
        }

        int totalPixels = 0;
        int redPixels = 0;

        foreach (Rectangle eye in eyes)
        {
            Rectangle region = ClampRectangle(eye, context.ImageSize);
            for (int x = region.X; x < region.Right; x++)
            {
                for (int y = region.Y; y < region.Bottom; y++)
                {
                    Color pixel = context.ColorBitmap.GetPixel(x, y);
                    if (pixel.R > 150 && pixel.R > pixel.G + 30 && pixel.R > pixel.B + 30)
                    {
                        redPixels++;
                    }
                    totalPixels++;
                }
            }
        }

        if (totalPixels == 0)
        {
            return false;
        }

        double ratio = (double)redPixels / totalPixels;
        return ratio > 0.02;
    }

    private bool HasGlassesGlare(ImageAnalysisContext context)
    {
        if (!context.HasEyeglasses || context.ColorBitmap == null)
        {
            return false;
        }

        int brightPixels = 0;
        int totalPixels = 0;

        foreach (Rectangle regionRect in context.EyeglassDetections)
        {
            Rectangle region = ClampRectangle(regionRect, context.ImageSize);
            for (int x = region.X; x < region.Right; x++)
            {
                for (int y = region.Y; y < region.Bottom; y++)
                {
                    Color pixel = context.ColorBitmap.GetPixel(x, y);
                    if (GetBrightness(pixel) > 230)
                    {
                        brightPixels++;
                    }
                    totalPixels++;
                }
            }
        }

        if (totalPixels == 0)
        {
            return false;
        }

        double ratio = (double)brightPixels / totalPixels;
        return ratio > 0.05;
    }

    private bool IsTooClose(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue)
        {
            return false;
        }

        Rectangle face = context.PrimaryFace.Value;
        double heightRatio = (double)face.Height / context.Height;
        return heightRatio > 0.7;
    }

    private bool IsTooFar(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue)
        {
            return false;
        }

        Rectangle face = context.PrimaryFace.Value;
        double heightRatio = (double)face.Height / context.Height;
        return heightRatio < 0.3;
    }

    private bool IsCameraAbove(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue)
        {
            return false;
        }

        Rectangle face = context.PrimaryFace.Value;
        double faceCenterY = face.Y + face.Height / 2.0;
        return faceCenterY < context.Height * 0.35;
    }

    private bool IsCameraBelow(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue)
        {
            return false;
        }

        Rectangle face = context.PrimaryFace.Value;
        double faceCenterY = face.Y + face.Height / 2.0;
        return faceCenterY > context.Height * 0.65;
    }

    private bool IsCameraShiftedLeft(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue)
        {
            return false;
        }

        Rectangle face = context.PrimaryFace.Value;
        double faceCenterX = face.X + face.Width / 2.0;
        return faceCenterX < context.Width * 0.4;
    }

    private double ComputeFaceTextureScore(ImageAnalysisContext context)
    {
        if (!context.PrimaryFace.HasValue || context.ColorBitmap == null)
        {
            return 0.0;
        }

        Rectangle face = ClampRectangle(context.PrimaryFace.Value, context.ImageSize);
        return ComputeGradientVariance(context.ColorBitmap, face);
    }

    private double ComputeImageSharpnessScore(ImageAnalysisContext context)
    {
        if (context.ColorBitmap == null)
        {
            return 0.0;
        }

        Rectangle full = new Rectangle(0, 0, context.Width, context.Height);
        return ComputeGradientVariance(context.ColorBitmap, full);
    }

    private double ComputeAverageSaturation(ImageAnalysisContext context)
    {
        if (context.ColorBitmap == null)
        {
            return 0.0;
        }

        Rectangle region = new Rectangle(0, 0, context.Width, context.Height);
        double saturationSum = 0.0;
        int count = 0;

        for (int x = region.X; x < region.Right; x++)
        {
            for (int y = region.Y; y < region.Bottom; y++)
            {
                Color pixel = context.ColorBitmap.GetPixel(x, y);
                double saturation = GetSaturation(pixel);
                saturationSum += saturation;
                count++;
            }
        }

        if (count == 0)
        {
            return 0.0;
        }

        return (saturationSum / count) * 100.0;
    }

    private bool IsPixelated(ImageAnalysisContext context)
    {
        if (context.ColorMat == null || context.ColorMat.IsEmpty)
        {
            return false;
        }

        int targetWidth = Math.Max(1, context.Width / 4);
        int targetHeight = Math.Max(1, context.Height / 4);

        using Mat small = new Mat();
        using Mat enlarged = new Mat();
        using Mat diff = new Mat();

        CvInvoke.Resize(context.ColorMat, small, new Size(targetWidth, targetHeight), 0, 0, Inter.Linear);
        CvInvoke.Resize(small, enlarged, new Size(context.Width, context.Height), 0, 0, Inter.Nearest);
        CvInvoke.AbsDiff(context.ColorMat, enlarged, diff);

        MCvScalar mean = CvInvoke.Mean(diff);
        double meanValue = (mean.V0 + mean.V1 + mean.V2) / 3.0;
        return meanValue < 10.0;
    }

    private double ComputeGradientVariance(Bitmap bitmap, Rectangle region)
    {
        Rectangle rect = ClampRectangle(region, new Size(bitmap.Width, bitmap.Height));
        if (rect.Width < 3 || rect.Height < 3)
        {
            return 0.0;
        }

        double sum = 0.0;
        double sumSq = 0.0;
        int count = 0;

        for (int x = rect.X + 1; x < rect.Right - 1; x++)
        {
            for (int y = rect.Y + 1; y < rect.Bottom - 1; y++)
            {
                double left = GetBrightness(bitmap.GetPixel(x - 1, y));
                double right = GetBrightness(bitmap.GetPixel(x + 1, y));
                double up = GetBrightness(bitmap.GetPixel(x, y - 1));
                double down = GetBrightness(bitmap.GetPixel(x, y + 1));

                double diffX = right - left;
                double diffY = down - up;
                double magnitude = Math.Sqrt(diffX * diffX + diffY * diffY);

                sum += magnitude;
                sumSq += magnitude * magnitude;
                count++;
            }
        }

        if (count == 0)
        {
            return 0.0;
        }

        double mean = sum / count;
        return sumSq / count - mean * mean;
    }

    private double GetAverageBrightness(Bitmap bitmap, Rectangle region)
    {
        Rectangle rect = ClampRectangle(region, new Size(bitmap.Width, bitmap.Height));
        if (rect.Width == 0 || rect.Height == 0)
        {
            return 0.0;
        }

        double total = 0.0;
        int count = 0;

        for (int x = rect.X; x < rect.Right; x++)
        {
            for (int y = rect.Y; y < rect.Bottom; y++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                total += GetBrightness(pixel);
                count++;
            }
        }

        if (count == 0)
        {
            return 0.0;
        }

        return total / count;
    }

    private static double GetBrightness(Color color)
    {
        return 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
    }

    private static double GetSaturation(Color color)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));

        if (max == 0)
        {
            return 0.0;
        }

        return (max - min) / max;
    }

    private Color CalculateAverageColor(Bitmap bitmap, Rectangle rect)
    {
        Rectangle region = ClampRectangle(rect, new Size(bitmap.Width, bitmap.Height));
        double r = 0.0;
        double g = 0.0;
        double b = 0.0;
        int count = 0;

        for (int x = region.X; x < region.Right; x++)
        {
            for (int y = region.Y; y < region.Bottom; y++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                r += pixel.R;
                g += pixel.G;
                b += pixel.B;
                count++;
            }
        }

        if (count == 0)
        {
            return Color.Black;
        }

        return Color.FromArgb((int)(r / count), (int)(g / count), (int)(b / count));
    }

    private static double GetColorDistance(Color c1, Color c2)
    {
        double dr = c1.R - c2.R;
        double dg = c1.G - c2.G;
        double db = c1.B - c2.B;
        return Math.Sqrt(dr * dr + dg * dg + db * db);
    }

    private ImageAnalysisContext AnalyzeImage(byte[] imageBytes)
    {
        Mat colorMat = LoadImage(imageBytes);
        if (colorMat.IsEmpty)
        {
            throw new ArgumentException("Image data could not be decoded.");
        }

        Mat grayMat = new Mat();
        CvInvoke.CvtColor(colorMat, grayMat, ColorConversion.Bgr2Gray);

        Rectangle[] faces = DetectFaces(grayMat);
        Rectangle? primaryFace = faces.Length > 0 ? faces.OrderByDescending(r => r.Width * r.Height).First() : (Rectangle?)null;

        Rectangle[] profileFaces = DetectProfileFaces(grayMat);

        Rectangle[] eyes = primaryFace.HasValue ? DetectEyes(grayMat, primaryFace.Value) : Array.Empty<Rectangle>();
        Rectangle[] primaryEyes = SelectPrimaryEyes(eyes);

        Rectangle[] mouths = primaryFace.HasValue ? DetectMouths(grayMat, primaryFace.Value) : Array.Empty<Rectangle>();
        Rectangle? primaryMouth = mouths.Length > 0 ? mouths.OrderByDescending(r => r.Width * r.Height).First() : (Rectangle?)null;

        Rectangle[] noses = primaryFace.HasValue ? DetectNoses(grayMat, primaryFace.Value) : Array.Empty<Rectangle>();
        Rectangle? primaryNose = noses.Length > 0 ? noses.OrderByDescending(r => r.Width * r.Height).First() : (Rectangle?)null;

        bool hasSmile = primaryFace.HasValue && DetectSmile(grayMat, primaryFace.Value);

        Rectangle[] eyeglassDetections = primaryFace.HasValue ? DetectEyeglasses(grayMat, primaryFace.Value) : Array.Empty<Rectangle>();

        Bitmap bitmap = colorMat.ToBitmap();

        return new ImageAnalysisContext(colorMat, grayMat, bitmap, faces, profileFaces, eyes, primaryEyes, mouths, noses, primaryFace, primaryMouth, primaryNose, eyeglassDetections, hasSmile);
    }

    private Rectangle[] DetectFaces(Mat grayMat)
    {
        using CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName);
        return faceCascade.DetectMultiScale(grayMat, 1.1, 4, Size.Empty, Size.Empty);
    }

    private Rectangle[] DetectProfileFaces(Mat grayMat)
    {
        using CascadeClassifier profileCascade = LoadCascadeClassifier(profileFaceFileName);
        Rectangle[] direct = profileCascade.DetectMultiScale(grayMat, 1.1, 3, Size.Empty, Size.Empty);

        using Mat flipped = new Mat();
        CvInvoke.Flip(grayMat, flipped, FlipType.Horizontal);
        Rectangle[] flippedDetections = profileCascade.DetectMultiScale(flipped, 1.1, 3, Size.Empty, Size.Empty);

        Rectangle[] mirrored = flippedDetections
            .Select(r => new Rectangle(grayMat.Width - r.Right, r.Y, r.Width, r.Height))
            .ToArray();

        return direct.Concat(mirrored).ToArray();
    }

    private Rectangle[] DetectEyes(Mat grayMat, Rectangle face)
    {
        Rectangle eyeRegion = new Rectangle(face.X, face.Y, face.Width, Math.Max(1, face.Height / 2));
        return DetectFeature(grayMat, eyeFileName, eyeRegion, 1.1, 4);
    }

    private Rectangle[] SelectPrimaryEyes(Rectangle[] eyes)
    {
        if (eyes.Length <= 2)
        {
            return eyes;
        }

        Rectangle[] candidates = eyes
            .OrderByDescending(r => r.Width * r.Height)
            .Take(4)
            .OrderBy(r => r.X)
            .ToArray();

        if (candidates.Length <= 2)
        {
            return candidates;
        }

        return new Rectangle[] { candidates.First(), candidates.Last() };
    }

    private Rectangle[] DetectMouths(Mat grayMat, Rectangle face)
    {
        Rectangle mouthRegion = new Rectangle(face.X, face.Y + face.Height / 2, face.Width, Math.Max(1, face.Height / 2));
        return DetectFeature(grayMat, mouthFileName, mouthRegion, 1.1, 10);
    }

    private Rectangle[] DetectNoses(Mat grayMat, Rectangle face)
    {
        int offsetY = face.Height / 4;
        Rectangle noseRegion = new Rectangle(face.X, face.Y + offsetY, face.Width, Math.Max(1, face.Height / 2));
        return DetectFeature(grayMat, noseFileName, noseRegion, 1.1, 4);
    }

    private Rectangle[] DetectEyeglasses(Mat grayMat, Rectangle face)
    {
        Rectangle glassesRegion = new Rectangle(face.X, face.Y, face.Width, Math.Max(1, face.Height / 2));
        return DetectFeature(grayMat, eyeGlassesFileName, glassesRegion, 1.1, 4);
    }

    private bool DetectSmile(Mat grayMat, Rectangle face)
    {
        using CascadeClassifier smileCascade = LoadCascadeClassifier(smileFileName);
        using Mat faceRegion = new Mat(grayMat, ClampRectangle(face, new Size(grayMat.Width, grayMat.Height)));
        Rectangle[] smiles = smileCascade.DetectMultiScale(faceRegion, 1.5, 20, Size.Empty, Size.Empty);
        return smiles.Length > 0;
    }

    private Rectangle[] DetectFeature(Mat grayMat, string cascadePath, Rectangle region, double scaleFactor, int minNeighbors)
    {
        Rectangle boundedRegion = ClampRectangle(region, new Size(grayMat.Width, grayMat.Height));
        if (boundedRegion.Width <= 0 || boundedRegion.Height <= 0)
        {
            return Array.Empty<Rectangle>();
        }

        using CascadeClassifier cascade = LoadCascadeClassifier(cascadePath);
        using Mat roi = new Mat(grayMat, boundedRegion);
        Rectangle[] detections = cascade.DetectMultiScale(roi, scaleFactor, minNeighbors, Size.Empty, Size.Empty);

        for (int i = 0; i < detections.Length; i++)
        {
            detections[i].Offset(boundedRegion.Location);
        }

        return detections;
    }

    private static Rectangle ClampRectangle(Rectangle rect, Size bounds)
    {
        int x = Math.Max(0, rect.X);
        int y = Math.Max(0, rect.Y);
        int right = Math.Min(bounds.Width, rect.Right);
        int bottom = Math.Min(bounds.Height, rect.Bottom);
        int width = Math.Max(0, right - x);
        int height = Math.Max(0, bottom - y);
        return new Rectangle(x, y, width, height);
    }

    private Mat LoadImage(byte[] imageBytes)
    {
        Mat image = new Mat();
        CvInvoke.Imdecode(imageBytes, ImreadModes.Color, image);
        return image;
    }

    private CascadeClassifier LoadCascadeClassifier(string fileName)
    {
        return new CascadeClassifier(fileName);
    }

    private sealed class ImageAnalysisContext : IDisposable
    {
        public ImageAnalysisContext(Mat colorMat, Mat grayMat, Bitmap bitmap, Rectangle[] faces, Rectangle[] profileFaces, Rectangle[] eyes, Rectangle[] primaryEyes, Rectangle[] mouths, Rectangle[] noses, Rectangle? primaryFace, Rectangle? primaryMouth, Rectangle? primaryNose, Rectangle[] eyeglassDetections, bool hasSmile)
        {
            ColorMat = colorMat;
            GrayMat = grayMat;
            ColorBitmap = bitmap;
            Faces = faces;
            ProfileFaces = profileFaces;
            Eyes = eyes;
            PrimaryEyes = primaryEyes;
            Mouths = mouths;
            Noses = noses;
            PrimaryFace = primaryFace;
            PrimaryMouth = primaryMouth;
            PrimaryNose = primaryNose;
            EyeglassDetections = eyeglassDetections;
            HasSmile = hasSmile;
        }

        public Mat ColorMat { get; }
        public Mat GrayMat { get; }
        public Bitmap ColorBitmap { get; }
        public Rectangle[] Faces { get; }
        public Rectangle[] ProfileFaces { get; }
        public Rectangle[] Eyes { get; }
        public Rectangle[] PrimaryEyes { get; }
        public Rectangle[] Mouths { get; }
        public Rectangle[] Noses { get; }
        public Rectangle? PrimaryFace { get; }
        public Rectangle? PrimaryMouth { get; }
        public Rectangle? PrimaryNose { get; }
        public Rectangle[] EyeglassDetections { get; }
        public bool HasSmile { get; }
        public bool HasFace => PrimaryFace.HasValue;
        public bool HasEyeglasses => EyeglassDetections.Length > 0;
        public int Width => ColorMat.Width;
        public int Height => ColorMat.Height;
        public Size ImageSize => new Size(Width, Height);

        public void Dispose()
        {
            ColorMat.Dispose();
            GrayMat.Dispose();
            ColorBitmap.Dispose();
        }
    }
}
