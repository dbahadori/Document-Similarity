using System.ComponentModel.DataAnnotations;

namespace DocumentSimilarity.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "وارد کردن نام کاربری ضروری می باشد")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required(ErrorMessage = "وارد کردن گذرواژه فعلی ضروری می باشد")]
        [DataType(DataType.Password)]
        [Display(Name = "گذرواژه فعلی")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = " وارد کردن گذرواژه جدید ضروری می باشد")]
        [StringLength(100, ErrorMessage = " {0}وارد شده باید حداقل دارای {2} کاراکتر باشد", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "گذرواژه جدید")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = " وارد کردن تکرار گذرواژه جدید ضروری می باشد")]
        [DataType(DataType.Password)]
        [Display(Name = "تکرار گذرواژه")]
        [Compare("NewPassword", ErrorMessage = "گذرواژه وارد شده با تکرار آن یکسان نمی باشد.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "وارد کردن نام کاربری ضروری می باشد")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "وارد کردن گذرواژه ضروری می باشد")]
        [DataType(DataType.Password)]
        [Display(Name = "گذرواژه")]
        public string Password { get; set; }

        [Display(Name = "مشخصات من را بخاطر بسپار؟")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "وارد کردن نام کاربری ضروری می باشد")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "وارد کردن گذرواژه ضروری می باشد")]
        [StringLength(100, ErrorMessage = " {0}وارد شده باید حداقل دارای {2} کاراکتر باشد", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "گذرواژه")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تکرار گذرواژه")]
        [Compare("Password", ErrorMessage = "گذرواژه وارد شده با تکرار آن یکسان نمی باشد.")]
        public string ConfirmPassword { get; set; }
    }
}
