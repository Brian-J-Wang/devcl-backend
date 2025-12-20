using DevCL.Model;

namespace DevCL.Model;

public class IntegerAttribute : BaseAttribute {
    public int minValue { get; set; }
    public int maxValue { get; set; }

    public override bool isValidAttributeValue(TaskAttribute attribute) {
        throw new NotImplementedException();
    }
}