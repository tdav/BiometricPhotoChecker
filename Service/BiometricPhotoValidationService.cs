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
    private static readonly string faceFileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName, "haarcascades", "haarcascade_frontalface_default.xml");
    private static readonly string smileFileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName, "haarcascades", "haarcascade_smile.xml");
    private static readonly string eyeFileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName, "haarcascades", "haarcascade_eye.xml");
    private static readonly string mouthFileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName, "haarcascades", "haarcascade_mcs_mouth.xml");
    private static readonly string noseFileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName, "haarcascades", "haarcascade_mcs_nose.xml");

    private const int MIN_WIDTH = 35;
    private const int MIN_HEIGHT = 45;

    public CheckInfo ValidateBiometricPhoto(byte[] imageBytes)
    {
        CheckInfo checkInfo = new();

        using Mat colorImage = LoadImage(imageBytes);
        if (colorImage.IsEmpty)
        {
            return checkInfo;
        }

        checkInfo.IsImageValid = IsImageValid(colorImage);

        using Mat grayImage = new();
        CvInvoke.CvtColor(colorImage, grayImage, ColorConversion.Bgr2Gray);
        CvInvoke.EqualizeHist(grayImage, grayImage);

        Rectangle[] faces;
        using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
        {
            faces = faceCascade.DetectMultiScale(
                grayImage,
                1.1,
                6,
                new Size(Math.Max(60, colorImage.Width / 10), Math.Max(60, colorImage.Height / 10)),
                Size.Empty);
        }

        checkInfo.IsFaceDetected = faces.Length > 0;

        using Bitmap fullBitmap = ConvertToBitmap(colorImage);

        if (!checkInfo.IsFaceDetected)
        {
            PopulateGlobalChecksWithoutFace(colorImage, grayImage, fullBitmap, checkInfo);
            return checkInfo;
        }

        Rectangle faceRect = faces.OrderByDescending(r => r.Width * r.Height).First();
        faceRect = EnsureWithinBounds(faceRect, colorImage.Width, colorImage.Height);

        using Mat faceGray = new(grayImage, faceRect);
        using Mat faceColor = new(colorImage, faceRect);
        using Bitmap faceBitmap = ConvertToBitmap(faceColor);

        Rectangle[] eyeRects;
        using (CascadeClassifier eyeCascade = LoadCascadeClassifier(eyeFileName))
        {
            eyeRects = GetProminentEyes(
                eyeCascade.DetectMultiScale(faceGray, 1.1, 5, new Size(Math.Max(20, faceRect.Width / 10), Math.Max(20, faceRect.Height / 10)), Size.Empty));
        }

        Rectangle[] noseRects;
        using (CascadeClassifier noseCascade = LoadCascadeClassifier(noseFileName))
        {
            noseRects = noseCascade.DetectMultiScale(faceGray, 1.1, 6, new Size(Math.Max(20, faceRect.Width / 8), Math.Max(20, faceRect.Height / 8)), Size.Empty);
        }

        Rectangle[] mouthRects;
        using (CascadeClassifier mouthCascade = LoadCascadeClassifier(mouthFileName))
        {
            mouthRects = mouthCascade.DetectMultiScale(faceGray, 1.1, 15, new Size(Math.Max(25, faceRect.Width / 6), Math.Max(20, faceRect.Height / 6)), Size.Empty);
        }

        Rectangle[] smileRects;
        using (CascadeClassifier smileCascade = LoadCascadeClassifier(smileFileName))
        {
            smileRects = smileCascade.DetectMultiScale(faceGray, 1.2, 22, new Size(Math.Max(25, faceRect.Width / 6), Math.Max(15, faceRect.Height / 8)), Size.Empty);
        }

        checkInfo.IsEyesValid = eyeRects.Length >= 2;
        checkInfo.IsNoseValid = noseRects.Length > 0;
        checkInfo.IsMouthValid = mouthRects.Length > 0;
        checkInfo.IsFacialExpressionNeutral = smileRects.Length == 0;
        checkInfo.IsSmileValid = checkInfo.IsFacialExpressionNeutral;

        checkInfo.IsMouthClosed = EvaluateMouthClosed(faceGray, mouthRects);
        checkInfo.IsMouthValid = checkInfo.IsMouthClosed;

        checkInfo.IsNotBlinking = EvaluateBlinking(faceGray, eyeRects);
        checkInfo.IsNotLookingSideways = EvaluateLookingStraight(faceRect, eyeRects);
        checkInfo.IsNotLookingUpOrDown = EvaluateLookingUpOrDown(faceRect, eyeRects);
        checkInfo.IsFaceOrientationFrontal = EvaluateFaceOrientation(faceRect, eyeRects, noseRects);
        checkInfo.IsFaceWellLit = EvaluateFaceLighting(faceGray);
        checkInfo.IsNotOverexposed = EvaluateOverExposure(colorImage);
        checkInfo.IsProperLighting = checkInfo.IsFaceWellLit && checkInfo.IsNotOverexposed;

        checkInfo.IsWithoutGlasses = EvaluateGlassesAbsence(faceGray, eyeRects);
        checkInfo.IsGlassesGlareAbsent = EvaluateGlassesGlare(faceBitmap, eyeRects);
        checkInfo.IsRedEyeAbsent = EvaluateRedEye(faceBitmap, eyeRects);

        checkInfo.IsSkinTextureClear = EvaluateSkinTextureClarity(faceGray);
        checkInfo.IsSkinTextureNatural = EvaluateSkinTextureNatural(faceGray);
        checkInfo.IsColorNotFaded = EvaluateColorNotFaded(colorImage);
        checkInfo.IsSaturationBalanced = EvaluateSaturationBalanced(colorImage);
        checkInfo.IsSharp = EvaluateSharpness(grayImage);
        checkInfo.IsNotPixelated = EvaluatePixelation(colorImage);

        checkInfo.IsDistanceNotTooClose = EvaluateDistanceNotTooClose(faceRect, colorImage.Size);
        checkInfo.IsDistanceNotTooFar = EvaluateDistanceNotTooFar(faceRect, colorImage.Size);

        checkInfo.IsNotShotFromAbove = EvaluateShotFromAbove(faceRect, eyeRects, colorImage.Size);
        checkInfo.IsNotShotFromBelow = EvaluateShotFromBelow(faceRect, eyeRects, colorImage.Size);
        checkInfo.IsNotShotFromSide = EvaluateShotFromSide(faceRect, eyeRects, colorImage.Size);

        checkInfo.IsHeadAndShouldersVisible = EvaluateHeadAndShoulders(faceRect, colorImage.Size);
        checkInfo.IsBackgroundUniform = EvaluateBackgroundUniform(fullBitmap, faceRect);
        checkInfo.IsBackgroundValid = checkInfo.IsBackgroundUniform;

        checkInfo.IsFacePositionValid = checkInfo.IsFaceOrientationFrontal
            && checkInfo.IsNotShotFromAbove
            && checkInfo.IsNotShotFromBelow
            && checkInfo.IsNotShotFromSide;

        checkInfo.IsEyePositionValid = checkInfo.IsEyesValid
            && checkInfo.IsNotBlinking
            && checkInfo.IsNotLookingSideways
            && checkInfo.IsNotLookingUpOrDown;

        return checkInfo;
    }

    private static void PopulateGlobalChecksWithoutFace(Mat colorImage, Mat grayImage, Bitmap fullBitmap, CheckInfo checkInfo)
    {
        checkInfo.IsFaceDetected = false;
        checkInfo.IsSmileValid = false;
        checkInfo.IsMouthValid = false;
        checkInfo.IsNoseValid = false;
        checkInfo.IsEyesValid = false;
        checkInfo.IsHeadAndShouldersVisible = false;
        checkInfo.IsFacePositionValid = false;
        checkInfo.IsEyePositionValid = false;

        checkInfo.IsNotOverexposed = EvaluateOverExposure(colorImage);
        checkInfo.IsFaceWellLit = false;
        checkInfo.IsProperLighting = false;
        checkInfo.IsColorNotFaded = EvaluateColorNotFaded(colorImage);
        checkInfo.IsSaturationBalanced = EvaluateSaturationBalanced(colorImage);
        checkInfo.IsSharp = EvaluateSharpness(grayImage);
        checkInfo.IsNotPixelated = EvaluatePixelation(colorImage);
        checkInfo.IsBackgroundUniform = EvaluateBackgroundUniform(fullBitmap, Rectangle.Empty);
        checkInfo.IsBackgroundValid = checkInfo.IsBackgroundUniform;
    }

    private static bool IsImageValid(Mat image)
    {
        return image.Width >= MIN_WIDTH && image.Height >= MIN_HEIGHT;
    }

    private static bool EvaluateMouthClosed(Mat faceGray, Rectangle[] mouthRects)
    {
        if (mouthRects.Length == 0)
        {
            return true;
        }

        Rectangle mouthRect = mouthRects.OrderByDescending(r => r.Width * r.Height).First();
        mouthRect = EnsureWithinBounds(mouthRect, faceGray.Width, faceGray.Height);

        if (mouthRect.Width == 0 || mouthRect.Height == 0)
        {
            return true;
        }

        using Mat mouthRegion = new(faceGray, mouthRect);
        CvInvoke.GaussianBlur(mouthRegion, mouthRegion, new Size(3, 3), 0);
        double darkRatio = CalculateDarkRatio(mouthRegion, 55);
        double aspect = (double)mouthRect.Height / Math.Max(1, mouthRect.Width);

        return darkRatio < 0.28 && aspect < 0.45;
    }

    private static bool EvaluateBlinking(Mat faceGray, Rectangle[] eyeRects)
    {
        if (eyeRects.Length < 2)
        {
            return false;
        }

        int validEyes = 0;
        foreach (Rectangle rectRaw in eyeRects)
        {
            Rectangle eyeRect = EnsureWithinBounds(rectRaw, faceGray.Width, faceGray.Height);
            if (eyeRect.Width == 0 || eyeRect.Height == 0)
            {
                continue;
            }

            using Mat eyeRegion = new(faceGray, eyeRect);
            double aspect = (double)eyeRect.Height / Math.Max(1, eyeRect.Width);
            double darkRatio = CalculateDarkRatio(eyeRegion, 60);
            bool eyeOpen = aspect > 0.17 && darkRatio < 0.65;
            if (!eyeOpen)
            {
                return false;
            }

            validEyes++;
        }

        return validEyes >= 2;
    }

    private static bool EvaluateLookingStraight(Rectangle faceRect, Rectangle[] eyeRects)
    {
        if (eyeRects.Length < 2)
        {
            return false;
        }

        Rectangle left = eyeRects[0];
        Rectangle right = eyeRects[1];

        double leftCenter = left.X + left.Width / 2.0;
        double rightCenter = right.X + right.Width / 2.0;
        double faceCenter = faceRect.Width / 2.0;

        double midPoint = (leftCenter + rightCenter) / 2.0;
        double diff = Math.Abs(midPoint - faceCenter) / Math.Max(1, faceRect.Width);
        double symmetry = Math.Abs((faceCenter - leftCenter) - (rightCenter - faceCenter)) / Math.Max(1, faceRect.Width);

        return diff < 0.12 && symmetry < 0.12;
    }

    private static bool EvaluateLookingUpOrDown(Rectangle faceRect, Rectangle[] eyeRects)
    {
        if (eyeRects.Length < 2)
        {
            return false;
        }

        Rectangle left = eyeRects[0];
        Rectangle right = eyeRects[1];
        double averageEyeY = (left.Y + left.Height / 2.0 + right.Y + right.Height / 2.0) / 2.0;
        double normalized = averageEyeY / Math.Max(1, faceRect.Height);

        return normalized > 0.3 && normalized < 0.6;
    }

    private static bool EvaluateFaceOrientation(Rectangle faceRect, Rectangle[] eyeRects, Rectangle[] noseRects)
    {
        if (noseRects.Length > 0)
        {
            Rectangle nose = noseRects.OrderByDescending(r => r.Width * r.Height).First();
            double noseCenter = nose.X + nose.Width / 2.0;
            double faceCenter = faceRect.Width / 2.0;
            double diff = Math.Abs(noseCenter - faceCenter) / Math.Max(1, faceRect.Width);
            return diff < 0.15;
        }

        if (eyeRects.Length >= 2)
        {
            Rectangle left = eyeRects[0];
            Rectangle right = eyeRects[1];
            double midPoint = (left.X + left.Width / 2.0 + right.X + right.Width / 2.0) / 2.0;
            double faceCenter = faceRect.Width / 2.0;
            double diff = Math.Abs(midPoint - faceCenter) / Math.Max(1, faceRect.Width);
            return diff < 0.15;
        }

        return false;
    }

    private static bool EvaluateFaceLighting(Mat faceGray)
    {
        MCvScalar mean = CvInvoke.Mean(faceGray);
        return mean.V0 >= 90;
    }

    private static bool EvaluateGlassesAbsence(Mat faceGray, Rectangle[] eyeRects)
    {
        if (eyeRects.Length == 0)
        {
            return true;
        }

        double totalDensity = 0;
        int validEyes = 0;

        foreach (Rectangle rectRaw in eyeRects)
        {
            Rectangle eyeRect = EnsureWithinBounds(rectRaw, faceGray.Width, faceGray.Height);
            if (eyeRect.Width == 0 || eyeRect.Height == 0)
            {
                continue;
            }

            using Mat eyeRegion = new(faceGray, eyeRect);
            using Mat edges = new();
            CvInvoke.Canny(eyeRegion, edges, 50, 150);
            double density = CvInvoke.CountNonZero(edges) / (double)(eyeRect.Width * eyeRect.Height);
            totalDensity += density;
            validEyes++;
        }

        if (validEyes == 0)
        {
            return true;
        }

        double averageDensity = totalDensity / validEyes;
        return averageDensity < 0.32;
    }

    private static bool EvaluateGlassesGlare(Bitmap faceBitmap, Rectangle[] eyeRects)
    {
        if (eyeRects.Length == 0)
        {
            return true;
        }

        foreach (Rectangle rectRaw in eyeRects)
        {
            Rectangle eyeRect = EnsureWithinBounds(rectRaw, faceBitmap.Width, faceBitmap.Height);
            if (eyeRect.Width == 0 || eyeRect.Height == 0)
            {
                continue;
            }

            double brightRatio = GetBrightPixelRatio(faceBitmap, eyeRect, 230);
            if (brightRatio > 0.18)
            {
                return false;
            }
        }

        return true;
    }

    private static bool EvaluateRedEye(Bitmap faceBitmap, Rectangle[] eyeRects)
    {
        if (eyeRects.Length == 0)
        {
            return true;
        }

        foreach (Rectangle rectRaw in eyeRects)
        {
            Rectangle eyeRect = EnsureWithinBounds(rectRaw, faceBitmap.Width, faceBitmap.Height);
            if (eyeRect.Width == 0 || eyeRect.Height == 0)
            {
                continue;
            }

            int totalPixels = eyeRect.Width * eyeRect.Height;
            int redPixels = 0;

            for (int x = eyeRect.X; x < eyeRect.Right; x++)
            {
                for (int y = eyeRect.Y; y < eyeRect.Bottom; y++)
                {
                    Color pixel = faceBitmap.GetPixel(x, y);
                    if (pixel.R > 90 && pixel.R > pixel.G * 1.5 && pixel.R > pixel.B * 1.5)
                    {
                        redPixels++;
                    }
                }
            }

            double ratio = totalPixels == 0 ? 0 : redPixels / (double)totalPixels;
            if (ratio > 0.12)
            {
                return false;
            }
        }

        return true;
    }

    private static bool EvaluateSkinTextureClarity(Mat faceGray)
    {
        double variance = CalculateLaplacianVariance(faceGray);
        return variance >= 45;
    }

    private static bool EvaluateSkinTextureNatural(Mat faceGray)
    {
        using Mat blur = new();
        CvInvoke.GaussianBlur(faceGray, blur, new Size(7, 7), 0);
        using Mat texture = new();
        CvInvoke.AbsDiff(faceGray, blur, texture);
        CvInvoke.MeanStdDev(texture, out _, out MCvScalar stdDev);
        return stdDev.V0 >= 6;
    }

    private static bool EvaluateColorNotFaded(Mat colorImage)
    {
        double avgSaturation = CalculateAverageSaturation(colorImage);
        return avgSaturation >= 40;
    }

    private static bool EvaluateSaturationBalanced(Mat colorImage)
    {
        double avgSaturation = CalculateAverageSaturation(colorImage);
        return avgSaturation >= 40 && avgSaturation <= 200;
    }

    private static bool EvaluateSharpness(Mat grayImage)
    {
        double variance = CalculateLaplacianVariance(grayImage);
        return variance >= 80;
    }

    private static bool EvaluatePixelation(Mat colorImage)
    {
        if (colorImage.Width < 8 || colorImage.Height < 8)
        {
            return false;
        }

        Size downSize = new(Math.Max(2, colorImage.Width / 4), Math.Max(2, colorImage.Height / 4));
        using Mat small = new();
        CvInvoke.Resize(colorImage, small, downSize, 0, 0, Inter.Area);
        using Mat upscaled = new();
        CvInvoke.Resize(small, upscaled, colorImage.Size, 0, 0, Inter.Linear);
        using Mat diff = new();
        CvInvoke.AbsDiff(colorImage, upscaled, diff);
        using Mat diffGray = new();
        CvInvoke.CvtColor(diff, diffGray, ColorConversion.Bgr2Gray);
        MCvScalar mean = CvInvoke.Mean(diffGray);
        return mean.V0 >= 12;
    }

    private static bool EvaluateOverExposure(Mat colorImage)
    {
        using Mat gray = new();
        CvInvoke.CvtColor(colorImage, gray, ColorConversion.Bgr2Gray);
        using Mat mask = new();
        CvInvoke.Threshold(gray, mask, 245, 255, ThresholdType.Binary);
        double brightPixels = CvInvoke.CountNonZero(mask);
        double totalPixels = colorImage.Width * colorImage.Height;
        return brightPixels / Math.Max(1, totalPixels) < 0.02;
    }

    private static bool EvaluateDistanceNotTooClose(Rectangle faceRect, Size imageSize)
    {
        double ratio = faceRect.Height / (double)Math.Max(1, imageSize.Height);
        return ratio <= 0.7;
    }

    private static bool EvaluateDistanceNotTooFar(Rectangle faceRect, Size imageSize)
    {
        double ratio = faceRect.Height / (double)Math.Max(1, imageSize.Height);
        return ratio >= 0.25;
    }

    private static bool EvaluateShotFromAbove(Rectangle faceRect, Rectangle[] eyeRects, Size imageSize)
    {
        if (eyeRects.Length >= 2)
        {
            Rectangle left = eyeRects[0];
            Rectangle right = eyeRects[1];
            double averageEyeY = (left.Y + left.Height / 2.0 + right.Y + right.Height / 2.0) / 2.0;
            double normalized = averageEyeY / Math.Max(1, faceRect.Height);
            return normalized >= 0.25;
        }

        return faceRect.Y > imageSize.Height * 0.05;
    }

    private static bool EvaluateShotFromBelow(Rectangle faceRect, Rectangle[] eyeRects, Size imageSize)
    {
        if (eyeRects.Length >= 2)
        {
            Rectangle left = eyeRects[0];
            Rectangle right = eyeRects[1];
            double averageEyeY = (left.Y + left.Height / 2.0 + right.Y + right.Height / 2.0) / 2.0;
            double normalized = averageEyeY / Math.Max(1, faceRect.Height);
            return normalized <= 0.7;
        }

        return faceRect.Bottom < imageSize.Height * 0.95;
    }

    private static bool EvaluateShotFromSide(Rectangle faceRect, Rectangle[] eyeRects, Size imageSize)
    {
        if (eyeRects.Length >= 2)
        {
            Rectangle left = eyeRects[0];
            Rectangle right = eyeRects[1];
            double averageEyeX = (left.X + left.Width / 2.0 + right.X + right.Width / 2.0) / 2.0;
            double normalized = averageEyeX / Math.Max(1, faceRect.Width);
            return normalized > 0.3 && normalized < 0.7;
        }

        return faceRect.Left > imageSize.Width * 0.05 && faceRect.Right < imageSize.Width * 0.95;
    }

    private static bool EvaluateHeadAndShoulders(Rectangle faceRect, Size imageSize)
    {
        double heightRatio = faceRect.Height / (double)Math.Max(1, imageSize.Height);
        double widthRatio = faceRect.Width / (double)Math.Max(1, imageSize.Width);
        bool centeredHorizontally = faceRect.Left > imageSize.Width * 0.1 && faceRect.Right < imageSize.Width * 0.9;
        bool verticalPosition = faceRect.Top > imageSize.Height * 0.05 && faceRect.Bottom < imageSize.Height * 0.95;
        return heightRatio >= 0.35 && heightRatio <= 0.7 && widthRatio >= 0.25 && widthRatio <= 0.6 && centeredHorizontally && verticalPosition;
    }

    private static bool EvaluateBackgroundUniform(Bitmap imageBitmap, Rectangle faceRect)
    {
        if (imageBitmap.Width == 0 || imageBitmap.Height == 0)
        {
            return false;
        }

        int sampleWidth = Math.Max(10, imageBitmap.Width / 8);
        int sampleHeight = Math.Max(10, imageBitmap.Height / 8);

        List<Rectangle> sampleAreas = new()
        {
            new Rectangle(0, 0, sampleWidth, sampleHeight),
            new Rectangle(imageBitmap.Width - sampleWidth, 0, sampleWidth, sampleHeight),
            new Rectangle(0, imageBitmap.Height - sampleHeight, sampleWidth, sampleHeight),
            new Rectangle(imageBitmap.Width - sampleWidth, imageBitmap.Height - sampleHeight, sampleWidth, sampleHeight),
            new Rectangle((imageBitmap.Width - sampleWidth) / 2, 0, sampleWidth, sampleHeight),
            new Rectangle((imageBitmap.Width - sampleWidth) / 2, imageBitmap.Height - sampleHeight, sampleWidth, sampleHeight)
        };

        List<Color> samples = new();

        foreach (Rectangle rect in sampleAreas)
        {
            Rectangle adjusted = EnsureWithinBounds(rect, imageBitmap.Width, imageBitmap.Height);
            if (adjusted.Width == 0 || adjusted.Height == 0)
            {
                continue;
            }

            if (faceRect != Rectangle.Empty && adjusted.IntersectsWith(faceRect))
            {
                continue;
            }

            samples.Add(CalculateAverageColor(imageBitmap, adjusted));
        }

        if (samples.Count < 2)
        {
            return false;
        }

        double maxDistance = 0;
        for (int i = 0; i < samples.Count; i++)
        {
            for (int j = i + 1; j < samples.Count; j++)
            {
                double distance = ColorDistance(samples[i], samples[j]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }
        }

        return maxDistance < 25;
    }

    private static double CalculateLaplacianVariance(Mat grayImage)
    {
        using Mat laplacian = new();
        CvInvoke.Laplacian(grayImage, laplacian, DepthType.Cv64F);
        CvInvoke.MeanStdDev(laplacian, out _, out MCvScalar stdDev);
        return stdDev.V0 * stdDev.V0;
    }

    private static double CalculateAverageSaturation(Mat colorImage)
    {
        using Image<Bgr, byte> bgrImage = colorImage.ToImage<Bgr, byte>();
        using Image<Hsv, byte> hsvImage = bgrImage.Convert<Hsv, byte>();
        MCvScalar average = hsvImage.GetAverage();
        return average.V1;
    }

    private static double CalculateDarkRatio(Mat region, double threshold)
    {
        using Mat binary = new();
        CvInvoke.Threshold(region, binary, threshold, 255, ThresholdType.BinaryInv);
        double darkPixels = CvInvoke.CountNonZero(binary);
        double totalPixels = region.Width * region.Height;
        return darkPixels / Math.Max(1, totalPixels);
    }

    private static Rectangle EnsureWithinBounds(Rectangle rect, int maxWidth, int maxHeight)
    {
        int x = Math.Clamp(rect.X, 0, Math.Max(0, maxWidth - 1));
        int y = Math.Clamp(rect.Y, 0, Math.Max(0, maxHeight - 1));
        int width = Math.Clamp(rect.Width, 0, Math.Max(0, maxWidth - x));
        int height = Math.Clamp(rect.Height, 0, Math.Max(0, maxHeight - y));
        return new Rectangle(x, y, width, height);
    }

    private static Rectangle[] GetProminentEyes(Rectangle[] eyeRects)
    {
        return eyeRects
            .OrderByDescending(r => r.Width * r.Height)
            .Take(2)
            .OrderBy(r => r.X)
            .ToArray();
    }

    private static double GetBrightPixelRatio(Bitmap bitmap, Rectangle rect, byte threshold)
    {
        long brightPixels = 0;
        long totalPixels = Math.Max(1, rect.Width * rect.Height);

        for (int x = rect.X; x < rect.Right; x++)
        {
            for (int y = rect.Y; y < rect.Bottom; y++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                double brightness = 0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B;
                if (brightness >= threshold)
                {
                    brightPixels++;
                }
            }
        }

        return brightPixels / (double)totalPixels;
    }

    private static Color CalculateAverageColor(Bitmap bitmap, Rectangle rect)
    {
        long r = 0;
        long g = 0;
        long b = 0;
        long total = 0;

        for (int x = rect.X; x < rect.Right; x++)
        {
            for (int y = rect.Y; y < rect.Bottom; y++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                r += pixel.R;
                g += pixel.G;
                b += pixel.B;
                total++;
            }
        }

        if (total == 0)
        {
            return Color.Black;
        }

        return Color.FromArgb((int)(r / total), (int)(g / total), (int)(b / total));
    }

    private static double ColorDistance(Color c1, Color c2)
    {
        double dr = c1.R - c2.R;
        double dg = c1.G - c2.G;
        double db = c1.B - c2.B;
        return Math.Sqrt(dr * dr + dg * dg + db * db);
    }

    private static Mat LoadImage(byte[] imageBytes)
    {
        Mat image = new();
        CvInvoke.Imdecode(imageBytes, ImreadModes.Color, image);
        return image;
    }

    private static CascadeClassifier LoadCascadeClassifier(string fileName)
    {
        return new CascadeClassifier(fileName);
    }

    private static Bitmap ConvertToBitmap(Mat image)
    {
        using Image<Bgr, byte> frame = image.ToImage<Bgr, byte>();
        return frame.ToBitmap();
    }
}
