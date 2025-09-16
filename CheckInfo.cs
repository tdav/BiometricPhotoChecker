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
            IsFaceDetected = false;
            IsFacialExpressionNeutral = false;
            IsWithoutGlasses = false;
            IsNotBlinking = false;
            IsMouthClosed = false;
            IsNotLookingSideways = false;
            IsRedEyeAbsent = false;
            IsFaceWellLit = false;
            IsSkinTextureClear = false;
            IsColorNotFaded = false;
            IsNotPixelated = false;
            IsSkinTextureNatural = false;
            IsGlassesGlareAbsent = false;
            IsNotOverexposed = false;
            IsFaceOrientationFrontal = false;
            IsNotLookingUpOrDown = false;
            IsDistanceNotTooClose = false;
            IsDistanceNotTooFar = false;
            IsNotShotFromAbove = false;
            IsNotShotFromBelow = false;
            IsNotShotFromSide = false;
            IsSharp = false;
            IsSaturationBalanced = false;
            IsBackgroundUniform = false;
        }

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
        public bool IsFaceDetected { get; set; }
        public bool IsFacialExpressionNeutral { get; set; }
        public bool IsWithoutGlasses { get; set; }
        public bool IsNotBlinking { get; set; }
        public bool IsMouthClosed { get; set; }
        public bool IsNotLookingSideways { get; set; }
        public bool IsRedEyeAbsent { get; set; }
        public bool IsFaceWellLit { get; set; }
        public bool IsSkinTextureClear { get; set; }
        public bool IsColorNotFaded { get; set; }
        public bool IsNotPixelated { get; set; }
        public bool IsSkinTextureNatural { get; set; }
        public bool IsGlassesGlareAbsent { get; set; }
        public bool IsNotOverexposed { get; set; }
        public bool IsFaceOrientationFrontal { get; set; }
        public bool IsNotLookingUpOrDown { get; set; }
        public bool IsDistanceNotTooClose { get; set; }
        public bool IsDistanceNotTooFar { get; set; }
        public bool IsNotShotFromAbove { get; set; }
        public bool IsNotShotFromBelow { get; set; }
        public bool IsNotShotFromSide { get; set; }
        public bool IsSharp { get; set; }
        public bool IsSaturationBalanced { get; set; }
        public bool IsBackgroundUniform { get; set; }
    }
}
