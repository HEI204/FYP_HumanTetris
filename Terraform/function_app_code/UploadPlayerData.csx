#r "Newtonsoft.Json"
#r "C:\home\data\Functions\packages\nuget\azure.core\1.37.0\lib\net6.0\Azure.Core.dll"
#r "C:\home\data\Functions\packages\nuget\azure.data.tables\12.8.3\lib\netstandard2.0\Azure.Data.Tables.dll"

using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");
    
    // Connect to Azure Table Storage
    string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    TableServiceClient tableServiceClient = new TableServiceClient(connectionString);

    // Get table from the Azure Table Storage
    string tableName = "fyptablestorage";
    TableClient table = tableServiceClient.GetTableClient(tableName);

    // Deserialize HTTP POST request body 
    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    log.LogInformation(requestBody);
    Player playerData = JsonConvert.DeserializeObject<Player>(requestBody);

    // Create entity & upload to Azure Table
    PlayerEntity playerEntity = new PlayerEntity {
        PartitionKey = "FYP",
        RowKey = playerData.user,
        User = playerData.user,
        Age = playerData.age,
        Score = playerData.score,
        // Landmarks = JsonConvert.SerializeObject(playerData.Landmarks),
        Landmark2 = JsonConvert.SerializeObject(playerData.Landmark2),
        Landmark3 = JsonConvert.SerializeObject(playerData.Landmark3),
        Landmark4 = JsonConvert.SerializeObject(playerData.Landmark4),
        Landmark5 = JsonConvert.SerializeObject(playerData.Landmark5),
        Landmark6 = JsonConvert.SerializeObject(playerData.Landmark6),
        Landmark7 = JsonConvert.SerializeObject(playerData.Landmark7),
        Landmark8 = JsonConvert.SerializeObject(playerData.Landmark8),
        Landmark9 = JsonConvert.SerializeObject(playerData.Landmark9),
        Landmark10 = JsonConvert.SerializeObject(playerData.Landmark10),
        Landmark11 = JsonConvert.SerializeObject(playerData.Landmark11),
        Landmark12 = JsonConvert.SerializeObject(playerData.Landmark12),
        Landmark13 = JsonConvert.SerializeObject(playerData.Landmark13),
        Landmark14 = JsonConvert.SerializeObject(playerData.Landmark14),
        Landmark15 = JsonConvert.SerializeObject(playerData.Landmark15),
        Landmark16 = JsonConvert.SerializeObject(playerData.Landmark16),
        Landmark17 = JsonConvert.SerializeObject(playerData.Landmark17),
        Landmark18 = JsonConvert.SerializeObject(playerData.Landmark18),
        Landmark19 = JsonConvert.SerializeObject(playerData.Landmark19),
        Landmark20 = JsonConvert.SerializeObject(playerData.Landmark20),
        Landmark21 = JsonConvert.SerializeObject(playerData.Landmark21),
        Landmark22 = JsonConvert.SerializeObject(playerData.Landmark22),
        Landmark23 = JsonConvert.SerializeObject(playerData.Landmark23),
        Landmark24 = JsonConvert.SerializeObject(playerData.Landmark24),
        Landmark25 = JsonConvert.SerializeObject(playerData.Landmark25),
        Landmark26 = JsonConvert.SerializeObject(playerData.Landmark26),
        Landmark27 = JsonConvert.SerializeObject(playerData.Landmark27),
        Landmark28 = JsonConvert.SerializeObject(playerData.Landmark28),
        Landmark29 = JsonConvert.SerializeObject(playerData.Landmark29),
        Landmark30 = JsonConvert.SerializeObject(playerData.Landmark30),
        Landmark31 = JsonConvert.SerializeObject(playerData.Landmark31),
        Landmark32 = JsonConvert.SerializeObject(playerData.Landmark32),
        Landmark33 = JsonConvert.SerializeObject(playerData.Landmark33),
    };
    try {
        table.AddEntity(playerEntity);

        var result = new ObjectResult("Successful!");
        result.StatusCode = 201;
        return result;
    }
    catch (Exception e) {
        var result = new ObjectResult("Fail to add");
        result.StatusCode = 200;
        return result;
    }
}

private class Player {
    public string user { get; set; }
    public int score { get; set; }
    public int age { get; set; }
    // public List<double>[] Landmarks { get; set; }
    public List<double> Landmark2 { get; set; }
    public List<double> Landmark3 { get; set; }
    public List<double> Landmark4 { get; set; }
    public List<double> Landmark5 { get; set; }
    public List<double> Landmark6 { get; set; }
    public List<double> Landmark7 { get; set; }
    public List<double> Landmark8 { get; set; }
    public List<double> Landmark9 { get; set; }
    public List<double> Landmark10 { get; set; }
    public List<double> Landmark11 { get; set; }
    public List<double> Landmark12 { get; set; }
    public List<double> Landmark13 { get; set; }
    public List<double> Landmark14 { get; set; }
    public List<double> Landmark15 { get; set; }
    public List<double> Landmark16 { get; set; }
    public List<double> Landmark17 { get; set; }
    public List<double> Landmark18 { get; set; }
    public List<double> Landmark19 { get; set; }
    public List<double> Landmark20 { get; set; }
    public List<double> Landmark21 { get; set; }
    public List<double> Landmark22 { get; set; }
    public List<double> Landmark23 { get; set; }
    public List<double> Landmark24 { get; set; }
    public List<double> Landmark25 { get; set; }
    public List<double> Landmark26 { get; set; }
    public List<double> Landmark27 { get; set; }
    public List<double> Landmark28 { get; set; }
    public List<double> Landmark29 { get; set; }
    public List<double> Landmark30 { get; set; }
    public List<double> Landmark31 { get; set; }
    public List<double> Landmark32 { get; set; }
    public List<double> Landmark33 { get; set; }
}

private class PlayerEntity : ITableEntity {

    public int? Age { get; set; }
    public string User { get; set; }
    public int Score { get; set; }
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public Azure.ETag ETag { get; set; }

    // public string Landmarks { get; set; }
    public string Landmark2 { get; set; }
    public string Landmark3 { get; set; }
    public string Landmark4 { get; set; }
    public string Landmark5 { get; set; }
    public string Landmark6 { get; set; }
    public string Landmark7 { get; set; }
    public string Landmark8 { get; set; }
    public string Landmark9 { get; set; }
    public string Landmark10 { get; set; }
    public string Landmark11 { get; set; }
    public string Landmark12 { get; set; }
    public string Landmark13 { get; set; }
    public string Landmark14 { get; set; }
    public string Landmark15 { get; set; }
    public string Landmark16 { get; set; }
    public string Landmark17 { get; set; }
    public string Landmark18 { get; set; }
    public string Landmark19 { get; set; }
    public string Landmark20 { get; set; }
    public string Landmark21 { get; set; }
    public string Landmark22 { get; set; }
    public string Landmark23 { get; set; }
    public string Landmark24 { get; set; }
    public string Landmark25 { get; set; }
    public string Landmark26 { get; set; }
    public string Landmark27 { get; set; }
    public string Landmark28 { get; set; }
    public string Landmark29 { get; set; }
    public string Landmark30 { get; set; }
    public string Landmark31 { get; set; }
    public string Landmark32 { get; set; }
    public string Landmark33 { get; set; }
}