using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TNTCalculatorRazor.Domain;

public class IndexModel : PageModel
{
    [BindProperty] public int Age { get; set; }
    [BindProperty] public double Height { get; set; }
    [BindProperty] public double Weight { get; set; }
    [BindProperty] public Sex Sex { get; set; }

    public double Bmr { get; set; }

    public void OnGet()
    {
    }

    public void OnPost()
    {
        Bmr = BmrCalculator.Calculate(
            age: Age,
            weightKg: Weight,
            heightCm: Height,
            sex: Sex
        );
    }
}