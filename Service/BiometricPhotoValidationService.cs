using BiometricPhotoChecker;
using BiometricPhotoChecker.Service;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

public class BiometricPhotoValidationService : IBiometricPhotoValidationService
{
    private static readonly string faceFileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades", "haarcascade_frontalface_default.xml");
    private static readonly string smileFileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades", "haarcascade_smile.xml");
    private static readonly string eyeFileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades", "haarcascade_eye.xml");
    private static readonly string mouthFileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades", "haarcascade_mcs_mouth.xml");
    private static readonly string noseFileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "haarcascades", "haarcascade_mcs_nose.xml");
    private const int MIN_WIDTH = 35;
    private const int MIN_HEIGHT = 45;

    public CheckInfo ValidateBiometricPhoto(byte[] imageBytes)
    {
        CheckInfo checkInfo = new CheckInfo();

        EyeAnalysis eyeAnalysis = AnalyzeEyesAndFace(imageBytes);

        checkInfo.IsFaceDetected = eyeAnalysis.FaceDetected;
        checkInfo.HasGlasses = eyeAnalysis.HasGlasses;
        checkInfo.HasGlassesGlare = eyeAnalysis.HasGlassesGlare;
        checkInfo.IsBlinking = eyeAnalysis.IsBlinking;
        checkInfo.HasRedEyeEffect = eyeAnalysis.HasRedEye;

        checkInfo.IsMouthOpen = IsMouthOpen(imageBytes, eyeAnalysis.FaceRectangle);
        bool hasSmile = HasSmile(imageBytes, eyeAnalysis.FaceRectangle);
        checkInfo.IsExpressionNeutral = !hasSmile && !checkInfo.IsMouthOpen;

        checkInfo.IsLookingAway = DetermineLookingAway(eyeAnalysis);
        checkInfo.IsFaceTurnedSideways = DetermineFaceTurnedSideways(eyeAnalysis);
        checkInfo.IsLookingUpOrDown = DetermineLookingUpOrDown(eyeAnalysis);
        checkInfo.IsTooClose = DetermineTooClose(eyeAnalysis);
        checkInfo.IsTooFar = DetermineTooFar(eyeAnalysis);
        checkInfo.IsCameraAngleAbove = DetermineCameraAngleAbove(eyeAnalysis);
        checkInfo.IsCameraAngleBelow = DetermineCameraAngleBelow(eyeAnalysis);
        checkInfo.IsCameraAngleLeft = DetermineCameraAngleLeft(eyeAnalysis);

        double skinTextureScore = CalculateSkinTextureScore(imageBytes, eyeAnalysis.FaceRectangle);
        checkInfo.IsSkinTextureClear = skinTextureScore >= 40;
        checkInfo.HasSkinRetouching = skinTextureScore > 0 && skinTextureScore < 30;

        checkInfo.IsFaceIlluminated = IsFaceIlluminated(imageBytes, eyeAnalysis.FaceRectangle);

        double saturation = CalculateAverageSaturation(imageBytes);
        checkInfo.HasColorFading = saturation > 0 && saturation < 60;
        checkInfo.IsSaturationAdequate = saturation >= 60 && saturation <= 200;

        checkInfo.HasPixelation = HasPixelation(imageBytes);
        checkInfo.HasOverExposure = HasOverExposure(imageBytes);
        checkInfo.IsImageSharp = IsImageSharp(imageBytes);
        checkInfo.IsBackgroundUniform = IsBackgroundUniform(imageBytes);

        checkInfo.IsImageValid = IsImageValid(imageBytes);
        checkInfo.IsSmileValid = !hasSmile;
        checkInfo.IsMouthValid = IsMouthValid(imageBytes);
        checkInfo.IsNoseValid = IsNoseValid(imageBytes);
        checkInfo.IsEyesValid = IsEyesValid(imageBytes);
        checkInfo.IsHeadAndShouldersVisible = IsHeadAndShouldersVisible(imageBytes);
        checkInfo.IsBackgroundValid = IsBackgroundValid(imageBytes);
        checkInfo.IsProperLighting = IsProperLighting(imageBytes);
        checkInfo.IsFacePositionValid = IsFacePositionValid(imageBytes);
        checkInfo.IsEyePositionValid = IsEyePositionValid(imageBytes);

        return checkInfo;
    }

    private struct EyeAnalysis
    {
        public bool FaceDetected;
        public Rectangle FaceRectangle;
        public Rectangle[] EyeRectangles;
        public int ImageWidth;
        public int ImageHeight;
        public bool HasGlasses;
        public bool HasGlassesGlare;
        public bool IsBlinking;
        public bool HasRedEye;
    }

    private EyeAnalysis AnalyzeEyesAndFace(byte[] imageBytes)
    {
        EyeAnalysis analysis = new EyeAnalysis
        {
            FaceDetected = false,
            FaceRectangle = Rectangle.Empty,
            EyeRectangles = Array.Empty<Rectangle>(),
            ImageWidth = 0,
            ImageHeight = 0,
            HasGlasses = false,
            HasGlassesGlare = false,
            IsBlinking = false,
            HasRedEye = false
        };

        using (Mat image = LoadImage(imageBytes))
        {
            if (image.IsEmpty)
            {
                return analysis;
            }

            analysis.ImageWidth = image.Width;
            analysis.ImageHeight = image.Height;

            using (Mat gray = new Mat())
            using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
            {
                CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
                Rectangle[] faces = faceCascade.DetectMultiScale(gray, 1.1, 3, Size.Empty, Size.Empty);
                if (faces.Length == 0)
                {
                    return analysis;
                }

                Rectangle face = faces.OrderByDescending(f => f.Width * f.Height).First();
                Rectangle safeFace = GetSafeRectangle(face, image.Size);
                if (safeFace == Rectangle.Empty)
                {
                    return analysis;
                }

                analysis.FaceDetected = true;
                analysis.FaceRectangle = safeFace;

                using (Mat faceGray = new Mat(gray, safeFace))
                using (Mat faceColor = new Mat(image, safeFace))
                using (CascadeClassifier eyeCascade = LoadCascadeClassifier(eyeFileName))
                {
                    Rectangle[] localEyes = eyeCascade.DetectMultiScale(faceGray, 1.1, 2, new Size(Math.Max(5, safeFace.Width / 8), Math.Max(5, safeFace.Height / 10)), Size.Empty);
                    Rectangle[] absoluteEyes = localEyes.Select(e => new Rectangle(safeFace.X + e.X, safeFace.Y + e.Y, e.Width, e.Height)).ToArray();
                    analysis.EyeRectangles = absoluteEyes;

                    if (localEyes.Length < 2)
                    {
                        analysis.IsBlinking = true;
                    }

                    foreach (Rectangle localEye in localEyes)
                    {
                        using Mat eyeGray = new Mat(faceGray, localEye);
                        using Mat eyeColor = new Mat(faceColor, localEye);

                        double aspectRatio = (double)localEye.Height / Math.Max(1, localEye.Width);
                        if (aspectRatio < 0.18)
                        {
                            analysis.IsBlinking = true;
                        }

                        double edgeDensity = CalculateEdgeDensity(eyeGray);
                        double brightRatio = CalculateBrightRatio(eyeGray, 225);
                        if (edgeDensity > 0.33)
                        {
                            analysis.HasGlasses = true;
                        }

                        if (brightRatio > 0.08)
                        {
                            analysis.HasGlassesGlare = true;
                        }

                        double redEyeRatio = CalculateRedEyeRatio(eyeColor);
                        if (redEyeRatio > 0.025)
                        {
                            analysis.HasRedEye = true;
                        }
                    }
                }
            }
        }

        return analysis;
    }

    private bool DetermineLookingAway(EyeAnalysis analysis)
    {
        if (!analysis.FaceDetected)
        {
            return true;
        }

        double faceCenterX = analysis.FaceRectangle.X + analysis.FaceRectangle.Width / 2.0;
        double horizontalOffset = Math.Abs(faceCenterX - analysis.ImageWidth / 2.0) / Math.Max(1, analysis.ImageWidth);
        if (horizontalOffset > 0.18)
        {
            return true;
        }

        if (analysis.EyeRectangles == null || analysis.EyeRectangles.Length < 2)
        {
            return true;
        }

        Rectangle[] orderedEyes = analysis.EyeRectangles.OrderBy(r => r.X).Take(2).ToArray();
        double faceCenter = analysis.FaceRectangle.X + analysis.FaceRectangle.Width / 2.0;
        double leftDistance = faceCenter - (orderedEyes[0].X + orderedEyes[0].Width / 2.0);
        double rightDistance = (orderedEyes[1].X + orderedEyes[1].Width / 2.0) - faceCenter;
        double imbalance = Math.Abs(leftDistance - rightDistance) / Math.Max(1, analysis.FaceRectangle.Width);

        return imbalance > 0.25;
    }

    private bool DetermineFaceTurnedSideways(EyeAnalysis analysis)
    {
        if (!analysis.FaceDetected || analysis.EyeRectangles == null || analysis.EyeRectangles.Length < 2)
        {
            return true;
        }

        Rectangle[] orderedEyes = analysis.EyeRectangles.OrderBy(r => r.X).Take(2).ToArray();
        double faceCenter = analysis.FaceRectangle.X + analysis.FaceRectangle.Width / 2.0;
        double leftCenter = orderedEyes[0].X + orderedEyes[0].Width / 2.0;
        double rightCenter = orderedEyes[1].X + orderedEyes[1].Width / 2.0;
        double leftDistance = faceCenter - leftCenter;
        double rightDistance = rightCenter - faceCenter;
        double ratio = Math.Abs(leftDistance - rightDistance) / Math.Max(1, analysis.FaceRectangle.Width);

        return ratio > 0.2;
    }

    private bool DetermineLookingUpOrDown(EyeAnalysis analysis)
    {
        if (!analysis.FaceDetected || analysis.EyeRectangles == null || analysis.EyeRectangles.Length < 2)
        {
            return true;
        }

        double averageEyeY = analysis.EyeRectangles.Average(r => r.Y + r.Height / 2.0);
        double faceCenterY = analysis.FaceRectangle.Y + analysis.FaceRectangle.Height / 2.0;
        double offset = Math.Abs(averageEyeY - faceCenterY) / Math.Max(1, analysis.FaceRectangle.Height);

        return offset > 0.18;
    }

    private bool DetermineTooClose(EyeAnalysis analysis)
    {
        if (!analysis.FaceDetected)
        {
            return false;
        }

        double faceArea = analysis.FaceRectangle.Width * analysis.FaceRectangle.Height;
        double imageArea = Math.Max(1, analysis.ImageWidth * analysis.ImageHeight);
        double ratio = faceArea / imageArea;

        return ratio > 0.55;
    }

    private bool DetermineTooFar(EyeAnalysis analysis)
    {
        if (!analysis.FaceDetected)
        {
            return true;
        }

        double faceArea = analysis.FaceRectangle.Width * analysis.FaceRectangle.Height;
        double imageArea = Math.Max(1, analysis.ImageWidth * analysis.ImageHeight);
        double ratio = faceArea / imageArea;

        return ratio < 0.15;
    }

    private bool DetermineCameraAngleAbove(EyeAnalysis analysis)
    {
        if (!analysis.FaceDetected)
        {
            return false;
        }

        double normalizedCenterY = (analysis.FaceRectangle.Y + analysis.FaceRectangle.Height / 2.0) / Math.Max(1, analysis.ImageHeight);
        return normalizedCenterY > 0.65;
    }

    private bool DetermineCameraAngleBelow(EyeAnalysis analysis)
    {
        if (!analysis.FaceDetected)
        {
            return false;
        }

        double normalizedCenterY = (analysis.FaceRectangle.Y + analysis.FaceRectangle.Height / 2.0) / Math.Max(1, analysis.ImageHeight);
        return normalizedCenterY < 0.35;
    }

    private bool DetermineCameraAngleLeft(EyeAnalysis analysis)
    {
        if (!analysis.FaceDetected)
        {
            return false;
        }

        double normalizedCenterX = (analysis.FaceRectangle.X + analysis.FaceRectangle.Width / 2.0) / Math.Max(1, analysis.ImageWidth);
        return normalizedCenterX < 0.35;
    }

    private bool IsFaceIlluminated(byte[] imageBytes, Rectangle faceRect)
    {
        if (faceRect == Rectangle.Empty)
        {
            return false;
        }

        using (Mat image = LoadImage(imageBytes))
        using (Mat gray = new Mat())
        {
            CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
            Rectangle validFace = GetSafeRectangle(faceRect, image.Size);
            if (validFace == Rectangle.Empty)
            {
                return false;
            }

            using Mat faceRegion = new Mat(gray, validFace);
            double brightness = CalculateAverageIntensity(faceRegion);
            return brightness >= 90 && brightness <= 200;
        }
    }

    private double CalculateSkinTextureScore(byte[] imageBytes, Rectangle faceRect)
    {
        if (faceRect == Rectangle.Empty)
        {
            return 0;
        }

        using (Mat image = LoadImage(imageBytes))
        using (Mat gray = new Mat())
        {
            CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
            Rectangle validFace = GetSafeRectangle(faceRect, image.Size);
            if (validFace == Rectangle.Empty)
            {
                return 0;
            }

            using Mat faceRegion = new Mat(gray, validFace);
            return CalculateLaplacianVariance(faceRegion);
        }
    }

    private bool IsImageSharp(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        using (Mat gray = new Mat())
        {
            CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
            double variance = CalculateLaplacianVariance(gray);
            return variance > 45;
        }
    }

    private double CalculateAverageSaturation(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        using (Image<Hsv, byte> hsvImage = image.ToImage<Hsv, byte>())
        {
            int rows = hsvImage.Rows;
            int cols = hsvImage.Cols;
            if (rows == 0 || cols == 0)
            {
                return 0;
            }

            long sum = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    sum += hsvImage.Data[y, x, 1];
                }
            }

            return (double)sum / (rows * cols);
        }
    }

    private bool HasPixelation(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        using (Bitmap bitmap = ConvertToBitmap(image))
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            
            //using Bitmap formattedBitmap = bitmap.Clone(rect, PixelFormat.Format24bppRgb);

            Bitmap clone = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(clone))
            {
                g.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
            }

            double pixelationScore = CalculatePixelationScore(clone);
            return pixelationScore > 20;
        }
    }

    private bool HasOverExposure(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        using (Mat gray = new Mat())
        using (Mat threshold = new Mat())
        {
            CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
            CvInvoke.Threshold(gray, threshold, 240, 255, ThresholdType.Binary);
            double brightRatio = CalculateRatio(threshold);
            return brightRatio > 0.12;
        }
    }

    private bool IsBackgroundUniform(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        using (Bitmap bitmap = ConvertToBitmap(image))
        {
            if (bitmap.Width < 20 || bitmap.Height < 20)
            {
                return false;
            }

            int margin = Math.Max(5, Math.Min(bitmap.Width, bitmap.Height) / 40);
            int rectWidth = Math.Max(5, bitmap.Width / 6);
            int rectHeight = Math.Max(5, bitmap.Height / 6);

            Rectangle topLeft = CreateSafeRectangle(margin, margin, rectWidth, rectHeight, bitmap.Width, bitmap.Height);
            Rectangle topRight = CreateSafeRectangle(bitmap.Width - rectWidth - margin, margin, rectWidth, rectHeight, bitmap.Width, bitmap.Height);
            Rectangle bottomLeft = CreateSafeRectangle(margin, bitmap.Height - rectHeight - margin, rectWidth, rectHeight, bitmap.Width, bitmap.Height);
            Rectangle bottomRight = CreateSafeRectangle(bitmap.Width - rectWidth - margin, bitmap.Height - rectHeight - margin, rectWidth, rectHeight, bitmap.Width, bitmap.Height);

            if (topLeft == Rectangle.Empty || topRight == Rectangle.Empty || bottomLeft == Rectangle.Empty || bottomRight == Rectangle.Empty)
            {
                return false;
            }

            Color avgTopLeft = CalculateAverageColor(bitmap, topLeft);
            Color avgTopRight = CalculateAverageColor(bitmap, topRight);
            Color avgBottomLeft = CalculateAverageColor(bitmap, bottomLeft);
            Color avgBottomRight = CalculateAverageColor(bitmap, bottomRight);

            double maxDiff = GetMaxColorDifference(new[] { avgTopLeft, avgTopRight, avgBottomLeft, avgBottomRight });

            double varianceTopLeft = CalculateColorVariance(bitmap, topLeft);
            double varianceTopRight = CalculateColorVariance(bitmap, topRight);
            double varianceBottomLeft = CalculateColorVariance(bitmap, bottomLeft);
            double varianceBottomRight = CalculateColorVariance(bitmap, bottomRight);
            bool smallVariance = varianceTopLeft < 0.004 && varianceTopRight < 0.004 && varianceBottomLeft < 0.004 && varianceBottomRight < 0.004;

            return maxDiff < 35 && smallVariance;
        }
    }

    private bool IsMouthOpen(byte[] imageBytes, Rectangle faceRect)
    {
        using (Mat image = LoadImage(imageBytes))
        using (Mat gray = new Mat())
        using (CascadeClassifier mouthCascade = LoadCascadeClassifier(mouthFileName))
        {
            CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);

            Rectangle searchRegion = faceRect == Rectangle.Empty
                ? new Rectangle(0, 0, image.Width, image.Height)
                : new Rectangle(faceRect.X, faceRect.Y + faceRect.Height / 2, faceRect.Width, faceRect.Height / 2);

            Rectangle validRegion = GetSafeRectangle(searchRegion, image.Size);
            if (validRegion == Rectangle.Empty)
            {
                return false;
            }

            using Mat region = new Mat(gray, validRegion);
            Rectangle[] mouths = mouthCascade.DetectMultiScale(region, 1.1, 2, Size.Empty, Size.Empty);

            foreach (Rectangle mouth in mouths)
            {
                using Mat mouthRegion = new Mat(region, mouth);
                double darkRatio = CalculateDarkPixelRatio(mouthRegion, 70);
                double aspect = (double)mouth.Height / Math.Max(1, mouth.Width);
                if (darkRatio > 0.28 && aspect > 0.35)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private bool HasSmile(byte[] imageBytes, Rectangle faceRect)
    {
        using (Mat image = LoadImage(imageBytes))
        using (Mat gray = new Mat())
        using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
        using (CascadeClassifier smileCascade = LoadCascadeClassifier(smileFileName))
        {
            CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
            List<Rectangle> faces = new List<Rectangle>();

            if (faceRect != Rectangle.Empty)
            {
                Rectangle validFace = GetSafeRectangle(faceRect, image.Size);
                if (validFace != Rectangle.Empty)
                {
                    faces.Add(validFace);
                }
            }

            if (faces.Count == 0)
            {
                faces.AddRange(faceCascade.DetectMultiScale(gray, 1.1, 3, Size.Empty, Size.Empty));
            }

            foreach (Rectangle face in faces)
            {
                using Mat faceRegion = new Mat(gray, face);
                CvInvoke.EqualizeHist(faceRegion, faceRegion);
                Rectangle[] smiles = smileCascade.DetectMultiScale(faceRegion, 1.7, 20, Size.Empty, Size.Empty);
                if (smiles.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private double CalculateEdgeDensity(Mat grayRegion)
    {
        using Mat edges = new Mat();
        CvInvoke.Canny(grayRegion, edges, 50, 150);
        double area = grayRegion.Width * grayRegion.Height;
        return area <= 0 ? 0 : CvInvoke.CountNonZero(edges) / area;
    }

    private double CalculateBrightRatio(Mat grayRegion, double threshold)
    {
        using Mat binary = new Mat();
        CvInvoke.Threshold(grayRegion, binary, threshold, 255, ThresholdType.Binary);
        return CalculateRatio(binary);
    }

    private double CalculateRedEyeRatio(Mat colorRegion)
    {
        using Image<Bgr, byte> eyeImage = colorRegion.ToImage<Bgr, byte>();
        int rows = eyeImage.Rows;
        int cols = eyeImage.Cols;
        if (rows == 0 || cols == 0)
        {
            return 0;
        }

        int redPixels = 0;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                byte b = eyeImage.Data[y, x, 0];
                byte g = eyeImage.Data[y, x, 1];
                byte r = eyeImage.Data[y, x, 2];
                if (r > 140 && r > g * 1.4 && r > b * 1.4)
                {
                    redPixels++;
                }
            }
        }

        return (double)redPixels / (rows * cols);
    }

    private double CalculateDarkPixelRatio(Mat grayRegion, double threshold)
    {
        using Mat binary = new Mat();
        CvInvoke.Threshold(grayRegion, binary, threshold, 255, ThresholdType.BinaryInv);
        return CalculateRatio(binary);
    }

    private double CalculateAverageIntensity(Mat grayRegion)
    {
        MCvScalar mean = CvInvoke.Mean(grayRegion);
        return mean.V0;
    }

    private double CalculateLaplacianVariance(Mat grayRegion)
    {
        MCvScalar stdDev= new MCvScalar();
        MCvScalar tmp = new MCvScalar();

        using Mat laplacian = new Mat();
        CvInvoke.Laplacian(grayRegion, laplacian, DepthType.Cv64F);
        CvInvoke.MeanStdDev(laplacian, ref tmp,  ref stdDev);
        double stdValue = stdDev.V0;
        return stdValue * stdValue;
    }

    private double CalculateRatio(Mat binaryMask)
    {
        double totalPixels = binaryMask.Width * binaryMask.Height;
        if (totalPixels <= 0)
        {
            return 0;
        }

        int nonZero = CvInvoke.CountNonZero(binaryMask);
        return nonZero / totalPixels;
    }

    private double CalculatePixelationScore(Bitmap bitmap)
    {
        Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        try
        {
            int stride = data.Stride;
            int height = bitmap.Height;
            int width = bitmap.Width;
            int bytes = stride * height;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(data.Scan0, buffer, 0, bytes);

            double diffSum = 0;
            int samples = 0;

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * stride;
                for (int x = 8; x < width; x += 8)
                {
                    int offset = rowStart + x * 3;
                    int prevOffset = rowStart + (x - 1) * 3;
                    int gray = (buffer[offset] + buffer[offset + 1] + buffer[offset + 2]) / 3;
                    int prevGray = (buffer[prevOffset] + buffer[prevOffset + 1] + buffer[prevOffset + 2]) / 3;
                    diffSum += Math.Abs(gray - prevGray);
                    samples++;
                }
            }

            for (int x = 0; x < width; x++)
            {
                int columnOffset = x * 3;
                for (int y = 8; y < height; y += 8)
                {
                    int offset = y * stride + columnOffset;
                    int prevOffset = (y - 1) * stride + columnOffset;
                    int gray = (buffer[offset] + buffer[offset + 1] + buffer[offset + 2]) / 3;
                    int prevGray = (buffer[prevOffset] + buffer[prevOffset + 1] + buffer[prevOffset + 2]) / 3;
                    diffSum += Math.Abs(gray - prevGray);
                    samples++;
                }
            }

            return samples == 0 ? 0 : diffSum / samples;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    private Rectangle GetSafeRectangle(Rectangle rect, Size imageSize)
    {
        Rectangle bounds = new Rectangle(Point.Empty, imageSize);
        Rectangle safe = Rectangle.Intersect(bounds, rect);
        if (safe.Width <= 0 || safe.Height <= 0)
        {
            return Rectangle.Empty;
        }

        return safe;
    }

    private Rectangle CreateSafeRectangle(int x, int y, int width, int height, int maxWidth, int maxHeight)
    {
        Rectangle rect = new Rectangle(x, y, width, height);
        return GetSafeRectangle(rect, new Size(maxWidth, maxHeight));
    }

    private double GetMaxColorDifference(IReadOnlyList<Color> colors)
    {
        double maxDiff = 0;
        for (int i = 0; i < colors.Count; i++)
        {
            for (int j = i + 1; j < colors.Count; j++)
            {
                maxDiff = Math.Max(maxDiff, ColorDistance(colors[i], colors[j]));
            }
        }

        return maxDiff;
    }

    private double ColorDistance(Color first, Color second)
    {
        int dr = first.R - second.R;
        int dg = first.G - second.G;
        int db = first.B - second.B;
        return Math.Sqrt(dr * dr + dg * dg + db * db);
    }

    private double CalculateColorVariance(Bitmap image, Rectangle rect)
    {
        double total = 0;
        int count = 0;
        for (int x = rect.X; x < rect.Right; x++)
        {
            for (int y = rect.Y; y < rect.Bottom; y++)
            {
                total += image.GetPixel(x, y).GetBrightness();
                count++;
            }
        }

        if (count == 0)
        {
            return 0;
        }

        double mean = total / count;
        double variance = 0;
        for (int x = rect.X; x < rect.Right; x++)
        {
            for (int y = rect.Y; y < rect.Bottom; y++)
            {
                double brightness = image.GetPixel(x, y).GetBrightness();
                variance += Math.Pow(brightness - mean, 2);
            }
        }

        return variance / count;
    }

    private bool IsFacePositionValid(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        {
            using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
            {
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);

                if (faces.Length == 0)
                {
                    return false; // Yüz tespit edilemedi
                }

                Rectangle face = faces[0]; // İlk tespit edilen yüzü kullan
                int imageWidth = image.Width;
                int imageHeight = image.Height;

                // Yüzün öne bakması gereklidir
                double faceCenterX = face.X + face.Width / 2;
                double faceCenterY = face.Y + face.Height / 2;
                double faceCenterThresholdX = 0.1 * imageWidth; // Yatayda %10 sapma izin verilebilir

                if (Math.Abs(faceCenterX - imageWidth / 2) > faceCenterThresholdX)
                {
                    return false; // Yüz yatayda çok fazla sapmış
                }

                double faceCenterThresholdY = 0.1 * imageHeight; // Dikeyde %10 sapma izin verilebilir

                if (Math.Abs(faceCenterY - imageHeight / 2) > faceCenterThresholdY)
                {
                    return false; // Yüz dikeyde çok fazla sapmış
                }

                return true;
            }
        }
    }

    private bool IsEyePositionValid(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        {
            using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
            using (CascadeClassifier eyeCascade = LoadCascadeClassifier(eyeFileName))
            {
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);

                if (faces.Length == 0)
                {
                    return false; // Yüz tespit edilemedi
                }

                Rectangle face = faces[0]; // İlk tespit edilen yüzü kullan

                Rectangle[] eyes = eyeCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);

                if (eyes.Length < 2)
                {
                    return false; // İki göz tespit edilemedi
                }

                Rectangle leftEye = eyes[0]; // İlk gözü kullan
                Rectangle rightEye = eyes[1]; // İkinci gözü kullan
                int imageWidth = image.Width;
                int imageHeight = image.Height;

                // Gözler aynı yükseklikte olmalıdır
                double leftEyeCenterY = leftEye.Y + leftEye.Height / 2;
                double rightEyeCenterY = rightEye.Y + rightEye.Height / 2;
                double eyeCenterThresholdY = 0.05 * imageHeight; // Dikeyde %5 sapma izin verilebilir

                if (Math.Abs(leftEyeCenterY - rightEyeCenterY) > eyeCenterThresholdY)
                {
                    return false; // Gözler dikeyde çok fazla sapmış
                }

                // Gözler arasındaki yatay mesafe belirli bir aralıkta olmalıdır
                double eyeDistanceThreshold = 0.2 * imageWidth; // Yatayda %20 sapma izin verilebilir
                double leftEyeCenterX = leftEye.X + leftEye.Width / 2;
                double rightEyeCenterX = rightEye.X + rightEye.Width / 2;
                double distanceBetweenEyes = Math.Abs(leftEyeCenterX - rightEyeCenterX);

                if (distanceBetweenEyes < eyeDistanceThreshold)
                {
                    return false; // Gözler arası mesafe çok dar
                }

                // Gözlerin merkezleri yüzün merkezine yakın olmalıdır
                double eyeCenterToFaceCenterThreshold = 0.2 * imageWidth; // Yatayda %20 sapma izin verilebilir
                double faceCenterX = imageWidth / 2;

                if (Math.Abs(leftEyeCenterX - faceCenterX) > eyeCenterToFaceCenterThreshold ||
                    Math.Abs(rightEyeCenterX - faceCenterX) > eyeCenterToFaceCenterThreshold)
                {
                    return false; // Gözler yüz merkezine çok uzak
                }

                return true;
            }
        }
    } 

    private bool IsImageValid(byte[] imageBytes)
    {
        using (MemoryStream ms = new MemoryStream(imageBytes))
        {
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
            int width = image.Width; // Görüntünün genişliği (mm)
            int height = image.Height; // Görüntünün yüksekliği (mm)

            return (width >= MIN_WIDTH && height >= MIN_HEIGHT);
        }
    }
    private bool IsSmileValid(byte[] imageBytes)
    {
        return !HasSmile(imageBytes, Rectangle.Empty);
    }


    private bool IsEyesValid(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        {
            using (CascadeClassifier eyeCascade = LoadCascadeClassifier(eyeFileName))
            {
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);

                Rectangle[] eyes = eyeCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);

                return eyes.Length >= 2;
            }
        }
    }
    private bool IsMouthValid(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        {
            using (CascadeClassifier mouthCascade = LoadCascadeClassifier(mouthFileName))
            {
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);

                Rectangle[] mouths = mouthCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);

                return mouths.Length >= 1;
            }
        }
    }

    private bool IsNoseValid(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        {
            using (CascadeClassifier noseCascade = LoadCascadeClassifier(noseFileName))
            {
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);

                Rectangle[] noses = noseCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);

                return noses.Length >= 1;
            }
        }
    }
    private bool IsHeadAndShouldersVisible(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        {
            using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
            {
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);

                Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);

                foreach (Rectangle face in faces)
                {
                    Mat faceRegion = new Mat(image, face); 

                    bool headAndShouldersVisible = CheckHeadAndShoulders(faceRegion);

                    if (!headAndShouldersVisible)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private bool CheckHeadAndShoulders(Mat faceRegion)
    {
        // Yüz bölgesinin tamamının beyaz piksellerini sayın
        int totalPixels = faceRegion.Width * faceRegion.Height;
        int whitePixels = CvInvoke.CountNonZero(faceRegion);
        double visibleArea = (double)whitePixels / totalPixels;

        // Baş ve omuzlar en az %90 görünürse true döndürün (eşik değeri değiştirilebilir)
        return (visibleArea >= 0.90);
    }


    private bool IsBackgroundValid(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        {
            Bitmap imageBM = ConvertToBitmap(image);
            int bgRectWidth = image.Width / 10;
            int bgRectHeight = image.Height / 10;

            Rectangle topLeftRect = new Rectangle(5, 5, bgRectWidth, bgRectHeight);
            Color averageColorTopLeft = CalculateAverageColor(imageBM, topLeftRect);

            Rectangle topRightRect = new Rectangle(image.Width - bgRectWidth - 5, 5, bgRectWidth, bgRectHeight);
            Color averageColorTopRight = CalculateAverageColor(imageBM, topRightRect);

            Color RGB = Color.FromArgb((int)(averageColorTopLeft.R + averageColorTopRight.R) / 2, (int)(averageColorTopLeft.G + averageColorTopRight.G) / 2, (int)(averageColorTopLeft.B + averageColorTopRight.B) / 2);

            return (RGB.R >= 200 && RGB.G >= 200 && RGB.B >= 200);
        }
    }

    private Color CalculateAverageColor(Bitmap imageBM, Rectangle rect)
    {
        double R = 0;
        double G = 0;
        double B = 0;

        for (int x = rect.X; x < rect.X + rect.Width; x++)
        {
            for (int y = rect.Y; y < rect.Y + rect.Height; y++)
            {
                Color pixelColor = imageBM.GetPixel(x, y);
                R = R + pixelColor.R;
                G = G + pixelColor.G;
                B = B + pixelColor.B;
            }
        }

        R = R / (rect.Height * rect.Width);
        G = G / (rect.Height * rect.Width);
        B = B / (rect.Height * rect.Width);

        return Color.FromArgb((int)R, (int)G, (int)B);
    }

    private bool IsProperLighting(byte[] imageBytes)
    {
        using (MemoryStream ms = new MemoryStream(imageBytes))
        {
            Bitmap image = new Bitmap(ms);

            double totalBrightness = 0;

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    totalBrightness += (0.299 * pixelColor.R + 0.587 * pixelColor.G + 0.114 * pixelColor.B);
                }
            }

            double averageBrightness = totalBrightness / (image.Width * image.Height);

            return (averageBrightness >= 80 && averageBrightness <= 240);
        }
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

    private Bitmap ConvertToBitmap(Mat image)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            image.ToImage<Bgr, byte>().ToBitmap().Save(ms, ImageFormat.Jpeg);
            return new Bitmap(ms);
        }
    }
}
