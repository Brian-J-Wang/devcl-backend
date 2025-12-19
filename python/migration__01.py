#converts old category task attribute system to new task attribute system

import subprocess
import sys
import os
import shutil
from pymongo import MongoClient
from bson import ObjectId


client = MongoClient("mongodb://localhost:27017/")
database = client["dev_cl"]
tasks = database["tasks"]

#clear content of dump if it exist or create it if it doesn't
if os.path.exists("dump/migration__01"):
    shutil.rmtree("dump/migration__01")
os.makedirs("dump/migration__01", 511, True)

#dump contents of collection
result = subprocess.run(["mongodump", "--db", "dev_cl", "--collection", "tasks", "--out", "./dump/migration__01"], capture_output=True, text=True)
print(result.stderr)

if (result.returncode != 0):
    print("something went wrong while backing up data")
    sys.exit(-1)

#find the attribute for the category section
template = database["shared_attributes"].find_one({
    "_id": ObjectId("6917f0e68970b5d5e7585429")
})
if (template == None):
    print("template with id has not been found")
    sys.exit(-1)

#map old values to new values
variables = {}
for value in template["validValues"]:
    variables[str(value["name"]).lower()] = ObjectId(value["_id"]).__str__()
    
#replace all old values with new values
results = tasks.find({
    "$or" : [
    {
        "attributes.value": "bug"
    },
    {
        "attributes.value": "feature"
    },
    {
        "attributes.value": "refactor"
    }
]})

for doc in results:
    attribute = next((x for x in doc["attributes"] if x["_id"] == "6917f0e68970b5d5e7585429"), None)
    if (attribute == None):
        print("attribute not found")
        continue;

    result = tasks.update_one({
        "_id": ObjectId(doc["_id"])
    }, {
        "$set": {
            "attributes.$[elem].value": variables[attribute["value"]]
        }
    }, 
        array_filters=[{
            "elem._id": "6917f0e68970b5d5e7585429"
        }]
    )

    if (result.modified_count > 0):
        print("document successfully updated:")
        print(tasks.find_one({ "_id": doc["_id"]})["attributes"])
    else:
        print("no such document was updated")

sys.exit(0)
    # for attribute in doc["attributes"]:
    #     if (attribute["_id"] != "6917f0e68970b5d5e7585429"):
    #         continue

    #     if (attribute["value"] == "bug"):
    #         print("bug")
    #     elif attribute["value"] == "feature":
    #         print("feature")
    #     elif attribute["value"] == "refactor":
    #         print("refactor")
    #     else:
    #         print("error")