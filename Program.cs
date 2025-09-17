using BiometricPhotoChecker;
using System.IO;
using System.Reflection; 

internal class Program
{
    private static void Main(string[] args)
    {
        string photoPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "img", "bad", "img_bad_attire_12.jpg");
        byte[] imageArray = File.ReadAllBytes(photoPath);

        BiometricPhotoValidationService service = new BiometricPhotoValidationService();
        CheckInfo result = service.ValidateBiometricPhoto(imageArray);

        foreach (PropertyInfo property in typeof(CheckInfo).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            object? value = property.GetValue(result);
            Console.WriteLine($"{GetName(property.Name)} - {value}");
        }

        Console.ReadKey();
    }

    private static string GetName(string s)
    {
        switch (s)
        {
            case "IsImageValid": return "Изображение корректное"; break;
            case "IsSmileValid": return "Улыбка допустима"; break;
            case "IsMouthValid": return "Рот допустим"; break;
            case "IsNoseValid": return "Нос допустим"; break;
            case "IsEyesValid": return "Глаза допустимы"; break;
            case "IsHeadAndShouldersVisible": return "Голова и плечи видны"; break;
            case "IsBackgroundValid": return "Фон допустим"; break;
            case "IsProperLighting": return "Освещение корректное"; break;
            case "IsFacePositionValid": return "Положение лица корректное"; break;
            case "IsEyePositionValid": return "Положение глаз корректное"; break;
            case "IsFaceDetected": return "Лицо обнаружено"; break;
            case "IsExpressionNeutral": return "Выражение лица нейтральное"; break;
            case "HasGlasses": return "Очки"; break;
            case "IsBlinking": return "Моргает"; break;
            case "IsMouthOpen": return "Рот открыт"; break;
            case "IsLookingAway": return "Смотрит в сторону"; break;
            case "HasRedEyeEffect": return "Эффект красных глаз"; break;
            case "IsFaceIlluminated": return "Лицо освещено"; break;
            case "IsSkinTextureClear": return "Текстура кожи чёткая"; break;
            case "HasColorFading": return "Выцветание цветов"; break;
            case "HasPixelation": return "Пикселизация"; break;
            case "HasSkinRetouching": return "Ретушь кожи"; break;
            case "HasGlassesGlare": return "Блик от очков"; break;
            case "HasOverExposure": return "Пересвет"; break;
            case "IsFaceTurnedSideways": return "Лицо повернуто вбок"; break;
            case "IsLookingUpOrDown": return "Смотрит вверх или вниз"; break;
            case "IsTooClose": return "Слишком близко"; break;
            case "IsTooFar": return "Слишком далеко"; break;
            case "IsCameraAngleAbove": return "Камера сверху"; break;
            case "IsCameraAngleBelow": return "Камера снизу"; break;
            case "IsCameraAngleLeft": return "Камера слева"; break;
            case "IsImageSharp": return "Изображение резкое"; break;
            case "IsSaturationAdequate": return "Насыщенность достаточная"; break;
            case "IsBackgroundUniform": return "Фон однородный"; break;
            default: return "";
        }
    }
}