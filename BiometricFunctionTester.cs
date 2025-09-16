using BiometricPhotoChecker;
using BiometricPhotoChecker.Service;

namespace BiometricPhotoChecker.Tests
{
    /// <summary>
    /// Individual function testing utility
    /// This class provides methods to test specific biometric validation functions
    /// </summary>
    public class BiometricFunctionTester
    {
        private readonly BiometricPhotoValidationService _service;

        public BiometricFunctionTester()
        {
            _service = new BiometricPhotoValidationService();
        }

        /// <summary>
        /// Test a specific image against all biometric functions
        /// </summary>
        /// <param name="imagePath">Path to the image file</param>
        /// <returns>CheckInfo with all validation results</returns>
        public CheckInfo TestAllFunctions(string imagePath)
        {
            if (!File.Exists(imagePath))
                throw new FileNotFoundException($"Image file not found: {imagePath}");

            byte[] imageBytes = File.ReadAllBytes(imagePath);
            return _service.ValidateBiometricPhoto(imageBytes);
        }

        /// <summary>
        /// Test multiple images and generate a report
        /// </summary>
        /// <param name="imageDirectory">Directory containing test images</param>
        /// <returns>Dictionary with results for each image</returns>
        public Dictionary<string, CheckInfo> TestMultipleImages(string imageDirectory)
        {
            var results = new Dictionary<string, CheckInfo>();
            
            if (!Directory.Exists(imageDirectory))
                throw new DirectoryNotFoundException($"Directory not found: {imageDirectory}");

            string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };
            var imageFiles = Directory.GetFiles(imageDirectory)
                .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()))
                .ToArray();

            foreach (string imageFile in imageFiles)
            {
                try
                {
                    var result = TestAllFunctions(imageFile);
                    results[Path.GetFileName(imageFile)] = result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error testing {imageFile}: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Generate a detailed report for a single image
        /// </summary>
        /// <param name="imagePath">Path to the image file</param>
        /// <returns>Formatted report string</returns>
        public string GenerateDetailedReport(string imagePath)
        {
            var result = TestAllFunctions(imagePath);
            var report = new System.Text.StringBuilder();
            
            report.AppendLine($"=== Biometric Photo Validation Report ===");
            report.AppendLine($"Image: {Path.GetFileName(imagePath)}");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            
            report.AppendLine("EXISTING VALIDATIONS:");
            report.AppendLine($"  Image Valid: {result.IsImageValid}");
            report.AppendLine($"  Smile Valid: {result.IsSmileValid}");
            report.AppendLine($"  Mouth Valid: {result.IsMouthValid}");
            report.AppendLine($"  Nose Valid: {result.IsNoseValid}");
            report.AppendLine($"  Eyes Valid: {result.IsEyesValid}");
            report.AppendLine($"  Head/Shoulders Visible: {result.IsHeadAndShouldersVisible}");
            report.AppendLine($"  Background Valid: {result.IsBackgroundValid}");
            report.AppendLine($"  Proper Lighting: {result.IsProperLighting}");
            report.AppendLine($"  Face Position Valid: {result.IsFacePositionValid}");
            report.AppendLine($"  Eye Position Valid: {result.IsEyePositionValid}");
            
            report.AppendLine();
            report.AppendLine("NEW BIOMETRIC VALIDATIONS:");
            
            // Face Detection & Expression
            report.AppendLine("  FACE DETECTION & EXPRESSION:");
            report.AppendLine($"    Face Detected (Юз аниқланди): {result.IsFaceDetected}");
            report.AppendLine($"    Facial Expression Valid (Юз ифодаси): {result.IsFacialExpressionValid}");
            
            // Eyes & Glasses
            report.AppendLine("  EYES & GLASSES:");
            report.AppendLine($"    Eyes/Glasses Valid (Кўз кўзойнак): {result.AreEyesGlassesValid}");
            report.AppendLine($"    Eye Blinking Detected (Кўзни пирпирлатмоқ): {result.IsEyeBlinkingDetected}");
            report.AppendLine($"    Deer Eyes Detected (Кийик кўз): {result.IsDeerEyesDetected}");
            report.AppendLine($"    Glasses Effect Detected (Кўзойнак таъсири): {result.IsGlassesEffectDetected}");
            
            // Mouth & Expression
            report.AppendLine("  MOUTH & EXPRESSION:");
            report.AppendLine($"    Mouth Open Detected (Оғиз очиқ): {result.IsMouthOpenDetected}");
            
            // Gaze Direction
            report.AppendLine("  GAZE DIRECTION:");
            report.AppendLine($"    Looking Away (Бошқа тарафга қараш): {result.IsLookingAway}");
            report.AppendLine($"    Looking Up/Down (Тепа-пастга қараш): {result.IsLookingUpDown}");
            
            // Lighting & Exposure
            report.AppendLine("  LIGHTING & EXPOSURE:");
            report.AppendLine($"    Face Not Illuminated (Юз атрофига ёритилмаган): {result.IsFaceNotIlluminated}");
            report.AppendLine($"    Overexposed (Ёниб кетганлик): {result.IsOverexposed}");
            
            // Image Quality
            report.AppendLine("  IMAGE QUALITY:");
            report.AppendLine($"    Unclear Face Skin (Нотайин юз териси): {result.IsUnclearFaceSkin}");
            report.AppendLine($"    Colors Faded (Ранглар оғариб кетган): {result.AreColorsFaded}");
            report.AppendLine($"    Pixelized (Пикселлашиш): {result.IsPixelized}");
            report.AppendLine($"    Skin Texture Good (Тери буртсиқ қайтарилиши): {result.IsSkinTextureReturned}");
            report.AppendLine($"    Image Sharpness Good (Суръат аниқлиги): {result.IsImageSharpnessGood}");
            report.AppendLine($"    Saturation Good (Тўйинганлик): {result.IsSaturationGood}");
            
            // Position & Angle
            report.AppendLine("  POSITION & ANGLE:");
            report.AppendLine($"    Face Turned Sideways (Юзни енгга буриш): {result.IsFaceTurnedSideways}");
            report.AppendLine($"    Too Close (Жуда яқин): {result.IsTooClose}");
            report.AppendLine($"    Too Far (Жуда узоқ): {result.IsTooFar}");
            report.AppendLine($"    From Above (Тепароқда): {result.IsFromAbove}");
            report.AppendLine($"    From Below (Пастроқда): {result.IsFromBelow}");
            report.AppendLine($"    From Left (Чапроқда): {result.IsFromLeft}");
            
            // Background
            report.AppendLine("  BACKGROUND:");
            report.AppendLine($"    Background Uniform (Фоннинг бир хиллиги): {result.IsBackgroundUniform}");
            
            // Overall Assessment
            report.AppendLine();
            report.AppendLine("OVERALL ASSESSMENT:");
            bool isGoodForBiometric = EvaluateOverallQuality(result);
            report.AppendLine($"  Suitable for Biometric Use: {isGoodForBiometric}");
            
            if (!isGoodForBiometric)
            {
                report.AppendLine("  Issues Found:");
                var issues = GetIssuesList(result);
                foreach (var issue in issues)
                {
                    report.AppendLine($"    - {issue}");
                }
            }
            
            return report.ToString();
        }

        /// <summary>
        /// Evaluate overall photo quality for biometric use
        /// </summary>
        private bool EvaluateOverallQuality(CheckInfo result)
        {
            // Critical requirements
            if (!result.IsFaceDetected) return false;
            if (!result.IsFacialExpressionValid) return false;
            if (result.IsEyeBlinkingDetected) return false;
            if (result.IsMouthOpenDetected) return false;
            if (result.IsOverexposed) return false;
            if (result.IsFaceNotIlluminated) return false;
            if (result.IsFaceTurnedSideways) return false;
            if (result.IsTooClose || result.IsTooFar) return false;
            
            // Quality requirements
            if (result.IsPixelized) return false;
            if (result.IsUnclearFaceSkin) return false;
            if (!result.IsImageSharpnessGood) return false;
            
            return true;
        }

        /// <summary>
        /// Get list of issues with the photo
        /// </summary>
        private List<string> GetIssuesList(CheckInfo result)
        {
            var issues = new List<string>();
            
            if (!result.IsFaceDetected) issues.Add("No face detected");
            if (!result.IsFacialExpressionValid) issues.Add("Invalid facial expression");
            if (result.IsEyeBlinkingDetected) issues.Add("Eyes are blinking/closed");
            if (result.IsMouthOpenDetected) issues.Add("Mouth is open");
            if (result.IsLookingAway) issues.Add("Person is looking away");
            if (result.IsLookingUpDown) issues.Add("Person is looking up or down");
            if (result.IsFaceNotIlluminated) issues.Add("Face is poorly illuminated");
            if (result.IsOverexposed) issues.Add("Image is overexposed");
            if (result.IsUnclearFaceSkin) issues.Add("Face skin is unclear/blurred");
            if (result.AreColorsFaded) issues.Add("Colors are faded");
            if (result.IsPixelized) issues.Add("Image is pixelized");
            if (!result.IsSkinTextureReturned) issues.Add("Poor skin texture quality");
            if (result.IsGlassesEffectDetected) issues.Add("Glasses reflection detected");
            if (result.IsFaceTurnedSideways) issues.Add("Face is turned sideways");
            if (result.IsTooClose) issues.Add("Face is too close to camera");
            if (result.IsTooFar) issues.Add("Face is too far from camera");
            if (result.IsFromAbove) issues.Add("Photo taken from above");
            if (result.IsFromBelow) issues.Add("Photo taken from below");
            if (result.IsFromLeft) issues.Add("Photo taken from left side");
            if (!result.IsImageSharpnessGood) issues.Add("Poor image sharpness");
            if (!result.IsSaturationGood) issues.Add("Poor color saturation");
            if (!result.IsBackgroundUniform) issues.Add("Background is not uniform");
            
            return issues;
        }
    }
}