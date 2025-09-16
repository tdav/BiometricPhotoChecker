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
            IsExpressionNeutral = false;
            HasGlasses = false;
            IsBlinking = false;
            IsMouthOpen = false;
            IsLookingAway = false;
            HasRedEyeEffect = false;
            IsFaceIlluminated = false;
            IsSkinTextureClear = false;
            HasColorFading = false;
            HasPixelation = false;
            HasSkinRetouching = false;
            HasGlassesGlare = false;
            HasOverExposure = false;
            IsFaceTurnedSideways = false;
            IsLookingUpOrDown = false;
            IsTooClose = false;
            IsTooFar = false;
            IsCameraAngleAbove = false;
            IsCameraAngleBelow = false;
            IsCameraAngleLeft = false;
            IsImageSharp = false;
            IsSaturationAdequate = false;
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
        public bool IsExpressionNeutral { get; set; }
        public bool HasGlasses { get; set; }
        public bool IsBlinking { get; set; }
        public bool IsMouthOpen { get; set; }
        public bool IsLookingAway { get; set; }
        public bool HasRedEyeEffect { get; set; }
        public bool IsFaceIlluminated { get; set; }
        public bool IsSkinTextureClear { get; set; }
        public bool HasColorFading { get; set; }
        public bool HasPixelation { get; set; }
        public bool HasSkinRetouching { get; set; }
        public bool HasGlassesGlare { get; set; }
        public bool HasOverExposure { get; set; }
        public bool IsFaceTurnedSideways { get; set; }
        public bool IsLookingUpOrDown { get; set; }
        public bool IsTooClose { get; set; }
        public bool IsTooFar { get; set; }
        public bool IsCameraAngleAbove { get; set; }
        public bool IsCameraAngleBelow { get; set; }
        public bool IsCameraAngleLeft { get; set; }
        public bool IsImageSharp { get; set; }
        public bool IsSaturationAdequate { get; set; }
        public bool IsBackgroundUniform { get; set; }
    }
}
