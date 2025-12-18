import subprocess
import sys
import os
from pymongo import MongoClient
from bson import ObjectId

client = MongoClient("mongodb://localhost:27017/");
database = client["dev_cl"]
tasks = database["tasks"]

os.makedirs("dump", 511, True)

result = subprocess.run(["ls", "-l"], capture_output=True, text=True)

print("Return code:", result.returncode)
print("Output:\n", result.stdout)
print("Errors:\n", result.stderr)

sys.exit(0)

template = database["shared_attributes"].find_one({
    "_id": ObjectId("6917f0e68970b5d5e7585429")
})

variables = {}

if (template == None):
    print("template with id has not been found")
    sys.exit(-1)
else:
    print("template with id has been found:")
    for value in template["validValues"]:
        variables[str(value["name"]).lower()] = ObjectId(value["_id"]).__str__()
    print(variables)
    

    

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
    for attribute in doc["attributes"]:
        if (attribute["_id"] != "6917f0e68970b5d5e7585429"):
            continue

        if (attribute["value"] == "bug"):
            print("bug")
        elif attribute["value"] == "feature":
            print("feature")
        elif attribute["value"] == "refactor":
            print("refactor")
        else:
            print("error")