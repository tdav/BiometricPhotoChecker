using BiometricPhotoChecker;
using BiometricPhotoChecker.Tests;

namespace BiometricPhotoChecker.Examples
{
    /// <summary>
    /// Example program demonstrating how to test biometric photo validation functions
    /// </summary>
    public class TestingExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Biometric Photo Checker Testing Example ===\n");

            try
            {
                var tester = new BiometricFunctionTester();

                // Test 1: Single image detailed report
                Console.WriteLine("TEST 1: Single Image Analysis");
                Console.WriteLine("==============================");
                
                string singleImagePath = Path.Combine(System.Environment.CurrentDirectory, "img", "biyo.png");
                if (File.Exists(singleImagePath))
                {
                    string report = tester.GenerateDetailedReport(singleImagePath);
                    Console.WriteLine(report);
                }
                else
                {
                    Console.WriteLine($"Test image not found: {singleImagePath}");
                }

                Console.WriteLine("\n" + new string('=', 50) + "\n");

                // Test 2: Multiple images testing
                Console.WriteLine("TEST 2: Multiple Images Analysis");
                Console.WriteLine("=================================");
                
                string imageDirectory = Path.Combine(System.Environment.CurrentDirectory, "img");
                if (Directory.Exists(imageDirectory))
                {
                    var results = tester.TestMultipleImages(imageDirectory);
                    
                    Console.WriteLine($"Tested {results.Count} images:\n");
                    
                    foreach (var kvp in results)
                    {
                        string imageName = kvp.Key;
                        CheckInfo result = kvp.Value;
                        
                        Console.WriteLine($"Image: {imageName}");
                        Console.WriteLine($"  Face Detected: {result.IsFaceDetected}");
                        Console.WriteLine($"  Valid Expression: {result.IsFacialExpressionValid}");
                        Console.WriteLine($"  Good Quality: {result.IsImageSharpnessGood}");
                        Console.WriteLine($"  Proper Lighting: {result.IsProperLighting}");
                        Console.WriteLine($"  No Blinking: {!result.IsEyeBlinkingDetected}");
                        Console.WriteLine($"  Mouth Closed: {!result.IsMouthOpenDetected}");
                        
                        // Quick quality assessment
                        bool isGoodPhoto = result.IsFaceDetected && 
                                         result.IsFacialExpressionValid && 
                                         !result.IsEyeBlinkingDetected &&
                                         !result.IsMouthOpenDetected &&
                                         !result.IsOverexposed &&
                                         result.IsImageSharpnessGood;
                        
                        Console.WriteLine($"  Overall Assessment: {(isGoodPhoto ? "SUITABLE" : "NEEDS IMPROVEMENT")}");
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine($"Image directory not found: {imageDirectory}");
                }

                Console.WriteLine("\n" + new string('=', 50) + "\n");

                // Test 3: Function-specific testing example
                Console.WriteLine("TEST 3: Function-Specific Testing");
                Console.WriteLine("==================================");
                
                if (File.Exists(singleImagePath))
                {
                    // Test individual functions
                    var service = new BiometricPhotoValidationService();
                    byte[] imageBytes = File.ReadAllBytes(singleImagePath);
                    var result = service.ValidateBiometricPhoto(imageBytes);
                    
                    Console.WriteLine("Testing specific functions:");
                    Console.WriteLine($"  Face Detection (Юз аниқланди): {result.IsFaceDetected}");
                    Console.WriteLine($"  Eye Blinking (Кўзни пирпирлатмоқ): {result.IsEyeBlinkingDetected}");
                    Console.WriteLine($"  Mouth Open (Оғиз очиқ): {result.IsMouthOpenDetected}");
                    Console.WriteLine($"  Looking Away (Бошқа тарафга қараш): {result.IsLookingAway}");
                    Console.WriteLine($"  Overexposed (Ёниб кетганлик): {result.IsOverexposed}");
                    Console.WriteLine($"  Too Close (Жуда яқин): {result.IsTooClose}");
                    Console.WriteLine($"  Too Far (Жуда узоқ): {result.IsTooFar}");
                    Console.WriteLine($"  Image Sharpness (Суръат аниқлиги): {result.IsImageSharpnessGood}");
                    Console.WriteLine($"  Background Uniform (Фоннинг бир хиллиги): {result.IsBackgroundUniform}");
                }

                Console.WriteLine("\n" + new string('=', 50) + "\n");

                // Test 4: Performance testing
                Console.WriteLine("TEST 4: Performance Testing");
                Console.WriteLine("============================");
                
                if (File.Exists(singleImagePath))
                {
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    
                    int iterations = 10;
                    for (int i = 0; i < iterations; i++)
                    {
                        tester.TestAllFunctions(singleImagePath);
                    }
                    
                    stopwatch.Stop();
                    double avgTime = stopwatch.ElapsedMilliseconds / (double)iterations;
                    
                    Console.WriteLine($"Performance Test Results:");
                    Console.WriteLine($"  Iterations: {iterations}");
                    Console.WriteLine($"  Total Time: {stopwatch.ElapsedMilliseconds}ms");
                    Console.WriteLine($"  Average Time per Image: {avgTime:F2}ms");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during testing: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nTesting completed. Press any key to exit...");
            Console.ReadKey();
        }
    }
}