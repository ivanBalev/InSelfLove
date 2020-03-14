namespace BDInSelfLove.Web.Controllers.Test
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Web.Infrastructure.ModelBinders;
    using BDInSelfLove.Web.Infrastructure.ValidationAttributes;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    public class Names
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    public class TestInputModel : IValidatableObject // ПОЗВОЛЯВА ВАЛИДАЦИЯ НА ЦЕЛИЯ ОБЕКТ ВМЕСТО ПООТДЕЛНО НА ВСЯКО ПРОПЪРТИ, АКО ИМА НУЖДА
    {
        public int Id { get; set; }

        public Names Names { get; set; }

        [ModelBinder(typeof(YearModelBinder))]
        public int Year { get; set; }

        // VALIDATION ATTRIBUTE
        [DataType(DataType.Date)] // Removes the hh:mm:ss requirement on both backend and frontend when using this property in a form, for example
        [RegularExpression("[0-9]{10}", ErrorMessage = "Invalid Data")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [BGIDValidation]
        public string EGN { get; set; }

        public IEnumerable<IFormFile> FilesFromForm { get; set; }

        // МЕТОДЪТ ОТ IValidatablebject
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (int.Parse(this.EGN.Substring(0, 2)) != this.DateOfBirth.Year % 100)
            {
                yield return new ValidationResult("Годината на раждане и ЕГН не съвпадат");
            }
        }
    }

    public class TestController : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> Index(/*[BindRequired]*/ /*[FromQuery]*/ /*[FromHeader]*/
            /*[FromForm]*/ /*[BindNever]*/ /*[Bind("Names")]*/TestInputModel input)
        {
            // OЩЕ ЕДИН НАЧИН ДА ПРОВЕРИМ ЗА СТЕЙТА НА ИНПУТА
            if(input.EGN.Length != 10)
            {
                this.ModelState.AddModelError(nameof(TestInputModel.EGN) /*ВМЕСТО СТРИНГОВ ЛИТЕРАЛ*/, "Invalid");
            }


            if (!this.ModelState.IsValid)
            {
                // ПОДАВАМЕ МОДЕЛА ВЪВ ФОРМАТА АКО СА НЕВАЛИДНИ ДАННИТЕ ЗА ДА СЕ ПРЕПОПУЛИРА ВСИЧКО И ДА НЕ ТРЯБВА ДА СЕ ВКАРВАТ НАНОВО ВСИЧКИ ДАННИ
                return this.View(input);
            }

            // НИКОГА НЕ РАЗЧИТАЙ НА КОНТЕНТ ТАЙПА ОТ КЛИЕНТА!!! ТОВА Е МНОГО СЕРИОЗЕН СЕКЮРИТИ ПРОБЛЕМ, ЗАЩОТО МОЖЕ ДА Е .ЕХЕ НАПРИМЕР И ДА СТАНЕ МИЗЕРИЯ СЕРИОЗНА
            // ЗА ДА ГО ИЗБЕГНЕМ, ВИНАГИ ВЪВЕЖДАМЕ РАЗШИРЕНИЕТО НА ФАЙЛА САМИ. АКО Е .JPG, ВЪВЕЖДАМЕ СИ САМИ .JPG

            // четене на файл от клиента
            // това отваря стрийм към user.pdf и пише директно в него
            using (var fileStream = new FileStream(@"C:\Temp\user.pdf", FileMode.Create))
            {
                if(input.FilesFromForm.Any(x => x.Length > 1024 * 1024 * 10))
                {
                    // invalid file size too large
                }

                await input.FilesFromForm.First().CopyToAsync(fileStream);
            }

            return this.Json(input);
        }

        public IActionResult Download(string fileName)
        {
            // ЗАДЪЛЖИТЕЛНО VALIDATE USER INPUT!!! В ПРОТИВЕН СЛУЧАЙ МОГАТ ДА НИ ВЪВЕДАТ СТРИНГ КОЙТО ДА СВАЛЯ ДРУГИ ДАННИ ОТ ФАЙЛОВАТА НИ СИСТЕМА
            // можем да ескейпнем всички наклонени черти и двоеточия за да се защитим
            return this.PhysicalFile(@$"C:\Temp\{fileName}", "application/pdf", "test.pdf");
        }

        //метод от нета за изчистване на имена на файлове от потенциални заплахи
        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }
    }
}
