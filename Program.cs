using System;
using System.IO;
using BiometricPhotoChecker;

string photoPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "img", "biyo.png");
byte[] imageArray = File.ReadAllBytes(photoPath);

BiometricPhotoValidationService service = new BiometricPhotoValidationService();
CheckInfo result = service.ValidateBiometricPhoto(imageArray);

Console.WriteLine(nameof(result.IsImageValid) + " - " + result.IsImageValid);
Console.WriteLine(nameof(result.IsSmileValid) + " - " + result.IsSmileValid);
Console.WriteLine(nameof(result.IsMouthValid) + " - " + result.IsMouthValid);
Console.WriteLine(nameof(result.IsNoseValid) + " - " + result.IsNoseValid);
Console.WriteLine(nameof(result.IsEyesValid) + " - " + result.IsEyesValid);
Console.WriteLine(nameof(result.IsHeadAndShouldersVisible) + " - " + result.IsHeadAndShouldersVisible);
Console.WriteLine(nameof(result.IsBackgroundValid) + " - " + result.IsBackgroundValid);
Console.WriteLine(nameof(result.IsProperLighting) + " - " + result.IsProperLighting);
Console.WriteLine(nameof(result.IsFacePositionValid) + " - " + result.IsFacePositionValid);
Console.WriteLine(nameof(result.IsEyePositionValid) + " - " + result.IsEyePositionValid);
Console.WriteLine(nameof(result.IsFaceDetected) + " - " + result.IsFaceDetected);
Console.WriteLine(nameof(result.IsExpressionNeutral) + " - " + result.IsExpressionNeutral);
Console.WriteLine(nameof(result.HasGlasses) + " - " + result.HasGlasses);
Console.WriteLine(nameof(result.IsBlinking) + " - " + result.IsBlinking);
Console.WriteLine(nameof(result.IsMouthOpen) + " - " + result.IsMouthOpen);
Console.WriteLine(nameof(result.IsLookingSideways) + " - " + result.IsLookingSideways);
Console.WriteLine(nameof(result.HasRedEyeEffect) + " - " + result.HasRedEyeEffect);
Console.WriteLine(nameof(result.IsFaceIlluminated) + " - " + result.IsFaceIlluminated);
Console.WriteLine(nameof(result.IsFaceSkinClear) + " - " + result.IsFaceSkinClear);
Console.WriteLine(nameof(result.AreColorsFaded) + " - " + result.AreColorsFaded);
Console.WriteLine(nameof(result.IsPixelated) + " - " + result.IsPixelated);
Console.WriteLine(nameof(result.IsSkinTextureNatural) + " - " + result.IsSkinTextureNatural);
Console.WriteLine(nameof(result.HasGlassesGlare) + " - " + result.HasGlassesGlare);
Console.WriteLine(nameof(result.IsOverExposed) + " - " + result.IsOverExposed);
Console.WriteLine(nameof(result.IsFaceTurnedSideways) + " - " + result.IsFaceTurnedSideways);
Console.WriteLine(nameof(result.IsLookingUpOrDown) + " - " + result.IsLookingUpOrDown);
Console.WriteLine(nameof(result.IsTooCloseToCamera) + " - " + result.IsTooCloseToCamera);
Console.WriteLine(nameof(result.IsTooFarFromCamera) + " - " + result.IsTooFarFromCamera);
Console.WriteLine(nameof(result.IsCameraAboveFace) + " - " + result.IsCameraAboveFace);
Console.WriteLine(nameof(result.IsCameraBelowFace) + " - " + result.IsCameraBelowFace);
Console.WriteLine(nameof(result.IsCameraShiftedLeft) + " - " + result.IsCameraShiftedLeft);
Console.WriteLine(nameof(result.IsImageSharp) + " - " + result.IsImageSharp);
Console.WriteLine(nameof(result.IsSaturationAdequate) + " - " + result.IsSaturationAdequate);
Console.WriteLine(nameof(result.IsBackgroundUniform) + " - " + result.IsBackgroundUniform);

Console.ReadKey();
