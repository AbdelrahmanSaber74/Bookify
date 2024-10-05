//// jQuery unobtrusive validation defaults
//$.validator.setDefaults({
//    errorClass: "", // You can specify a default error class if needed
//    validClass: "", // You can specify a default valid class if needed

//    highlight: function (element, errorClass, validClass) {
//        // Add both 'is-invalid' and 'input-validation-error' classes to invalid elements
//        $(element).addClass("is-invalid input-validation-error").removeClass("is-valid");
//        $(element.form).find("[data-valmsg-for=" + element.id + "]").addClass("invalid-feedback");
//    },

//    unhighlight: function (element, errorClass, validClass) {
//        // Add 'is-valid' class to valid elements
//        $(element).addClass("is-valid").removeClass("is-invalid input-validation-error");
//        $(element.form).find("[data-valmsg-for=" + element.id + "]").removeClass("invalid-feedback");
//    },
//});
