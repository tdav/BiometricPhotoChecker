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
            AreEyesOpen = false;
            IsMouthClosed = false;
            IsGazeStraightAhead = false;
            IsRedEyeAbsent = false;
            IsFaceWellLit = false;
            IsSkinTextureClear = false;
            AreColorsNotFaded = false;
            IsNotPixelated = false;
            IsSkinTextureNatural = false;
            IsGlareAbsent = false;
            IsNotOverexposed = false;
            IsFaceNotTurnedSideways = false;
            IsGazeLevel = false;
            IsNotTooClose = false;
            IsNotTooFar = false;
            IsFaceNotTooHigh = false;
            IsFaceNotTooLow = false;
            IsFaceNotTooLeft = false;
            IsImageSharp = false;
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
        public bool AreEyesOpen { get; set; }
        public bool IsMouthClosed { get; set; }
        public bool IsGazeStraightAhead { get; set; }
        public bool IsRedEyeAbsent { get; set; }
        public bool IsFaceWellLit { get; set; }
        public bool IsSkinTextureClear { get; set; }
        public bool AreColorsNotFaded { get; set; }
        public bool IsNotPixelated { get; set; }
        public bool IsSkinTextureNatural { get; set; }
        public bool IsGlareAbsent { get; set; }
        public bool IsNotOverexposed { get; set; }
        public bool IsFaceNotTurnedSideways { get; set; }
        public bool IsGazeLevel { get; set; }
        public bool IsNotTooClose { get; set; }
        public bool IsNotTooFar { get; set; }
        public bool IsFaceNotTooHigh { get; set; }
        public bool IsFaceNotTooLow { get; set; }
        public bool IsFaceNotTooLeft { get; set; }
        public bool IsImageSharp { get; set; }
        public bool IsSaturationBalanced { get; set; }
        public bool IsBackgroundUniform { get; set; }
    }
}
