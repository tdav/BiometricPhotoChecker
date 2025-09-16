# Biometric Photo Checking Functions

This document describes all the biometric photo validation functions implemented in the BiometricPhotoValidationService.

## Existing Functions

### Basic Image Validation
- **IsImageValid**: Checks if image dimensions meet minimum requirements (35x45 pixels)
- **IsSmileValid**: Validates that no excessive smile is detected (neutral expression preferred)
- **IsMouthValid**: Ensures mouth is detected in the image
- **IsNoseValid**: Ensures nose is detected in the image
- **IsEyesValid**: Validates that at least 2 eyes are detected
- **IsHeadAndShouldersVisible**: Checks if head and shoulders are properly visible
- **IsBackgroundValid**: Validates background color is light (RGB values ≥ 200)
- **IsProperLighting**: Checks if average brightness is within acceptable range (80-240)
- **IsFacePositionValid**: Validates face is centered in the image
- **IsEyePositionValid**: Ensures eyes are properly aligned and positioned

## New Biometric Functions

### 1. IsFaceDetected (Юз аниқланди — лицо определено)
**Purpose**: Detects if a face is present in the image
**Algorithm**: Uses Haar cascade face detection
**Returns**: True if at least one face is detected

### 2. IsFacialExpressionValid (Юз ифодаси — выражение лица)
**Purpose**: Validates neutral facial expression
**Algorithm**: Combines face detection with smile detection; valid if face detected without excessive smile
**Returns**: True if face has neutral expression

### 3. AreEyesGlassesValid (Кўз кўзойнак — глаза (очки))
**Purpose**: Checks for glasses interference or reflections
**Algorithm**: Uses eye-glasses cascade classifier to detect glasses artifacts
**Returns**: True if no glasses interference detected

### 4. IsEyeBlinkingDetected (Кўзни пирпирлатмоқ — моргание)
**Purpose**: Detects if eyes are blinking (eyes closed)
**Algorithm**: Checks if fewer than 2 eyes are detected (possible blinking)
**Returns**: True if blinking detected (should be false for valid photos)

### 5. IsMouthOpenDetected (Оғиз очиқ — рот открыт)
**Purpose**: Detects if mouth is open
**Algorithm**: Analyzes mouth region dimensions; open if height > 60% of width
**Returns**: True if open mouth detected (should be false for valid photos)

### 6. IsLookingAway (Бошқа тарафга қараш — взгляд в сторону)
**Purpose**: Detects if person is looking away from camera
**Algorithm**: Compares eye center position with face center; looking away if offset > 15%
**Returns**: True if looking away (should be false for valid photos)

### 7. IsDeerEyesDetected (Кийик кўз — взгляд)
**Purpose**: Detects unusual eye characteristics (deer-like wide-set eyes)
**Algorithm**: Measures distance between eyes relative to eye size; deer-like if distance > 3x eye size
**Returns**: True if deer-like eyes detected

### 8. IsFaceNotIlluminated (Юз атрофига ёритилмаган — лицо не освещено)
**Purpose**: Detects poor face illumination
**Algorithm**: Measures average brightness in face region; poorly lit if < 60
**Returns**: True if face is poorly illuminated (should be false for valid photos)

### 9. IsUnclearFaceSkin (Нотайин юз териси — нечеткая кожа лица)
**Purpose**: Detects unclear or blurred face skin
**Algorithm**: Uses Laplacian variance to measure face region sharpness; unclear if variance < 100
**Returns**: True if face skin is unclear (should be false for valid photos)

### 10. AreColorsFaded (Ранглар оғариб кетган — цвета поблекли)
**Purpose**: Detects faded colors in the image
**Algorithm**: Analyzes HSV saturation channel; faded if average saturation < 80
**Returns**: True if colors are faded (should be false for valid photos)

### 11. IsPixelized (Пикселлашиш — пикселизация)
**Purpose**: Detects image pixelization
**Algorithm**: Uses Canny edge detection; pixelized if edge ratio > 15%
**Returns**: True if image is pixelized (should be false for valid photos)

### 12. IsSkinTextureReturned (Тери буртсиқ қайтарилиши — возврат кожной текстуры)
**Purpose**: Validates skin texture quality
**Algorithm**: Compares original face with blurred version; good texture if difference > 10
**Returns**: True if good skin texture is present

### 13. IsGlassesEffectDetected (Кўзойнак таъсири — эффект очков)
**Purpose**: Detects glasses reflections or optical effects
**Algorithm**: Uses HoughCircles to detect circular reflections; effect if > 2 circles
**Returns**: True if glasses effects detected (should be false for valid photos)

### 14. IsOverexposed (Ёниб кетганлик — пересвет)
**Purpose**: Detects image overexposure
**Algorithm**: Checks average brightness > 240 or > 10% bright pixels (>245)
**Returns**: True if image is overexposed (should be false for valid photos)

### 15. IsFaceTurnedSideways (Юзни енгга буриш — поворот лица вбок)
**Purpose**: Detects if face is turned sideways
**Algorithm**: Compares frontal vs. profile face detection; sideways if profile detected without frontal
**Returns**: True if face is turned sideways (should be false for valid photos)

### 16. IsLookingUpDown (Тепа-пастга қараш — взгляд вверх-вниз)
**Purpose**: Detects if person is looking up or down
**Algorithm**: Compares eye vertical position with expected face center; looking up/down if offset > 20%
**Returns**: True if looking up/down (should be false for valid photos)

### 17. IsTooClose (Жуда яқин — слишком близко)
**Purpose**: Detects if face is too close to camera
**Algorithm**: Measures face area ratio; too close if face > 60% of image area
**Returns**: True if too close (should be false for valid photos)

### 18. IsTooFar (Жуда узоқ — слишком далеко)
**Purpose**: Detects if face is too far from camera
**Algorithm**: Measures face area ratio; too far if face < 5% of image area
**Returns**: True if too far (should be false for valid photos)

### 19. IsFromAbove (Тепароқда — сверху)
**Purpose**: Detects if photo is taken from above
**Algorithm**: Compares face center with image center; from above if face center > 15% above image center
**Returns**: True if taken from above (should be false for valid photos)

### 20. IsFromBelow (Пастроқда — снизу)
**Purpose**: Detects if photo is taken from below
**Algorithm**: Compares face center with image center; from below if face center > 15% below image center
**Returns**: True if taken from below (should be false for valid photos)

### 21. IsFromLeft (Чапроқда — слева)
**Purpose**: Detects if photo is taken from left side
**Algorithm**: Compares face center with image center; from left if face center > 15% left of image center
**Returns**: True if taken from left (should be false for valid photos)

### 22. IsImageSharpnessGood (Суръат аниқлиги — резкость изображения)
**Purpose**: Validates image sharpness quality
**Algorithm**: Uses Laplacian variance to measure overall sharpness; good if variance > 150
**Returns**: True if image has good sharpness

### 23. IsSaturationGood (Тўйинганлик — насыщенность)
**Purpose**: Validates color saturation quality
**Algorithm**: Analyzes HSV saturation; good if between 50-200
**Returns**: True if saturation is good

### 24. IsBackgroundUniform (Фоннинг бир хиллиги — однообразность фона)
**Purpose**: Validates background uniformity
**Algorithm**: Calculates background variance excluding face region; uniform if variance < 500
**Returns**: True if background is uniform

## Usage Example

```csharp
byte[] imageBytes = File.ReadAllBytes("photo.jpg");
BiometricPhotoValidationService service = new BiometricPhotoValidationService();
CheckInfo result = service.ValidateBiometricPhoto(imageBytes);

// Check individual results
bool isValidForBiometric = result.IsFaceDetected && 
                          result.IsFacialExpressionValid && 
                          !result.IsEyeBlinkingDetected &&
                          !result.IsMouthOpenDetected &&
                          !result.IsOverexposed &&
                          result.IsImageSharpnessGood;
```

## Error Handling

All functions include try-catch blocks and return appropriate default values:
- Detection functions return `false` if an error occurs
- Quality validation functions return `false` if unable to assess
- This ensures the application remains stable even with problematic images

## Requirements

- OpenCV native libraries (EmguCV package handles this)
- Haar cascade XML files for face, eye, smile, mouth, and nose detection
- Images should be in common formats (PNG, JPEG, etc.)