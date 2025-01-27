using DotNetEnv;
using MongoDB.Driver;
using DevCL.Database.Model;
using DevCL.Controllers;
using DevCL.Exceptions;
using MongoDB.Bson;
using DevCL.Requests;
using Microsoft.AspNetCore.Mvc;
using SharpCompress.Common;
using DnsClient.Protocol;

namespace DevCL.Database;

public class CLDatabase {
    IMongoCollection<CLCollection> collections;
    public CLDatabase() {
        Env.Load();

        var mongoClient = new MongoClient(Env.GetString("DB_URL"));
        collections = mongoClient.GetDatabase("dev_cl").GetCollection<CLCollection>("collection");
    }


}

static class CLCollections {

    static FilterDefinition<CLCollection> Document(string id) {
        return Builders<CLCollection>.Filter.Eq(d => d.Id, id);
    }

    static IMongoCollection<CLCollection>? collection;

    public static void Init() {
        Env.Load();

        var client = new MongoClient(Env.GetString("DB_URL"));

        collection = client.GetDatabase("dev_cl").GetCollection<CLCollection>("collection");

        Console.WriteLine("Collection successfully retrieved");
    }
    

    public static void InsertNewDocument() {}

    public static CLCollection RetreiveDocument(string documentId) {
        if (collection == null) {
            throw new CollectionNotInitializedException("The collection has not been initialized");
        }

        var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, documentId);

        var document = collection.Find(filter).First();

        return document;
    }

    public static PatchNotes PushNewVersion(string documentId, PatchType patchType) {
        if (collection == null) {
            throw new CollectionNotInitializedException("The collection has not been initialized");
        }

        var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, documentId);

        var document = collection.Find(filter).First();

        List<string> patchItems = new List<string>();

        document.Categories.ForEach((section) => {
            section.Items = section.Items.Where((item) => {
                if (item.Checked) {
                    patchItems.Add($"{section.format}: {item.Blurb}");
                    return false;
                } else {
                    return true;
                }
            }).ToList();
        });

        document.Version = GetNewVersion(document.Version, patchType);

        PatchNotes patch = new PatchNotes() {
            Id = ObjectId.GenerateNewId().ToString(),
            Version = document.Version,
            Content = patchItems
        };
        document.patchNotes.Add(patch);

        var update = Builders<CLCollection>.Update
                        .Set(d => d.Version, document.Version)
                        .Set(d => d.Categories, document.Categories)
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

        var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, documentId);
        var document = collection.Find(filter).First();
        
        var category = document.Categories.Find(section => section.Id == item.Section) ?? throw new CategoryNotFoundException($"sectionId {item.Section} was not found in document {documentId}");
        category.Items.Add(entry);

        var update = Builders<CLCollection>.Update
                        .Set(d => d.Categories, document.Categories);
        collection.UpdateOne(filter, update);

        return entry;
    }

    public static CLItem UpdateTask(string documentId, IncomingCLItem reqItem) {
        if (collection == null) {
            throw new CollectionNotInitializedException("The collection has not been initialized");
        }

        //declare document filter
        var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, documentId);
        //declare array filter
        var arrayFilter = new [] {
            new BsonDocument("section._id", ObjectId.Parse(reqItem.Section)),
            new BsonDocument("item._id", ObjectId.Parse(reqItem.Id))
        };

        var options = new UpdateOptions {
            ArrayFilters = arrayFilter.Select(bson => new BsonDocumentArrayFilterDefinition<BsonDocument>(bson)).ToList()
        };
        //declare set builder
        var update = Builders<CLCollection>.Update.Set("checkList.$[section].items.$[item].checked", reqItem.IsChecked);

        //call coolection update
        collection.UpdateOne(filter, update, options);

        var itemProjection = Builders<CLCollection>.Projection.Expression(u => u.Categories
                        .Where(section => section.Id == reqItem.Section).First()
                        .Items.Where(item => item.Id == reqItem.Id).First());
        var item = collection.Find(filter).Project(itemProjection).First();

        return item;
    }

    public static void UpdateDocument(string documentId, CheckListPatchRequest request) {
        if (collection == null) {
            throw new CollectionNotInitializedException("The collection has not been initialized");
        }

        var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, documentId);
        var update = request.GetUpdateDefinition();

        collection.UpdateOne(filter, update);
    }

    public static CLItem PatchItem(Identifier id, PatchItem item) {
        if (collection == null) {
            throw new CollectionNotInitializedException("The collection has not been initialized");
        }

        var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id.collection);

        var arrayFilter = new [] {
            new BsonDocument("category._id", ObjectId.Parse(id.category)),
            new BsonDocument("item._id", ObjectId.Parse(id.item))
        };

        var options = new UpdateOptions {
            ArrayFilters = arrayFilter.Select(bson => new BsonDocumentArrayFilterDefinition<BsonDocument>(bson)).ToList()
        };

        var updateDef = GetUpdateDefintion(); 

        collection.UpdateOne(filter, updateDef, options);

        if (item.Category != null) {
            Console.WriteLine("Changing category has not been implemented yet");
        }

        var itemProjection = Builders<CLCollection>.Projection.Expression(u => u.Categories
                        .Where(category => category.Id == id.category).First()
                        .Items.Where(item => item.Id == id.item).First());
        var newItem = collection.Find(filter).Project(itemProjection).First();

        return newItem;

        UpdateDefinition<CLCollection> GetUpdateDefintion()
        {
            var builder = Builders<CLCollection>.Update.Combine();

            if (item.Blurb != null)
            {
                builder = builder.Set("categories.$[category].items.$[item].blurb", item.Blurb);
            }

            if (item.Checked != null)
            {
                builder = builder.Set("categories.$[category].items.$[item].checked", item.Checked);
            }

            return builder;
        }
    }

    public static CLItem PostItem(Identifier id, CLItem item) {
        if (collection == null) {
            throw new CollectionNotInitializedException("The collection has not been initialized");
        }

        var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id.collection);
        var document = collection.Find(filter).First();

        var updateDef = Builders<CLCollection>.Update.Push("categories.$[category].items", item);

        var arrayFitler = new [] {
            new BsonDocument("category._id", ObjectId.Parse(id.category))
        };

        var options = new UpdateOptions {
            ArrayFilters = arrayFitler.Select(bson => new BsonDocumentArrayFilterDefinition<CLCollection>(bson)).ToList()
        };

        collection.UpdateOne(filter, updateDef, options);

        var itemProjection = Builders<CLCollection>.Projection.Expression(u => u.Categories
                        .Where(category => category.Id == id.category).First()
                        .Items.Where(item => item.Id == id.item.ToString()).First());
        var newItem = collection.Find(filter).Project(itemProjection).First();

        return newItem;
    }

    public static void DeleteItem(Identifier id) {
        if (collection == null) {
            throw new CollectionNotInitializedException("The collection has not been initialized");
        }

        var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id.collection);
        var document = collection.Find(filter).First();

        var updateDef = Builders<CLCollection>.Update.PullFilter("categories.$[category].items", Builders<CLItem>.Filter.Eq(i => i.Id, id.item));
        
        var arrayFitler = new [] {
            new BsonDocument("category._id", ObjectId.Parse(id.category))
        };

        var options = new UpdateOptions {
            ArrayFilters = arrayFitler.Select(bson => new BsonDocumentArrayFilterDefinition<CLCollection>(bson)).ToList()
        };

        collection.UpdateOne(filter, updateDef, options);
    }
}