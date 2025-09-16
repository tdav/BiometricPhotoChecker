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
            IsLookingSideways = false;
            HasRedEyeEffect = false;
            IsFaceIlluminated = false;
            IsFaceSkinClear = false;
            AreColorsFaded = false;
            IsPixelated = false;
            IsSkinTextureNatural = false;
            HasGlassesGlare = false;
            IsOverExposed = false;
            IsFaceTurnedSideways = false;
            IsLookingUpOrDown = false;
            IsTooCloseToCamera = false;
            IsTooFarFromCamera = false;
            IsCameraAboveFace = false;
            IsCameraBelowFace = false;
            IsCameraShiftedLeft = false;
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
        public bool IsLookingSideways { get; set; }
        public bool HasRedEyeEffect { get; set; }
        public bool IsFaceIlluminated { get; set; }
        public bool IsFaceSkinClear { get; set; }
        public bool AreColorsFaded { get; set; }
        public bool IsPixelated { get; set; }
        public bool IsSkinTextureNatural { get; set; }
        public bool HasGlassesGlare { get; set; }
        public bool IsOverExposed { get; set; }
        public bool IsFaceTurnedSideways { get; set; }
        public bool IsLookingUpOrDown { get; set; }
        public bool IsTooCloseToCamera { get; set; }
        public bool IsTooFarFromCamera { get; set; }
        public bool IsCameraAboveFace { get; set; }
        public bool IsCameraBelowFace { get; set; }
        public bool IsCameraShiftedLeft { get; set; }
        public bool IsImageSharp { get; set; }
        public bool IsSaturationAdequate { get; set; }
        public bool IsBackgroundUniform { get; set; }
    }
}