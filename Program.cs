using BiometricPhotoChecker;
using System.IO;
using System.Reflection;

string photoPath = Path.Combine(Directory.GetParent(System.Environment.CurrentDirectory).Parent.Parent.FullName, "img", "biyo.png");
byte[] imageArray = File.ReadAllBytes(photoPath);

BiometricPhotoValidationService service = new BiometricPhotoValidationService();
CheckInfo result = service.ValidateBiometricPhoto(imageArray);

foreach (PropertyInfo property in typeof(CheckInfo).GetProperties(BindingFlags.Instance | BindingFlags.Public))
{
    object? value = property.GetValue(result);
    Console.WriteLine($"{property.Name} - {value}");
}

Console.ReadKey();

