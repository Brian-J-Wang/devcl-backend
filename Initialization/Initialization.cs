using DevCL.Model;
using MongoDB.Bson;

public static class Initialization
{
    public static void SeedAttributesDB(AttributeService service) {
        //cleans db of shared attributes and re-adds them;
        List<BaseAttribute> attributes = new List<BaseAttribute> {
            new EnumAttribute {
                Id = new ObjectId("6917f0e68970b5d5e7585429"),
                Name = "Category",
                PrimaryColor = "#699b22",
                ValidValues = {
                    new EnumAttributeValue {
                        Id = new ObjectId("6941ede66a5b123d2c8ccc4d"),
                        Name = "Refactor"
                    },
                    new EnumAttributeValue {
                        Id = new ObjectId("6941edf1c619b4086bbb1b4b"),
                        Name = "Bug"
                    },
                    new EnumAttributeValue {
                        Id = new ObjectId("6941edfece58943f78c46c03"),
                        Name = "Feature"
                    }
                }
            },
            new EnumAttribute {
                Id = new ObjectId("6941f26ae488f7b403c3eafb"),
                Name = "Priority",
                PrimaryColor = "#da8300",
                AttributeConfig = {
                    coloringMode = ColoringMode.secondary,
                    showAttributeName = true
                },
                ValidValues = {
                    new EnumAttributeValue {
                        Id = new ObjectId("6941f2ae1eb8589afc1e72c3"),
                        Name = "Low",
                        SecondaryColor = "#517908ff"
                    },
                    new EnumAttributeValue {
                        Id = new ObjectId("6941f2cc9c3fd0d6d8f8330f"),
                        Name = "Medium",
                        SecondaryColor = "#bd8100ff"
                    },
                    new EnumAttributeValue {
                        Id = new ObjectId("6941f2e5e098e85c78d2b86f"),
                        Name = "High",
                        SecondaryColor = "#9b0e0eff"
                    }
                }
            }
        };

        foreach (var attribute in attributes) {
            var result = service.AddSharedAttribute(attribute);
            Console.WriteLine($"Successfully updated {result.Name} attribute, type of {result.GetType()}");
        }
    } 
}