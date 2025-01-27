using System.Reflection;
using MongoDB.Driver;

namespace DevCL.Requests;

public class PatchRequest<T> {
    bool updateDefsInitialized = false;
    UpdateDefinition<T> _updateDefs = Builders<T>.Update.Combine();
    public UpdateDefinition<T> GetUpdateDefinition() {
        if (!updateDefsInitialized) {
            GenerateUpdateDefinition();
            updateDefsInitialized = true;
        }

        return _updateDefs;
    }
    
    void GenerateUpdateDefinition() {
        foreach (PropertyInfo prop in GetType().GetProperties()) {
            Console.WriteLine($"{prop.Name}: {prop.GetValue(this)}");

            if (prop.GetValue(this) == null) {
                continue;
            }

            _updateDefs = _updateDefs.Set($"{prop.Name}", prop.GetValue(this));
        }
    }
}