namespace BiometricPhotoChecker
{
    public class CheckInfo 
    { 
        public CheckInfo() 
        { 
            IsImageValid = false;
            IsSmileValid = false;
            IsMouthValid = false;
            IsNoseValid = false;
            IsEyesValid = false;
            IsHeadAndShouldersVisible = false;
            IsBackgroundValid = false;
            IsProperLighting = false;
            IsFacePositionValid = false;
            IsEyePositionValid = false;
            
            // New properties for biometric photo checking
            IsFaceDetected = false; // Юз аниқланди — лицо определено
            IsFacialExpressionValid = false; // Юз ифодаси — выражение лица
            AreEyesGlassesValid = false; // Кўз кўзойнак — глаза (очки)
            IsEyeBlinkingDetected = false; // Кўзни пирпирлатмоқ — моргание
            IsMouthOpenDetected = false; // Оғиз очиқ — рот открыт
            IsLookingAway = false; // Бошқа тарафга қараш — взгляд в сторону
            IsDeerEyesDetected = false; // Кийик кўз — взгляд (букв. «олений глаз»)
            IsFaceNotIlluminated = false; // Юз атрофига ёритилмаган — лицо не освещено
            IsUnclearFaceSkin = false; // Нотайин юз териси — нечеткая кожа лица
            AreColorsFaded = false; // Ранглар оғариб кетган — цвета поблекли
            IsPixelized = false; // Пикселлашиш — пикселизация
            IsSkinTextureReturned = false; // Тери буртсиқ қайтарилиши — возврат кожной текстуры
            IsGlassesEffectDetected = false; // Кўзойнак таъсири — эффект очков
            IsOverexposed = false; // Ёниб кетганлик — пересвет (яркое освещение)
            IsFaceTurnedSideways = false; // Юзни енгга буриш — поворот лица вбок
            IsLookingUpDown = false; // Тепа-пастга қараш — взгляд вверх-вниз
            IsTooClose = false; // Жуда яқин — слишком близко
            IsTooFar = false; // Жуда узоқ — слишком далеко
            IsFromAbove = false; // Тепароқда — сверху
            IsFromBelow = false; // Пастроқда — снизу
            IsFromLeft = false; // Чапроқда — слева
            IsImageSharpnessGood = false; // Суръат аниқлиги/Sharpness — резкость изображения
            IsSaturationGood = false; // Тўйинганлик — насыщенность
            IsBackgroundUniform = false; // Фоннинг бир хиллиги — однообразность фона
        } 
        
        // Existing properties
        public bool IsImageValid { get; set; }
        public bool IsSmileValid { get; set; }
        public bool IsMouthValid { get; set; }
        public bool IsNoseValid { get; set; }
        public bool IsEyesValid { get; set; }
        public bool IsHeadAndShouldersVisible { get; set; }
        public bool IsBackgroundValid { get; set; }
        public bool IsProperLighting { get; set; }
        public bool IsFacePositionValid { get; set; }
        public bool IsEyePositionValid { get; set; }
        
        // New biometric photo checking properties
        public bool IsFaceDetected { get; set; } // Юз аниқланди — лицо определено
        public bool IsFacialExpressionValid { get; set; } // Юз ифодаси — выражение лица
        public bool AreEyesGlassesValid { get; set; } // Кўз кўзойнак — глаза (очки)
        public bool IsEyeBlinkingDetected { get; set; } // Кўзни пирпирлатмоқ — моргание
        public bool IsMouthOpenDetected { get; set; } // Оғиз очиқ — рот открыт
        public bool IsLookingAway { get; set; } // Бошқа тарафга қараш — взгляд в сторону
        public bool IsDeerEyesDetected { get; set; } // Кийик кўз — взгляд (букв. «олений глаз»)
        public bool IsFaceNotIlluminated { get; set; } // Юз атрофига ёритилмаган — лицо не освещено
        public bool IsUnclearFaceSkin { get; set; } // Нотайин юз териси — нечеткая кожа лица
        public bool AreColorsFaded { get; set; } // Ранглар оғариб кетган — цвета поблекли
        public bool IsPixelized { get; set; } // Пикселлашиш — пикселизация
        public bool IsSkinTextureReturned { get; set; } // Тери буртсиқ қайтарилиши — возврат кожной текстуры
        public bool IsGlassesEffectDetected { get; set; } // Кўзойнак таъсири — эффект очков
        public bool IsOverexposed { get; set; } // Ёниб кетганлик — пересвет (яркое освещение)
        public bool IsFaceTurnedSideways { get; set; } // Юзни енгга буриш — поворот лица вбок
        public bool IsLookingUpDown { get; set; } // Тепа-пастга қараш — взгляд вверх-вниз
        public bool IsTooClose { get; set; } // Жуда яқин — слишком близко
        public bool IsTooFar { get; set; } // Жуда узоқ — слишком далеко
        public bool IsFromAbove { get; set; } // Тепароқда — сверху
        public bool IsFromBelow { get; set; } // Пастроқда — снизу
        public bool IsFromLeft { get; set; } // Чапроқда — слева
        public bool IsImageSharpnessGood { get; set; } // Суръат аниқлиги/Sharpness — резкость изображения
        public bool IsSaturationGood { get; set; } // Тўйинганлик — насыщенность
        public bool IsBackgroundUniform { get; set; } // Фоннинг бир хиллиги — однообразность фона
    }
}