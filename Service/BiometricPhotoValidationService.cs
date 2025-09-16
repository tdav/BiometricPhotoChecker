using BiometricPhotoChecker;
using BiometricPhotoChecker.Service;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;

public class BiometricPhotoValidationService : IBiometricPhotoValidationService
{
    private static string faceFileName = Path.Combine(System.Environment.CurrentDirectory, "haarcascades", "haarcascade_frontalface_default.xml");
    private static string smileFileName = Path.Combine(System.Environment.CurrentDirectory, "haarcascades", "haarcascade_smile.xml");
    private static string eyeFileName = Path.Combine(System.Environment.CurrentDirectory, "haarcascades", "haarcascade_eye.xml");
    private static string mouthFileName = Path.Combine(System.Environment.CurrentDirectory, "haarcascades", "haarcascade_mcs_mouth.xml");
    private static string noseFileName = Path.Combine(System.Environment.CurrentDirectory, "haarcascades", "haarcascade_mcs_nose.xml");
    private const int MIN_WIDTH = 35; // Minimum genişlik (mm)
    private const int MIN_HEIGHT = 45; // Minimum yükseklik (mm)

    public CheckInfo ValidateBiometricPhoto(byte[] imageBytes)
    {
        CheckInfo checkInfo = new CheckInfo();
        
        // Existing validations
        checkInfo.IsImageValid = IsImageValid(imageBytes);
        checkInfo.IsSmileValid = IsSmileValid(imageBytes);
        checkInfo.IsMouthValid = IsMouthValid(imageBytes);
        checkInfo.IsNoseValid = IsNoseValid(imageBytes);
        checkInfo.IsEyesValid = IsEyesValid(imageBytes);
        checkInfo.IsHeadAndShouldersVisible = IsHeadAndShouldersVisible(imageBytes);
        checkInfo.IsBackgroundValid = IsBackgroundValid(imageBytes);
        checkInfo.IsProperLighting = IsProperLighting(imageBytes);
        checkInfo.IsFacePositionValid = IsFacePositionValid(imageBytes);
        checkInfo.IsEyePositionValid = IsEyePositionValid(imageBytes);
        
        // New biometric validations
        checkInfo.IsFaceDetected = IsFaceDetected(imageBytes);
        checkInfo.IsFacialExpressionValid = IsFacialExpressionValid(imageBytes);
        checkInfo.AreEyesGlassesValid = AreEyesGlassesValid(imageBytes);
        checkInfo.IsEyeBlinkingDetected = IsEyeBlinkingDetected(imageBytes);
        checkInfo.IsMouthOpenDetected = IsMouthOpenDetected(imageBytes);
        checkInfo.IsLookingAway = IsLookingAway(imageBytes);
        checkInfo.IsDeerEyesDetected = IsDeerEyesDetected(imageBytes);
        checkInfo.IsFaceNotIlluminated = IsFaceNotIlluminated(imageBytes);
        checkInfo.IsUnclearFaceSkin = IsUnclearFaceSkin(imageBytes);
        checkInfo.AreColorsFaded = AreColorsFaded(imageBytes);
        checkInfo.IsPixelized = IsPixelized(imageBytes);
        checkInfo.IsSkinTextureReturned = IsSkinTextureReturned(imageBytes);
        checkInfo.IsGlassesEffectDetected = IsGlassesEffectDetected(imageBytes);
        checkInfo.IsOverexposed = IsOverexposed(imageBytes);
        checkInfo.IsFaceTurnedSideways = IsFaceTurnedSideways(imageBytes);
        checkInfo.IsLookingUpDown = IsLookingUpDown(imageBytes);
        checkInfo.IsTooClose = IsTooClose(imageBytes);
        checkInfo.IsTooFar = IsTooFar(imageBytes);
        checkInfo.IsFromAbove = IsFromAbove(imageBytes);
        checkInfo.IsFromBelow = IsFromBelow(imageBytes);
        checkInfo.IsFromLeft = IsFromLeft(imageBytes);
        checkInfo.IsImageSharpnessGood = IsImageSharpnessGood(imageBytes);
        checkInfo.IsSaturationGood = IsSaturationGood(imageBytes);
        checkInfo.IsBackgroundUniform = IsBackgroundUniform(imageBytes);
        
        return checkInfo;
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
        using (Mat image = LoadImage(imageBytes))
        {
            int width = image.Width; // Görüntünün genişliği (pixels)
            int height = image.Height; // Görüntünün yüksekliği (pixels)

            return (width >= MIN_WIDTH && height >= MIN_HEIGHT);
        }
    }
    private bool IsSmileValid(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        {
            using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
            using (CascadeClassifier smileCascade = LoadCascadeClassifier(smileFileName))
            {
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);

                Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);

                foreach (Rectangle face in faces)
                {
                    Mat faceRegion = new Mat(image, face);
                    CvInvoke.EqualizeHist(faceRegion, faceRegion); // Histogram eşitleme uygula

                    Rectangle[] smiles = smileCascade.DetectMultiScale(faceRegion, 1.8, 20, Size.Empty, Size.Empty);

                    foreach (Rectangle smile in smiles)
                    {
                        // Gülümseme tespiti bulunduğunda false döndürün
                        return false;
                    }
                }
            }
        }

        // Gülümseme tespiti bulunamadığında veya yüz tespiti yapılamadığında true döndürün
        return true;
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
            int bgRectWidth = image.Width / 10;
            int bgRectHeight = image.Height / 10;

            // Define regions for background analysis
            Rectangle topLeftRect = new Rectangle(5, 5, bgRectWidth, bgRectHeight);
            Rectangle topRightRect = new Rectangle(image.Width - bgRectWidth - 5, 5, bgRectWidth, bgRectHeight);

            // Calculate average colors using OpenCV
            MCvScalar avgColorTopLeft = CalculateAverageColorCV(image, topLeftRect);
            MCvScalar avgColorTopRight = CalculateAverageColorCV(image, topRightRect);

            // Calculate combined average
            int avgR = (int)((avgColorTopLeft.V2 + avgColorTopRight.V2) / 2);
            int avgG = (int)((avgColorTopLeft.V1 + avgColorTopRight.V1) / 2);
            int avgB = (int)((avgColorTopLeft.V0 + avgColorTopRight.V0) / 2);

            return (avgR >= 200 && avgG >= 200 && avgB >= 200);
        }
    }

    private MCvScalar CalculateAverageColorCV(Mat image, Rectangle rect)
    {
        Mat roi = new Mat(image, rect);
        return CvInvoke.Mean(roi);
    }

    private bool IsProperLighting(byte[] imageBytes)
    {
        using (Mat image = LoadImage(imageBytes))
        {
            // Convert to grayscale for brightness calculation
            Mat grayImage = new Mat();
            CvInvoke.CvtColor(image, grayImage, ColorConversion.Bgr2Gray);

            // Calculate mean brightness
            MCvScalar meanBrightness = CvInvoke.Mean(grayImage);
            double averageBrightness = meanBrightness.V0;

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

    // New biometric detection methods

    // Юз аниқланди — лицо определено
    private bool IsFaceDetected(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    return faces.Length > 0;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Юз ифодаси — выражение лица
    private bool IsFacialExpressionValid(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                using (CascadeClassifier smileCascade = LoadCascadeClassifier(smileFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    foreach (Rectangle face in faces)
                    {
                        Mat faceRegion = new Mat(image, face);
                        Rectangle[] smiles = smileCascade.DetectMultiScale(faceRegion, 1.8, 20, Size.Empty, Size.Empty);
                        
                        // Valid expression if no excessive smile detected (neutral expression preferred)
                        if (smiles.Length > 0) return false;
                    }
                    return faces.Length > 0; // Face detected with neutral expression
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Кўз кўзойнак — глаза (очки)  
    private bool AreEyesGlassesValid(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier eyeGlassesCascade = LoadCascadeClassifier(Path.Combine(System.Environment.CurrentDirectory, "haarcascades", "haarcascade_eye_tree_eyeglasses.xml")))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] glasses = eyeGlassesCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    // Valid if no glasses reflection or interference detected
                    return glasses.Length == 0;
                }
            }
        }
        catch
        {
            return true; // If cannot detect, assume no glasses
        }
    }

    // Кўзни пирпирлатмоқ — моргание
    private bool IsEyeBlinkingDetected(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier eyeCascade = LoadCascadeClassifier(eyeFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] eyes = eyeCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    // Check if less than 2 eyes detected (possible blinking)
                    return eyes.Length < 2;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Оғиз очиқ — рот открыт
    private bool IsMouthOpenDetected(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                using (CascadeClassifier mouthCascade = LoadCascadeClassifier(mouthFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    foreach (Rectangle face in faces)
                    {
                        Mat faceRegion = new Mat(image, face);
                        Rectangle[] mouths = mouthCascade.DetectMultiScale(faceRegion, 1.1, 3, Size.Empty, Size.Empty);
                        
                        // Check for larger mouth detection (indication of open mouth)
                        foreach (Rectangle mouth in mouths)
                        {
                            if (mouth.Height > mouth.Width * 0.6) // Height/Width ratio suggests open mouth
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Бошқа тарафга қараш — взгляд в сторону
    private bool IsLookingAway(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                using (CascadeClassifier eyeCascade = LoadCascadeClassifier(eyeFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    Rectangle[] eyes = eyeCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    if (faces.Length > 0 && eyes.Length >= 2)
                    {
                        Rectangle face = faces[0];
                        double faceCenterX = face.X + face.Width / 2.0;
                        double eyesCenterX = (eyes[0].X + eyes[1].X) / 2.0 + eyes[0].Width / 2.0;
                        
                        // If eyes center is significantly off from face center
                        return Math.Abs(eyesCenterX - faceCenterX) > face.Width * 0.15;
                    }
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Кийик кўз — взгляд (букв. «олений глаз»)
    private bool IsDeerEyesDetected(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier eyeCascade = LoadCascadeClassifier(eyeFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] eyes = eyeCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    // Check for unusually large or wide-set eyes
                    if (eyes.Length >= 2)
                    {
                        double eyeDistance = Math.Abs(eyes[0].X - eyes[1].X);
                        double avgEyeSize = (eyes[0].Width + eyes[1].Width) / 2.0;
                        
                        // Deer-like eyes: wide-set and large
                        return eyeDistance > avgEyeSize * 3;
                    }
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Юз атрофига ёритилмаган — лицо не освещено
    private bool IsFaceNotIlluminated(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    if (faces.Length > 0)
                    {
                        Mat faceRegion = new Mat(image, faces[0]);
                        MCvScalar faceAvgBrightness = CvInvoke.Mean(faceRegion);
                        
                        // Face is not well illuminated if brightness is too low
                        return faceAvgBrightness.V0 < 60;
                    }
                    return true;
                }
            }
        }
        catch
        {
            return true;
        }
    }

    // Нотайин юз териси — нечеткая кожа лица
    private bool IsUnclearFaceSkin(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    if (faces.Length > 0)
                    {
                        Mat faceRegion = new Mat(image, faces[0]);
                        Mat laplacian = new Mat();
                        CvInvoke.Laplacian(faceRegion, laplacian, DepthType.Cv64F);
                        
                        MCvScalar variance = CvInvoke.Mean(laplacian);
                        // Low variance indicates blurred/unclear skin
                        return variance.V0 < 100;
                    }
                    return true;
                }
            }
        }
        catch
        {
            return true;
        }
    }

    // Ранглар оғариб кетган — цвета поблекли
    private bool AreColorsFaded(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                // Convert to HSV to check saturation
                Mat hsvImage = new Mat();
                CvInvoke.CvtColor(image, hsvImage, ColorConversion.Bgr2Hsv);
                
                // Split channels
                VectorOfMat channels = new VectorOfMat();
                CvInvoke.Split(hsvImage, channels);
                
                // Check saturation channel (index 1)
                MCvScalar avgSaturation = CvInvoke.Mean(channels[1]);
                
                // Colors are faded if saturation is low
                return avgSaturation.V0 < 80;
            }
        }
        catch
        {
            return false;
        }
    }

    // Пикселлашиш — пикселизация  
    private bool IsPixelized(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                // Check for pixelation by detecting sharp edges
                Mat edges = new Mat();
                CvInvoke.Canny(image, edges, 50, 150);
                
                int totalPixels = image.Width * image.Height;
                int edgePixels = CvInvoke.CountNonZero(edges);
                
                // High edge ratio might indicate pixelation
                double edgeRatio = (double)edgePixels / totalPixels;
                return edgeRatio > 0.15;
            }
        }
        catch
        {
            return false;
        }
    }

    // Тери буртсиқ қайтарилиши — возврат кожной текстуры
    private bool IsSkinTextureReturned(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    if (faces.Length > 0)
                    {
                        Mat faceRegion = new Mat(image, faces[0]);
                        
                        // Apply Gaussian blur and compare with original to detect texture
                        Mat blurred = new Mat();
                        CvInvoke.GaussianBlur(faceRegion, blurred, new Size(15, 15), 0);
                        
                        Mat diff = new Mat();
                        CvInvoke.AbsDiff(faceRegion, blurred, diff);
                        
                        MCvScalar avgDiff = CvInvoke.Mean(diff);
                        // Good texture if there's sufficient detail
                        return avgDiff.V0 > 10;
                    }
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Кўзойнак таъсири — эффект очков
    private bool IsGlassesEffectDetected(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                // Convert to grayscale and detect circular shapes (lens reflections)
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                
                // Use HoughCircles to detect circular reflections from glasses
                CircleF[] circles = CvInvoke.HoughCircles(image, HoughModes.Gradient, 1, 50, 100, 30, 10, 100);
                
                // If multiple circles detected in eye region, likely glasses effect
                return circles.Length > 2;
            }
        }
        catch
        {
            return false;
        }
    }

    // Ёниб кетганлик — пересвет (яркое освещение)
    private bool IsOverexposed(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                Mat grayImage = new Mat();
                CvInvoke.CvtColor(image, grayImage, ColorConversion.Bgr2Gray);
                
                MCvScalar avgBrightness = CvInvoke.Mean(grayImage);
                
                // Also check for blown-out highlights
                int totalPixels = image.Width * image.Height;
                Mat mask = new Mat();
                CvInvoke.Threshold(grayImage, mask, 245, 255, ThresholdType.Binary);
                int brightPixels = CvInvoke.CountNonZero(mask);
                
                double brightPixelRatio = (double)brightPixels / totalPixels;
                
                // Overexposed if average brightness too high or too many bright pixels
                return avgBrightness.V0 > 240 || brightPixelRatio > 0.1;
            }
        }
        catch
        {
            return false;
        }
    }

    // Юзни енгга буриш — поворот лица вбок
    private bool IsFaceTurnedSideways(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                using (CascadeClassifier profileCascade = LoadCascadeClassifier(Path.Combine(System.Environment.CurrentDirectory, "haarcascades", "haarcascade_profileface.xml")))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] frontFaces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    Rectangle[] profileFaces = profileCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    // Face turned sideways if profile detected but not frontal
                    return profileFaces.Length > 0 && frontFaces.Length == 0;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Тепа-пастга қараш — взгляд вверх-вниз
    private bool IsLookingUpDown(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                using (CascadeClassifier eyeCascade = LoadCascadeClassifier(eyeFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    Rectangle[] eyes = eyeCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    if (faces.Length > 0 && eyes.Length >= 2)
                    {
                        Rectangle face = faces[0];
                        double faceCenterY = face.Y + face.Height / 2.0;
                        double eyesCenterY = (eyes[0].Y + eyes[1].Y) / 2.0 + eyes[0].Height / 2.0;
                        
                        // If eyes center is significantly off vertically from expected position
                        return Math.Abs(eyesCenterY - faceCenterY) > face.Height * 0.2;
                    }
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Жуда яқин — слишком близко
    private bool IsTooClose(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    if (faces.Length > 0)
                    {
                        Rectangle face = faces[0];
                        double faceArea = face.Width * face.Height;
                        double imageArea = image.Width * image.Height;
                        double faceRatio = faceArea / imageArea;
                        
                        // Too close if face takes up more than 60% of image
                        return faceRatio > 0.6;
                    }
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Жуда узоқ — слишком далеко
    private bool IsTooFar(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    if (faces.Length > 0)
                    {
                        Rectangle face = faces[0];
                        double faceArea = face.Width * face.Height;
                        double imageArea = image.Width * image.Height;
                        double faceRatio = faceArea / imageArea;
                        
                        // Too far if face takes up less than 5% of image
                        return faceRatio < 0.05;
                    }
                    return true; // If no face detected, likely too far
                }
            }
        }
        catch
        {
            return true;
        }
    }

    // Тепароқда — сверху
    private bool IsFromAbove(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    if (faces.Length > 0)
                    {
                        Rectangle face = faces[0];
                        double faceCenterY = face.Y + face.Height / 2.0;
                        double imageCenterY = image.Height / 2.0;
                        
                        // From above if face center is significantly above image center
                        return faceCenterY < imageCenterY - image.Height * 0.15;
                    }
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Пастроқда — снизу
    private bool IsFromBelow(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    if (faces.Length > 0)
                    {
                        Rectangle face = faces[0];
                        double faceCenterY = face.Y + face.Height / 2.0;
                        double imageCenterY = image.Height / 2.0;
                        
                        // From below if face center is significantly below image center
                        return faceCenterY > imageCenterY + image.Height * 0.15;
                    }
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Чапроқда — слева
    private bool IsFromLeft(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    if (faces.Length > 0)
                    {
                        Rectangle face = faces[0];
                        double faceCenterX = face.X + face.Width / 2.0;
                        double imageCenterX = image.Width / 2.0;
                        
                        // From left if face center is significantly to the left of image center
                        return faceCenterX < imageCenterX - image.Width * 0.15;
                    }
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Суръат аниқлиги/Sharpness — резкость изображения
    private bool IsImageSharpnessGood(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                Mat grayImage = new Mat();
                CvInvoke.CvtColor(image, grayImage, ColorConversion.Bgr2Gray);
                
                // Use Laplacian variance to measure sharpness
                Mat laplacian = new Mat();
                CvInvoke.Laplacian(grayImage, laplacian, DepthType.Cv64F);
                
                MCvScalar mean = CvInvoke.Mean(laplacian);
                Mat sqDiff = new Mat();
                Mat meanMat = new Mat(laplacian.Size, DepthType.Cv64F, 1);
                meanMat.SetTo(new MCvScalar(mean.V0));
                CvInvoke.Subtract(laplacian, meanMat, sqDiff);
                CvInvoke.Multiply(sqDiff, sqDiff, sqDiff);
                
                MCvScalar variance = CvInvoke.Mean(sqDiff);
                
                // Good sharpness if variance is above threshold
                return variance.V0 > 150;
            }
        }
        catch
        {
            return false;
        }
    }

    // Тўйинганлик — насыщенность
    private bool IsSaturationGood(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                Mat hsvImage = new Mat();
                CvInvoke.CvtColor(image, hsvImage, ColorConversion.Bgr2Hsv);
                
                VectorOfMat channels = new VectorOfMat();
                CvInvoke.Split(hsvImage, channels);
                
                // Check saturation channel (index 1)
                MCvScalar avgSaturation = CvInvoke.Mean(channels[1]);
                
                // Good saturation if within reasonable range
                return avgSaturation.V0 > 50 && avgSaturation.V0 < 200;
            }
        }
        catch
        {
            return false;
        }
    }

    // Фоннинг бир хиллиги — однообразность фона
    private bool IsBackgroundUniform(byte[] imageBytes)
    {
        try
        {
            using (Mat image = LoadImage(imageBytes))
            {
                using (CascadeClassifier faceCascade = LoadCascadeClassifier(faceFileName))
                {
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    Rectangle[] faces = faceCascade.DetectMultiScale(image, 1.1, 3, Size.Empty, Size.Empty);
                    
                    if (faces.Length > 0)
                    {
                        // Create mask to exclude face region
                        Mat mask = Mat.Ones(image.Rows, image.Cols, DepthType.Cv8U, 1);
                        Rectangle face = faces[0];
                        
                        // Expand face rectangle to exclude more
                        Rectangle expandedFace = new Rectangle(
                            Math.Max(0, face.X - face.Width / 4),
                            Math.Max(0, face.Y - face.Height / 4),
                            Math.Min(image.Width - face.X, face.Width + face.Width / 2),
                            Math.Min(image.Height - face.Y, face.Height + face.Height / 2)
                        );
                        
                        CvInvoke.Rectangle(mask, expandedFace, new MCvScalar(0), -1);
                        
                        // Calculate background variance
                        MCvScalar bgMean = CvInvoke.Mean(image, mask);
                        Mat bgDiff = new Mat();
                        Mat meanMat = new Mat(image.Size, DepthType.Cv8U, 1);
                        meanMat.SetTo(new MCvScalar(bgMean.V0));
                        CvInvoke.Subtract(image, meanMat, bgDiff);
                        CvInvoke.Multiply(bgDiff, bgDiff, bgDiff);
                        
                        MCvScalar bgVariance = CvInvoke.Mean(bgDiff, mask);
                        
                        // Uniform background has low variance
                        return bgVariance.V0 < 500;
                    }
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }
    }
}
