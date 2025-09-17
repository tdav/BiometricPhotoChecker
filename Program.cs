using BiometricPhotoChecker;
using System.Linq;

string photoPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName, "img", "biyo.png");
byte[] imageArray = File.ReadAllBytes(photoPath);

BiometricPhotoValidationService service = new BiometricPhotoValidationService();
CheckInfo result = service.ValidateBiometricPhoto(imageArray);

foreach (var property in typeof(CheckInfo).GetProperties().OrderBy(p => p.Name))
{
    Console.WriteLine($"{property.Name} - {property.GetValue(result)}");
}

Console.ReadKey();
