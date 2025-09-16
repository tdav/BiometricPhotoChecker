using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BiometricPhotoChecker;
using BiometricPhotoChecker.Service;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

public class BiometricPhotoValidationService : IBiometricPhotoValidationService
{
    private static readonly string BaseCascadePath = Path.Combine(Directory.GetParent(System.Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades");
    private static readonly string faceFileName = Path.Combine(BaseCascadePath, "haarcascade_frontalface_default.xml");
    private static readonly string smileFileName = Path.Combine(BaseCascadePath, "haarcascade_smile.xml");
    private static readonly string eyeFileName = Path.Combine(BaseCascadePath, "haarcascade_eye.xml");
    private static readonly string eyeGlassesFileName = Path.Combine(BaseCascadePath, "haarcascade_eye_tree_eyeglasses.xml");
    private static readonly string mouthFileName = Path.Combine(BaseCascadePath, "haarcascade_mcs_mouth.xml");
    private static readonly string noseFileName = Path.Combine(BaseCascadePath, "haarcascade_mcs_nose.xml");
    private static readonly string profileFaceFileName = Path.Combine(BaseCascadePath, "haarcascade_profileface.xml");

    private const int MIN_WIDTH = 35;
    private const int MIN_HEIGHT = 45;

    public CheckInfo ValidateBiometricPhoto(byte[] imageBytes)
    {
        CheckInfo checkInfo = new CheckInfo();

        using Mat colorImage = LoadImage(imageBytes);
        using Mat grayImage = new Mat();
        CvInvoke.CvtColor(colorImage, grayImage, ColorConversion.Bgr2Gray);

        checkInfo.IsImageValid = IsImageValid(colorImage);

        Rectangle[] faces = DetectFaces(grayImage);
        Rectangle primaryFace = GetLargestRectangle(faces);
        bool hasFace = !primaryFace.IsEmpty;
        checkInfo.IsFaceDetected = hasFace;

        Rectangle leftEyeRect = Rectangle.Empty;
        Rectangle rightEyeRect = Rectangle.Empty;
        bool hasEyePair = hasFace && TryGetEyePair(grayImage, primaryFace, out leftEyeRect, out rightEyeRect);

        checkInfo.IsNotOverexposed = IsNotOverexposed(grayImage);
        checkInfo.IsProperLighting = IsProperLighting(grayImage);
        checkInfo.AreColorsNotFaded = AreColorsNotFaded(colorImage);
        checkInfo.IsSaturationBalanced = IsSaturationBalanced(colorImage);
        checkInfo.IsNotPixelated = IsNotPixelated(colorImage, grayImage);
        checkInfo.IsImageSharp = IsImageSharp(grayImage);

        bool backgroundBright = IsBackgroundBrightEnough(colorImage);
        bool backgroundUniform = IsBackgroundUniform(colorImage);
        checkInfo.IsBackgroundUniform = backgroundUniform;
        checkInfo.IsBackgroundValid = backgroundBright && backgroundUniform;

        checkInfo.IsFacialExpressionNeutral = hasFace && IsFacialExpressionNeutral(grayImage, primaryFace);
        checkInfo.IsSmileValid = checkInfo.IsFacialExpressionNeutral;

        checkInfo.IsMouthValid = hasFace && IsMouthDetected(grayImage, primaryFace);
        checkInfo.IsMouthClosed = hasFace && IsMouthClosed(grayImage, primaryFace);
        checkInfo.IsMouthValid = checkInfo.IsMouthValid && checkInfo.IsMouthClosed;

        checkInfo.IsNoseValid = hasFace && IsNoseValid(grayImage, primaryFace);
        checkInfo.IsWithoutGlasses = hasFace && IsWithoutGlasses(grayImage, primaryFace);
        checkInfo.IsFaceWellLit = hasFace && IsFaceWellLit(grayImage, primaryFace) && checkInfo.IsNotOverexposed;
        checkInfo.IsSkinTextureClear = hasFace && IsSkinTextureClear(grayImage, primaryFace);
        checkInfo.IsSkinTextureNatural = hasFace && IsSkinTextureNatural(grayImage, primaryFace);
        checkInfo.IsFaceNotTurnedSideways = hasFace && IsFaceNotTurnedSideways(grayImage, primaryFace);
        checkInfo.IsNotTooClose = hasFace && IsNotTooClose(colorImage, primaryFace);
        checkInfo.IsNotTooFar = hasFace && IsNotTooFar(colorImage, primaryFace);
        checkInfo.IsFaceNotTooHigh = hasFace && IsFaceNotTooHigh(colorImage, primaryFace);
        checkInfo.IsFaceNotTooLow = hasFace && IsFaceNotTooLow(colorImage, primaryFace);
        checkInfo.IsFaceNotTooLeft = hasFace && IsFaceNotTooLeft(colorImage, primaryFace);
        checkInfo.IsFacePositionValid = hasFace && IsFacePositionValid(colorImage, primaryFace);
        checkInfo.IsHeadAndShouldersVisible = hasFace && IsHeadAndShouldersVisible(colorImage, primaryFace);

        checkInfo.IsEyesValid = hasEyePair;
        checkInfo.AreEyesOpen = hasEyePair && AreEyesOpen(grayImage, primaryFace, leftEyeRect, rightEyeRect);
        checkInfo.IsGazeStraightAhead = hasEyePair && IsGazeStraightAhead(grayImage, primaryFace, leftEyeRect, rightEyeRect);
        checkInfo.IsGazeLevel = hasEyePair && IsGazeLevel(grayImage, primaryFace, leftEyeRect, rightEyeRect);
        checkInfo.IsRedEyeAbsent = hasEyePair && IsRedEyeAbsent(colorImage, primaryFace, leftEyeRect, rightEyeRect);
        checkInfo.IsGlareAbsent = hasEyePair && IsGlareAbsent(colorImage, primaryFace, leftEyeRect, rightEyeRect);
        checkInfo.IsEyePositionValid = hasEyePair && IsEyePositionValid(primaryFace, leftEyeRect, rightEyeRect);
        checkInfo.IsEyesValid = checkInfo.IsEyesValid && checkInfo.AreEyesOpen && checkInfo.IsWithoutGlasses && checkInfo.IsRedEyeAbsent && checkInfo.IsGlareAbsent;

        return checkInfo;
    }

    private bool IsImageValid(Mat image)
    {
        return image.Width >= MIN_WIDTH && image.Height >= MIN_HEIGHT;
    }

    private Rectangle[] DetectFaces(Mat grayImage)
    {
        using CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName);
        return faceCascade.DetectMultiScale(grayImage, 1.1, 3, Size.Empty, Size.Empty);
    }

    private Rectangle GetLargestRectangle(Rectangle[] rectangles)
    {
        if (rectangles == null || rectangles.Length == 0)
        {
            return Rectangle.Empty;
        }

        Rectangle largest = rectangles[0];
        int largestArea = largest.Width * largest.Height;

        for (int i = 1; i < rectangles.Length; i++)
        {
            int area = rectangles[i].Width * rectangles[i].Height;
            if (area > largestArea)
            {
                largest = rectangles[i];
                largestArea = area;
            }
        }

        return largest;
    }

    private bool TryGetEyePair(Mat grayImage, Rectangle faceRect, out Rectangle leftEyeRect, out Rectangle rightEyeRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        Rectangle eyeSearchArea = new Rectangle(0, 0, faceRegion.Width, Math.Max(1, faceRegion.Height / 2));
        using Mat eyesRegion = new Mat(faceRegion, eyeSearchArea);
        using CascadeClassifier eyeCascade = LoadCascadeClassifier(eyeFileName);
        Rectangle[] detectedEyes = eyeCascade.DetectMultiScale(eyesRegion, 1.1, 5, Size.Empty, new Size(Math.Max(15, faceRect.Width / 10), Math.Max(15, faceRect.Height / 10)));

        if (detectedEyes.Length < 2)
        {
            leftEyeRect = Rectangle.Empty;
            rightEyeRect = Rectangle.Empty;
            return false;
        }

        Rectangle[] selectedEyes = detectedEyes
            .OrderByDescending(r => r.Width * r.Height)
            .Take(2)
            .OrderBy(r => r.X)
            .ToArray();

        leftEyeRect = selectedEyes[0];
        rightEyeRect = selectedEyes[1];
        leftEyeRect.Offset(eyeSearchArea.X, eyeSearchArea.Y);
        rightEyeRect.Offset(eyeSearchArea.X, eyeSearchArea.Y);
        return true;
    }

    private bool IsFacialExpressionNeutral(Mat grayImage, Rectangle faceRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        using CascadeClassifier smileCascade = LoadCascadeClassifier(smileFileName);
        CvInvoke.EqualizeHist(faceRegion, faceRegion);
        Rectangle[] smiles = smileCascade.DetectMultiScale(faceRegion, 1.8, 20, Size.Empty, new Size(Math.Max(15, faceRect.Width / 5), Math.Max(15, faceRect.Height / 5)));
        return smiles.Length == 0;
    }

    private bool IsMouthDetected(Mat grayImage, Rectangle faceRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        Rectangle mouthSearchArea = new Rectangle(0, faceRegion.Height / 2, faceRegion.Width, Math.Max(1, faceRegion.Height / 2));
        using Mat mouthRegion = new Mat(faceRegion, mouthSearchArea);
        using CascadeClassifier mouthCascade = LoadCascadeClassifier(mouthFileName);
        Rectangle[] mouths = mouthCascade.DetectMultiScale(mouthRegion, 1.1, 11, Size.Empty, new Size(Math.Max(20, faceRect.Width / 5), Math.Max(15, faceRect.Height / 8)));
        return mouths.Length > 0;
    }

    private bool IsMouthClosed(Mat grayImage, Rectangle faceRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        Rectangle mouthSearchArea = new Rectangle(0, faceRegion.Height / 2, faceRegion.Width, Math.Max(1, faceRegion.Height / 2));
        using Mat mouthRegion = new Mat(faceRegion, mouthSearchArea);
        using CascadeClassifier mouthCascade = LoadCascadeClassifier(mouthFileName);
        Rectangle[] mouths = mouthCascade.DetectMultiScale(mouthRegion, 1.1, 11, Size.Empty, new Size(Math.Max(20, faceRect.Width / 5), Math.Max(15, faceRect.Height / 8)));

        if (mouths.Length == 0)
        {
            return true;
        }

        Rectangle mouthRect = mouths.OrderByDescending(r => r.Width * r.Height).First();
        mouthRect.Offset(mouthSearchArea.X, mouthSearchArea.Y);

        using Mat detectedMouth = new Mat(faceRegion, mouthRect);
        CvInvoke.EqualizeHist(detectedMouth, detectedMouth);
        using Mat binary = new Mat();
        CvInvoke.Threshold(detectedMouth, binary, 60, 255, ThresholdType.BinaryInv);
        double openRatio = CvInvoke.CountNonZero(binary) / (double)(mouthRect.Width * mouthRect.Height);
        return openRatio < 0.3;
    }

    private bool IsNoseValid(Mat grayImage, Rectangle faceRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        using CascadeClassifier noseCascade = LoadCascadeClassifier(noseFileName);
        Rectangle[] noses = noseCascade.DetectMultiScale(faceRegion, 1.1, 5, Size.Empty, new Size(Math.Max(15, faceRect.Width / 6), Math.Max(15, faceRect.Height / 6)));
        return noses.Length > 0;
    }

    private bool IsWithoutGlasses(Mat grayImage, Rectangle faceRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        using CascadeClassifier glassesCascade = LoadCascadeClassifier(eyeGlassesFileName);
        Rectangle[] glasses = glassesCascade.DetectMultiScale(faceRegion, 1.05, 5, Size.Empty, new Size(Math.Max(15, faceRect.Width / 6), Math.Max(15, faceRect.Height / 6)));
        return glasses.Length == 0;
    }

    private bool AreEyesOpen(Mat grayImage, Rectangle faceRect, Rectangle leftEyeRect, Rectangle rightEyeRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        foreach (Rectangle eyeRect in new[] { leftEyeRect, rightEyeRect })
        {
            double aspectRatio = (double)eyeRect.Height / Math.Max(1, eyeRect.Width);
            if (aspectRatio < 0.18)
            {
                return false;
            }

            using Mat eyeRegion = new Mat(faceRegion, eyeRect);
            var (_, _, success) = CalculatePupilOffsets(eyeRegion);
            if (!success)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsGazeStraightAhead(Mat grayImage, Rectangle faceRect, Rectangle leftEyeRect, Rectangle rightEyeRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        foreach (Rectangle eyeRect in new[] { leftEyeRect, rightEyeRect })
        {
            using Mat eyeRegion = new Mat(faceRegion, eyeRect);
            var (horizontalOffset, _, success) = CalculatePupilOffsets(eyeRegion);
            if (!success || Math.Abs(horizontalOffset) > 0.25)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsGazeLevel(Mat grayImage, Rectangle faceRect, Rectangle leftEyeRect, Rectangle rightEyeRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        foreach (Rectangle eyeRect in new[] { leftEyeRect, rightEyeRect })
        {
            using Mat eyeRegion = new Mat(faceRegion, eyeRect);
            var (_, verticalOffset, success) = CalculatePupilOffsets(eyeRegion);
            if (!success || Math.Abs(verticalOffset) > 0.25)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsRedEyeAbsent(Mat colorImage, Rectangle faceRect, Rectangle leftEyeRect, Rectangle rightEyeRect)
    {
        using Mat faceRegion = new Mat(colorImage, faceRect);
        foreach (Rectangle eyeRect in new[] { leftEyeRect, rightEyeRect })
        {
            using Mat eyeRegion = new Mat(faceRegion, eyeRect);
            if (HasRedEye(eyeRegion))
            {
                return false;
            }
        }

        return true;
    }

    private bool HasRedEye(Mat eyeRegion)
    {
        using Mat eyeHsv = new Mat();
        CvInvoke.CvtColor(eyeRegion, eyeHsv, ColorConversion.Bgr2Hsv);

        using Mat lowerMask = new Mat();
        using Mat upperMask = new Mat();
        using Mat combinedMask = new Mat();
        using ScalarArray lowerRed1 = new ScalarArray(new MCvScalar(0, 100, 80));
        using ScalarArray upperRed1 = new ScalarArray(new MCvScalar(10, 255, 255));
        using ScalarArray lowerRed2 = new ScalarArray(new MCvScalar(160, 100, 80));
        using ScalarArray upperRed2 = new ScalarArray(new MCvScalar(179, 255, 255));

        CvInvoke.InRange(eyeHsv, lowerRed1, upperRed1, lowerMask);
        CvInvoke.InRange(eyeHsv, lowerRed2, upperRed2, upperMask);
        CvInvoke.BitwiseOr(lowerMask, upperMask, combinedMask);

        double ratio = CvInvoke.CountNonZero(combinedMask) / (double)(eyeRegion.Width * eyeRegion.Height);
        return ratio > 0.15;
    }

    private bool IsGlareAbsent(Mat colorImage, Rectangle faceRect, Rectangle leftEyeRect, Rectangle rightEyeRect)
    {
        using Mat faceRegion = new Mat(colorImage, faceRect);
        foreach (Rectangle eyeRect in new[] { leftEyeRect, rightEyeRect })
        {
            using Mat eyeRegion = new Mat(faceRegion, eyeRect);
            if (HasStrongGlare(eyeRegion))
            {
                return false;
            }
        }

        return true;
    }

    private bool HasStrongGlare(Mat eyeRegion)
    {
        using Mat eyeHsv = new Mat();
        CvInvoke.CvtColor(eyeRegion, eyeHsv, ColorConversion.Bgr2Hsv);
        using ScalarArray lower = new ScalarArray(new MCvScalar(0, 0, 220));
        using ScalarArray upper = new ScalarArray(new MCvScalar(179, 80, 255));
        using Mat mask = new Mat();
        CvInvoke.InRange(eyeHsv, lower, upper, mask);
        double ratio = CvInvoke.CountNonZero(mask) / (double)(eyeRegion.Width * eyeRegion.Height);
        return ratio > 0.1;
    }

    private bool IsFaceWellLit(Mat grayImage, Rectangle faceRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        double average = CvInvoke.Mean(faceRegion).V0;
        return average >= 90 && average <= 220;
    }

    private bool IsSkinTextureClear(Mat grayImage, Rectangle faceRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        double variance = CalculateLaplacianVariance(faceRegion);
        return variance > 60;
    }

    private bool IsSkinTextureNatural(Mat grayImage, Rectangle faceRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        using Mat blurred = new Mat();
        CvInvoke.GaussianBlur(faceRegion, blurred, new Size(3, 3), 0);
        using Mat diff = new Mat();
        CvInvoke.AbsDiff(faceRegion, blurred, diff);
        double meanDifference = CvInvoke.Mean(diff).V0;
        return meanDifference > 8;
    }

    private bool IsFaceNotTurnedSideways(Mat grayImage, Rectangle faceRect)
    {
        using Mat faceRegion = new Mat(grayImage, faceRect);
        using CascadeClassifier profileCascade = LoadCascadeClassifier(profileFaceFileName);
        Rectangle[] profiles = profileCascade.DetectMultiScale(faceRegion, 1.1, 3, Size.Empty, new Size(Math.Max(15, faceRect.Width / 5), Math.Max(15, faceRect.Height / 5)));
        if (profiles.Length > 0)
        {
            return false;
        }

        using Mat flipped = new Mat();
        CvInvoke.Flip(faceRegion, flipped, FlipType.Horizontal);
        Rectangle[] mirrorProfiles = profileCascade.DetectMultiScale(flipped, 1.1, 3, Size.Empty, new Size(Math.Max(15, faceRect.Width / 5), Math.Max(15, faceRect.Height / 5)));
        return mirrorProfiles.Length == 0;
    }

    private bool IsNotTooClose(Mat colorImage, Rectangle faceRect)
    {
        double ratio = (double)faceRect.Height / colorImage.Height;
        return ratio <= 0.7;
    }

    private bool IsNotTooFar(Mat colorImage, Rectangle faceRect)
    {
        double ratio = (double)faceRect.Height / colorImage.Height;
        return ratio >= 0.3;
    }

    private bool IsFaceNotTooHigh(Mat colorImage, Rectangle faceRect)
    {
        double centerY = faceRect.Y + faceRect.Height / 2.0;
        return centerY >= colorImage.Height * 0.35;
    }

    private bool IsFaceNotTooLow(Mat colorImage, Rectangle faceRect)
    {
        double centerY = faceRect.Y + faceRect.Height / 2.0;
        return centerY <= colorImage.Height * 0.65;
    }

    private bool IsFaceNotTooLeft(Mat colorImage, Rectangle faceRect)
    {
        double centerX = faceRect.X + faceRect.Width / 2.0;
        return centerX >= colorImage.Width * 0.35 && centerX <= colorImage.Width * 0.65;
    }

    private bool IsFacePositionValid(Mat colorImage, Rectangle faceRect)
    {
        double faceCenterX = faceRect.X + faceRect.Width / 2.0;
        double faceCenterY = faceRect.Y + faceRect.Height / 2.0;
        double thresholdX = colorImage.Width * 0.1;
        double thresholdY = colorImage.Height * 0.1;
        return Math.Abs(faceCenterX - colorImage.Width / 2.0) <= thresholdX && Math.Abs(faceCenterY - colorImage.Height / 2.0) <= thresholdY;
    }

    private bool IsEyePositionValid(Rectangle faceRect, Rectangle leftEyeRect, Rectangle rightEyeRect)
    {
        double leftEyeCenterY = leftEyeRect.Y + leftEyeRect.Height / 2.0;
        double rightEyeCenterY = rightEyeRect.Y + rightEyeRect.Height / 2.0;
        double verticalThreshold = faceRect.Height * 0.05;
        if (Math.Abs(leftEyeCenterY - rightEyeCenterY) > verticalThreshold)
        {
            return false;
        }

        double leftEyeCenterX = leftEyeRect.X + leftEyeRect.Width / 2.0;
        double rightEyeCenterX = rightEyeRect.X + rightEyeRect.Width / 2.0;
        double distanceBetweenEyes = Math.Abs(leftEyeCenterX - rightEyeCenterX);
        if (distanceBetweenEyes < faceRect.Width * 0.2)
        {
            return false;
        }

        double faceCenterX = faceRect.Width / 2.0;
        double horizontalThreshold = faceRect.Width * 0.2;
        return Math.Abs(leftEyeCenterX - faceCenterX) <= horizontalThreshold && Math.Abs(rightEyeCenterX - faceCenterX) <= horizontalThreshold;
    }

    private bool IsHeadAndShouldersVisible(Mat colorImage, Rectangle faceRect)
    {
        double ratio = (double)faceRect.Height / colorImage.Height;
        if (ratio < 0.3 || ratio > 0.6)
        {
            return false;
        }

        double topMargin = faceRect.Y;
        double bottomMargin = colorImage.Height - faceRect.Bottom;
        return topMargin >= colorImage.Height * 0.05 && bottomMargin >= colorImage.Height * 0.15;
    }

    private bool AreColorsNotFaded(Mat colorImage)
    {
        double saturation = CalculateAverageSaturation(colorImage);
        return saturation >= 0.2;
    }

    private bool IsSaturationBalanced(Mat colorImage)
    {
        double saturation = CalculateAverageSaturation(colorImage);
        return saturation >= 0.2 && saturation <= 0.8;
    }

    private bool IsNotPixelated(Mat colorImage, Mat grayImage)
    {
        if (colorImage.Width < 300 || colorImage.Height < 300)
        {
            return false;
        }

        double variance = CalculateLaplacianVariance(grayImage);
        return variance > 80;
    }

    private bool IsImageSharp(Mat grayImage)
    {
        double variance = CalculateLaplacianVariance(grayImage);
        return variance > 70;
    }

    private bool IsProperLighting(Mat grayImage)
    {
        double average = CvInvoke.Mean(grayImage).V0;
        return average >= 80 && average <= 240;
    }

    private bool IsNotOverexposed(Mat grayImage)
    {
        double average = CvInvoke.Mean(grayImage).V0;
        return average <= 230;
    }

    private bool IsBackgroundBrightEnough(Mat colorImage)
    {
        using Bitmap bitmap = ConvertToBitmap(colorImage);
        int sampleWidth = Math.Max(1, bitmap.Width / 10);
        int sampleHeight = Math.Max(1, bitmap.Height / 10);

        Color topLeft = CalculateAverageColor(bitmap, new Rectangle(5, 5, sampleWidth, sampleHeight));
        Color topRight = CalculateAverageColor(bitmap, new Rectangle(bitmap.Width - sampleWidth - 5, 5, sampleWidth, sampleHeight));
        Color bottomLeft = CalculateAverageColor(bitmap, new Rectangle(5, bitmap.Height - sampleHeight - 5, sampleWidth, sampleHeight));
        Color bottomRight = CalculateAverageColor(bitmap, new Rectangle(bitmap.Width - sampleWidth - 5, bitmap.Height - sampleHeight - 5, sampleWidth, sampleHeight));

        int averageR = (topLeft.R + topRight.R + bottomLeft.R + bottomRight.R) / 4;
        int averageG = (topLeft.G + topRight.G + bottomLeft.G + bottomRight.G) / 4;
        int averageB = (topLeft.B + topRight.B + bottomLeft.B + bottomRight.B) / 4;

        return averageR >= 200 && averageG >= 200 && averageB >= 200;
    }

    private bool IsBackgroundUniform(Mat colorImage)
    {
        using Bitmap bitmap = ConvertToBitmap(colorImage);
        int sampleWidth = Math.Max(1, bitmap.Width / 10);
        int sampleHeight = Math.Max(1, bitmap.Height / 10);

        List<Color> samples = new List<Color>
        {
            CalculateAverageColor(bitmap, new Rectangle(5, 5, sampleWidth, sampleHeight)),
            CalculateAverageColor(bitmap, new Rectangle(bitmap.Width - sampleWidth - 5, 5, sampleWidth, sampleHeight)),
            CalculateAverageColor(bitmap, new Rectangle(5, bitmap.Height - sampleHeight - 5, sampleWidth, sampleHeight)),
            CalculateAverageColor(bitmap, new Rectangle(bitmap.Width - sampleWidth - 5, bitmap.Height - sampleHeight - 5, sampleWidth, sampleHeight)),
            CalculateAverageColor(bitmap, new Rectangle((bitmap.Width - sampleWidth) / 2, (bitmap.Height - sampleHeight) / 2, sampleWidth, sampleHeight))
        };

        double maxDistance = 0;
        for (int i = 0; i < samples.Count; i++)
        {
            for (int j = i + 1; j < samples.Count; j++)
            {
                double distance = CalculateColorDistance(samples[i], samples[j]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }
        }

        return maxDistance <= 30;
    }

    private Bitmap ConvertToBitmap(Mat image)
    {
        using Image<Bgr, byte> bgr = image.ToImage<Bgr, byte>();
        return bgr.ToBitmap();
    }

    private Color CalculateAverageColor(Bitmap image, Rectangle rect)
    {
        int startX = Math.Max(0, rect.X);
        int startY = Math.Max(0, rect.Y);
        int endX = Math.Min(image.Width, rect.X + rect.Width);
        int endY = Math.Min(image.Height, rect.Y + rect.Height);

        if (startX >= endX || startY >= endY)
        {
            return Color.Black;
        }

        double r = 0;
        double g = 0;
        double b = 0;
        int count = 0;

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Color pixel = image.GetPixel(x, y);
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

    private double CalculateColorDistance(Color first, Color second)
    {
        double dr = first.R - second.R;
        double dg = first.G - second.G;
        double db = first.B - second.B;
        return Math.Sqrt(dr * dr + dg * dg + db * db);
    }

    private double CalculateAverageSaturation(Mat colorImage)
    {
        using Image<Bgr, byte> bgr = colorImage.ToImage<Bgr, byte>();
        using Image<Hsv, byte> hsv = bgr.Convert<Hsv, byte>();
        double sum = 0;
        int rows = hsv.Rows;
        int cols = hsv.Cols;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                sum += hsv.Data[y, x, 1];
            }
        }

        double count = rows * cols;
        if (count == 0)
        {
            return 0;
        }

        return (sum / count) / 255.0;
    }

    private double CalculateLaplacianVariance(Mat grayImage)
    {
        using Mat laplacian = new Mat();
        CvInvoke.Laplacian(grayImage, laplacian, DepthType.Cv64F);
        using Image<Gray, double> laplacianImage = laplacian.ToImage<Gray, double>();

        double sum = 0;
        double sumSq = 0;
        int rows = laplacianImage.Rows;
        int cols = laplacianImage.Cols;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                double value = laplacianImage.Data[y, x, 0];
                sum += value;
                sumSq += value * value;
            }
        }

        double count = rows * cols;
        if (count <= 0)
        {
            return 0;
        }

        double mean = sum / count;
        return (sumSq / count) - (mean * mean);
    }

    private (double Horizontal, double Vertical, bool Success) CalculatePupilOffsets(Mat eyeRegion)
    {
        if (eyeRegion.Width <= 0 || eyeRegion.Height <= 0)
        {
            return (0, 0, false);
        }

        using Mat blurred = new Mat();
        CvInvoke.GaussianBlur(eyeRegion, blurred, new Size(5, 5), 0);
        using Mat binary = new Mat();
        CvInvoke.Threshold(blurred, binary, 60, 255, ThresholdType.BinaryInv);
        MCvMoments moments = CvInvoke.Moments(binary, true);

        if (Math.Abs(moments.M00) < double.Epsilon)
        {
            return (0, 0, false);
        }

        double cx = moments.M10 / moments.M00;
        double cy = moments.M01 / moments.M00;
        double horizontalOffset = (cx / eyeRegion.Width) - 0.5;
        double verticalOffset = (cy / eyeRegion.Height) - 0.5;
        return (horizontalOffset, verticalOffset, true);
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
}
