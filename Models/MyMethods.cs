using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace System
{
    
    public static class MyMethods
    {
        public static bool QuickValid(this object _object) {
			return Validator.TryValidateObject(_object, new ValidationContext(_object), new List<ValidationResult>(), true);
            
        }
        public static bool QuickValid(this object _object,string variableName)
        {
            var results = new List<ValidationResult>();
            bool valid = Validator.TryValidateObject(_object, new ValidationContext(_object), results, true);
            if (valid)
                $"\'{_object} {variableName}\' is valid".WriteLineGreen();
            else {
                $"\'{_object} {variableName}\' is not valid:".WriteLineRed();
                results.ToList().ForEach(error => {
                    $"                {error.MemberNames.First()}: {error.ErrorMessage}".WriteLineRed();
                });
            }
            return valid;
            
        }
        public static void PrintRazor_G_CS_Files_Path()
        {
			(Directory.GetCurrentDirectory() + @"\obj\Debug\net6.0\generated\Microsoft.NET.Sdk.Razor.SourceGenerators\Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator").WriteLineGreen();
		}
		
        

		public static string ToButton(this string content,string btnClass = "info")
        {
            return $"<a href = \"{content}\"  class = \"btn btn-{btnClass} btn-sm m-1\">{content}</a>";
        }
        public static string ToHtml(this string content)
        {
            return $@"
                    <!DOCTYPE html>
                    <html>
                        <head>
                            <meta charset=""UTF-8"">
                            <title>Testing Page</title>
                            <link rel=""stylesheet"" href=""/css/site.min.css"" />
                            <link rel=""stylesheet"" href=""/css/bootstrap.min.css"" />
                            <script src=""/js/jquery.min.js"">
                            </script><script src=""/js/popper.min.js"">
                            </script><script src=""/js/bootstrap.min.js""></script> 
                        </head>
                        <body>
                            {content}
                        </body>
                    </html>";
        }

        public static string SqlString(string databaseName) {
            return @"
                Data Source=localhost,1433;
                Initial Catalog="+databaseName+@";
                User ID=sa;
                Password=sa123456!;
                TrustServerCertificate=true";
        }
        public static string SqlString(string databaseName,bool PlainText)
        {
            return "Data Source=localhost,1433;Initial Catalog=" + databaseName + ";User ID=sa;Password=sa123456!;TrustServerCertificate=true";
        }
        public static void ShowErrors(this ModelStateDictionary modelStates)
        {
            
            foreach(var key in modelStates.Keys)
            {
                foreach (ModelError error in modelStates[key].Errors)
                {
                    ("Error [" + key + "]: " + error.ErrorMessage).WriteLineRed();
                }
            }
        }
        public static T WriteLineRed<T>(this T _object,params bool[] titles)
        {
            if(titles.Length > 0) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("---------------------------------------"+_object+ "---------------------------------------");
                Console.ResetColor();
                return _object;
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(_object);
            Console.ResetColor();
            return _object;
        }
        public static T WriteLineDarkRed<T>(this T _object, params bool[] titles)
        {
            if (titles.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("---------------------------------------" + _object + "---------------------------------------");
                Console.ResetColor();
                return _object;
            }
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(_object);
            Console.ResetColor();
            return _object;
        }
        public static T WriteLineGreen<T>(this T _object, params bool[] titles)
        {
            if (titles.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("---------------------------------------" + _object + "---------------------------------------");
                Console.ResetColor();
                return _object;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(_object);
            Console.ResetColor();
            return _object;
        }
        public static T WriteLineBlue<T>(this T _object, params bool[] titles)
        {
            if (titles.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("---------------------------------------" + _object + "---------------------------------------");
                Console.ResetColor();
                return _object;
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(_object);
            Console.ResetColor();
            return _object;
        }
        public static T WriteLineMagenta<T>(this T _object, params bool[] titles)
        {
            if (titles.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("---------------------------------------" + _object + "---------------------------------------");
                Console.ResetColor();
                return _object;
            }
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(_object);
            Console.ResetColor();
            return _object;
        }
        public static T WriteLine<T>(this T _object, ConsoleColor _color, params bool[] titles)
        {
            if (titles.Length > 0)
            {
                Console.ForegroundColor = _color;
                Console.WriteLine("---------------------------------------" + _object + "---------------------------------------");
                Console.ResetColor();
                return _object;
            }
            Console.ForegroundColor = _color;
            Console.WriteLine(_object);
            Console.ResetColor();
            return _object;
        }
        public static List<T> CreateAnonymousList<T>(T a) 
        {
            return new List<T>();
        }
        public static T WriteLine<T>(this T _object, params bool[] titles)
        {
            if (titles.Length > 0)
            {
                Console.WriteLine("---------------------------------------" + _object + "---------------------------------------");
                return _object;
            }
            Console.WriteLine(_object);
            return _object;
        }
        public static void WriteLineJson(this string path) {
            try { File.ReadAllText(path).WriteLineBlue(); } catch (Exception) {
                "Failed To Read Json".WriteLineBlue();
            }
        }
        public static T printAllProperties<T>(this T _object, string customName)
        {
            try
            {
                
                Console.WriteLine("Name: " + customName);
                $"Class name: {_object.GetType().Name}".WriteLine();
                _object.GetType().GetFields().ToList().ForEach(p =>
                {
                    try
                    {
                        string valueString;
                    if (p.GetValue(_object) == null) valueString = "null"; else valueString = p.GetValue(_object).ToString();
                    Console.WriteLine($"{p.Name,20} : {valueString}");
                    
                        if ((p.GetValue(_object) as Array) != null)
                        {
                            Array array = p.GetValue(_object) as Array;
                            for (int i = 0; i < array.Length; i++)
                            {
                                Console.WriteLine($"{"",20}   {array.GetValue(i).ToString()}");
                            }

                        }
                    } catch (Exception) { $"printAllProperties {customName} Failed 1".WriteLineRed(); }
                    
                });

                _object.GetType().GetProperties().ToList().ForEach(p =>
                {
                    try
                    {
                        string valueString;
                    if (p.GetValue(_object) == null) valueString = "null"; else valueString = p.GetValue(_object).ToString();
                    Console.WriteLine($"{p.Name,20} : {valueString}");
                    
                        if ((p.GetValue(_object) as Array) != null)
                        {
                            Array array = p.GetValue(_object) as Array;
                            for (int i = 0; i < array.Length; i++)
                            {
                                Console.WriteLine($"{"",20}   {array.GetValue(i).ToString()}");
                            }

                        }
                    } catch (Exception e) { $"printAllProperties {customName} Failed 2:{e.Message}".WriteLineRed(); }
                    
                });
            }
            catch (Exception) {
                $"printAllProperties {customName} Failed 0".WriteLineRed();
            }
            
            return _object;
        }
    }
}