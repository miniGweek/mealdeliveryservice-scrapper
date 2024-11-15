using MealDeliveryServiceScrapper.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealDeliveryServiceScrapper
{
    public class ExtractPapaMacrosNutritionInfo
    {
        private const string PapaMacrosMenuUrl = "https://www.papamacros.com.au/product-category/meals/";
        private readonly ILogger<ExtractPapaMacrosNutritionInfo> _logger;

        public ExtractPapaMacrosNutritionInfo(ILogger<ExtractPapaMacrosNutritionInfo> logger,
            IConfiguration config)
        {
            _logger = logger;
        }

        internal async Task Extract()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var context = await browser.NewContextAsync();
            // Create a new page inside context.
            var page = await context.NewPageAsync();

            await page.GotoAsync(PapaMacrosMenuUrl);
            _logger.LogInformation("Navigated to {url}", PapaMacrosMenuUrl);

            var allMeals = page.Locator("ul.fusion-grid.fusion-grid-3.fusion-flex-align-items-stretch.fusion-grid-posts-cards>li");
            var countDivs = await allMeals.CountAsync();
            var meals = new List<MealNutrition>();

            for (int index = 0; index < countDivs; index++)
            {
                _logger.LogInformation("Processing meal {index}", index + 1);

                var mealCard = allMeals.Nth(index);
                var mealTitle = await mealCard.Locator("div:nth-child(4)>h3>a").InnerTextAsync();
                _logger.LogInformation("Processing meal {mealTitle}", mealTitle);

                var nutritionElement = mealCard.Locator("div:nth-child(6)>div>div>div table:nth-child(1)");
                var caloriesLabel = await nutritionElement.Locator("tbody>tr>th:nth-child(1)").First.InnerTextAsync();
                var protein = await nutritionElement.Locator("tbody>tr>th:nth-child(3)").First.InnerTextAsync();
                var carbs = await nutritionElement.Locator("tbody>tr>th:nth-child(5)").First.InnerTextAsync();
                var fat = await nutritionElement.Locator("tbody>tr>th:nth-child(7)").First.InnerTextAsync();
                var price = await mealCard.Locator("div:nth-child(7)>p>span>bdi").First.InnerTextAsync();

                _logger.LogInformation("Meal {mealTitle} has {caloriesLabel} calories, {protein} protein, {carbs} carbs, {fat} fat and costs {price}", mealTitle, caloriesLabel, protein, carbs, fat, price);

                meals.Add(new MealNutrition
                {
                    Name = mealTitle,
                    EnergyKCal = caloriesLabel,
                    Proteing = protein,
                    Carbohydratesg = carbs,
                    Fatg = fat,
                    Price = price
                });
            }
            _logger.LogInformation("Processed {countDivs} meals", countDivs);
            await page.CloseAsync();
            await using var writer = new StreamWriter("papamacros.csv");
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            await csv.WriteRecordsAsync((IEnumerable)meals);
        }
    }
}
