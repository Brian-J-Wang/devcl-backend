using DotNetEnv;
using MongoDB.Driver;
using DevCL.Database.Model;
using DevCL.Controllers;
using DevCL.Exceptions;
using MongoDB.Bson;

namespace DevCL.Database;

static class CLCollections {

    static FilterDefinition<CheckList> Document(string id) {
        return Builders<CheckList>.Filter.Eq(d => d.Id, id);
    }

    static IMongoCollection<CheckList>? collection;

    public static void Init() {
        Env.Load();

        var client = new MongoClient(Env.GetString("DB_URL"));

        collection = client.GetDatabase("dev_cl").GetCollection<CheckList>("collection");

        Console.WriteLine("Collection successfully retrieved");
    }

    public static void InsertNewDocument() {}

    public static CheckList RetreiveDocument(string documentId) {
        if (collection == null) {
            throw new CollectionNotInitializedException("The collection has not been initialized");
        }

        var filter = Builders<CheckList>.Filter.Eq(d => d.Id, documentId);

        var document = collection.Find(filter).First();

        return document;
    }

    public static PatchNotes PushNewVersion(string documentId, PatchType patchType) {
        if (collection == null) {
            throw new CollectionNotInitializedException("The collection has not been initialized");
        }

        var filter = Builders<CheckList>.Filter.Eq(d => d.Id, documentId);

        var document = collection.Find(filter).First();

        List<string> patchItems = new List<string>();

        document.checkListSection.ForEach((section) => {
            section.items = section.items.Where((item) => {
                if (item.IsChecked) {
                    patchItems.Add($"{section.format}: {item.Title}");
                    return false;
                } else {
                    return true;
                }
            }).ToList();
        });

        PatchNotes patch = new PatchNotes() {
            Id = ObjectId.GenerateNewId().ToString(),
            version = document.version,
            content = patchItems
        };
        document.patchNotes.Add(patch);

        document.version = GetNewVersion(document.version, patchType);


        var update = Builders<CheckList>.Update
                        .Set(d => d.version, document.version)
                        .Set(d => d.checkListSection, document.checkListSection)
                        .Set(d => d.patchNotes, document.patchNotes);

        collection.UpdateOne(d => d.Id == document.Id, update);

        return patch;

        static string GetNewVersion(string version, PatchType patchType) {
            string[] values = version.Split(".");

            if (patchType == PatchType.major) {
                values[0] = (int.Parse(values[0]) + 1).ToString();
                values[1] = "0";
                values[2] = "0";
            } else if (patchType == PatchType.minor) {
                values[1] = (int.Parse(values[1]) + 1).ToString();
                values[2] = "0";
            } else {
                values[2] = (int.Parse(values[2]) + 1).ToString();
            }

            return $"{values[0]}.{values[1]}.{values[2]}";
        }
    }

    public static CLItem AddNewTask(string documentId, IncomingCLItem item) {
        if (collection == null) {
            throw new CollectionNotInitializedException("The collection has not been initialized");
        }
        CLItem entry = item.ToCLItem();

        var filter = Builders<CheckList>.Filter.Eq(d => d.Id, documentId);
        var document = collection.Find(filter).First();
        
        var category = document.checkListSection.Find(section => section.Id == item.Section) ?? throw new CategoryNotFoundException($"sectionId {item.Section} was not found in document {documentId}");
        category.items.Add(entry);

        var update = Builders<CheckList>.Update
                        .Set(d => d.checkListSection, document.checkListSection);
        collection.UpdateOne(filter, update);

        return entry;
    }

    public static CLItem UpdateTask(string documentId, IncomingCLItem reqItem) {
        if (collection == null) {
            throw new CollectionNotInitializedException("The collection has not been initialized");
        }

        //declare document filter
        var filter = Builders<CheckList>.Filter.Eq(d => d.Id, documentId);
        //declare array filter
        var arrayFilter = new [] {
            new BsonDocument("section._id", ObjectId.Parse(reqItem.Section)),
            new BsonDocument("item._id", ObjectId.Parse(reqItem.Id))
        };

        var options = new UpdateOptions {
            ArrayFilters = arrayFilter.Select(bson => new BsonDocumentArrayFilterDefinition<BsonDocument>(bson)).ToList()
        };
        //declare set builder
        var update = Builders<CheckList>.Update.Set("checkList.$[section].items.$[item].checked", reqItem.IsChecked);

        //call coolection update
        collection.UpdateOne(filter, update, options);

        Console.WriteLine("Retrieving Item");
        var itemProjection = Builders<CheckList>.Projection.Expression(u => u.checkListSection
                        .Where(section => section.Id == reqItem.Section).First()
                        .items.Where(item => item.Id == reqItem.Id).First());
        var item = collection.Find(filter).Project(itemProjection).First();
        
        Console.WriteLine(item.ToJson());

        return item;
    }
}