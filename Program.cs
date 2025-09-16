using BiometricPhotoChecker;

string photoPath = Path.Combine(System.Environment.CurrentDirectory, "img", "biyo.png");
byte[] imageArray = System.IO.File.ReadAllBytes(photoPath);

BiometricPhotoValidationService service = new BiometricPhotoValidationService();
CheckInfo result = service.ValidateBiometricPhoto(imageArray);

// Existing checks
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

// New biometric checks
Console.WriteLine(nameof(result.IsFaceDetected) + " (Юз аниқланди) - " + result.IsFaceDetected);
Console.WriteLine(nameof(result.IsFacialExpressionValid) + " (Юз ифодаси) - " + result.IsFacialExpressionValid);
Console.WriteLine(nameof(result.AreEyesGlassesValid) + " (Кўз кўзойнак) - " + result.AreEyesGlassesValid);
Console.WriteLine(nameof(result.IsEyeBlinkingDetected) + " (Кўзни пирпирлатмоқ) - " + result.IsEyeBlinkingDetected);
Console.WriteLine(nameof(result.IsMouthOpenDetected) + " (Оғиз очиқ) - " + result.IsMouthOpenDetected);
Console.WriteLine(nameof(result.IsLookingAway) + " (Бошқа тарафга қараш) - " + result.IsLookingAway);
Console.WriteLine(nameof(result.IsDeerEyesDetected) + " (Кийик кўз) - " + result.IsDeerEyesDetected);
Console.WriteLine(nameof(result.IsFaceNotIlluminated) + " (Юз атрофига ёритилмаган) - " + result.IsFaceNotIlluminated);
Console.WriteLine(nameof(result.IsUnclearFaceSkin) + " (Нотайин юз териси) - " + result.IsUnclearFaceSkin);
Console.WriteLine(nameof(result.AreColorsFaded) + " (Ранглар оғариб кетган) - " + result.AreColorsFaded);
Console.WriteLine(nameof(result.IsPixelized) + " (Пикселлашиш) - " + result.IsPixelized);
Console.WriteLine(nameof(result.IsSkinTextureReturned) + " (Тери буртсиқ қайтарилиши) - " + result.IsSkinTextureReturned);
Console.WriteLine(nameof(result.IsGlassesEffectDetected) + " (Кўзойнак таъсири) - " + result.IsGlassesEffectDetected);
Console.WriteLine(nameof(result.IsOverexposed) + " (Ёниб кетганлик) - " + result.IsOverexposed);
Console.WriteLine(nameof(result.IsFaceTurnedSideways) + " (Юзни енгга буриш) - " + result.IsFaceTurnedSideways);
Console.WriteLine(nameof(result.IsLookingUpDown) + " (Тепа-пастга қараш) - " + result.IsLookingUpDown);
Console.WriteLine(nameof(result.IsTooClose) + " (Жуда яқин) - " + result.IsTooClose);
Console.WriteLine(nameof(result.IsTooFar) + " (Жуда узоқ) - " + result.IsTooFar);
Console.WriteLine(nameof(result.IsFromAbove) + " (Тепароқда) - " + result.IsFromAbove);
Console.WriteLine(nameof(result.IsFromBelow) + " (Пастроқда) - " + result.IsFromBelow);
Console.WriteLine(nameof(result.IsFromLeft) + " (Чапроқда) - " + result.IsFromLeft);
Console.WriteLine(nameof(result.IsImageSharpnessGood) + " (Суръат аниқлиги) - " + result.IsImageSharpnessGood);
Console.WriteLine(nameof(result.IsSaturationGood) + " (Тўйинганлик) - " + result.IsSaturationGood);
Console.WriteLine(nameof(result.IsBackgroundUniform) + " (Фоннинг бир хиллиги) - " + result.IsBackgroundUniform);

Console.ReadKey();