public static class Initialization
{
    public static void SeedAttributesDB(AttributeService service) {
        //cleans db of shared attributes and re-adds them;
        List<Attribute> attributes = new List<Attribute> {
            new EnumAttribute {
                Id = "6917f0e68970b5d5e7585429",
                Name = "Category",
                ValidValues = {
                    "feature", "refactor", "bug"
                }
            }
        };

        foreach (var Attribute in attributes) {
            var result = service.AddSharedAttribute(Attribute);
            Console.WriteLine($"Successfully updated {result.Name} attribute, type of {result.GetType()}");
        }
    } 
}