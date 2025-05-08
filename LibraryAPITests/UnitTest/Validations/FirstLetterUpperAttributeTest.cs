using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryAPI.Validations;

namespace LibraryAPITests.UnitTest.Validations
{
    [TestClass]
    public class FirstLetterUpperAttributeTest
    { 
        // A Test must have the name of what is expected to be evaluated
        [TestMethod]
        [DataRow("")] //This Decorator help to trigger the same test with differents attributes
        [DataRow("    ")]
        [DataRow(null)]
        [DataRow("Luis")]
        public void Is_valid_ReturnSucced_IfValueIsEmptyOrNull_TheFirstLetterWontBeLower(string value)
        {
            //Preparacion
            var firstUpperLetterAttribute = new FirstLetterUpperAttribute();
            var validationContext = new ValidationContext(new object());
            // var value = string.Empty;

            // Prueba
            var result = firstUpperLetterAttribute.GetValidationResult(value, validationContext);

            // Verificacion
            // Assert.AreEqual(expected: 1, actual: 2);
            Assert.AreEqual(expected: ValidationResult.Success, actual: result);
        }

        [TestMethod]
        [DataRow("luis")]
        public void Is_invalid_ReturnSucced_TheFirstLetter_is_upper(string value)
        {
            //Preparacion
            var firstUpperLetterAttribute = new FirstLetterUpperAttribute();
            var validationContext = new ValidationContext(new object());
            // var value = string.Empty;

            // Prueba
            var result = firstUpperLetterAttribute.GetValidationResult(value, validationContext);

            // Verificacion
            // Assert.AreEqual(expected: 1, actual: 2);
            Assert.AreEqual(
                expected: "The first letter must be capitalize", 
                actual: result!.ErrorMessage
                );
        }

        // [TestMethod]
        // [DataRow("Luis")] //This Decorator help to trigger the same test with differents attributes
        // public void Is_valid_ReturnSucced_IfTheFirstLetterIsUpper(string value)
        // {
        //     //Preparacion
        //     var firstUpperLetterAttribute = new FirstLetterUpperAttribute();
        //     var validationContext = new ValidationContext(new object());
        //     // var value = string.Empty;

        //     // Prueba
        //     var result = firstUpperLetterAttribute.GetValidationResult(value, validationContext);

        //     // Verificacion
        //     // Assert.AreEqual(expected: 1, actual: 2);
        //     Assert.AreEqual(expected: ValidationResult.Success, actual: result);
        // }
    }
    
}