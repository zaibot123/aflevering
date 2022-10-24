using Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Assignment3TestSuite;
using Response = Server.Response;
using System.Runtime.Serialization;
using System.IO;

Console.WriteLine("Hello, World!");

var server = new TcpListener(IPAddress.Loopback, 5000);
server.Start();
Console.WriteLine("Server started...");


// adding elements
var data = new Dictionary<int, string>()
{
    {1, "Beverages" },
    {2, "Condiments"},
    {3, "Confections"}
};

var inc = 4;

while (true)
{
    var client = server.AcceptTcpClient();
    Console.WriteLine("Client connected...");

    try
    {
        HandleClient(client, data);
    }
    catch (Exception e)

    {
        Console.WriteLine("Unable to communicate with client...");
    }

}

static void HandleClient(TcpClient client, Dictionary<int, string>? data)

{
    List<string> listoferror = new List<string>();
    string[] methods = { "read", "update", "delete", "read" };
    var stream = client.GetStream();
    var buffer = new byte[1024];
    var rcnt = stream.Read(buffer);
    var requestText = Encoding.UTF8.GetString(buffer, 0, rcnt);
    string[] split_path;
    var request = JsonSerializer.Deserialize<Request>(requestText);

    //checks if path is invalid
    var validpath = true;
    split_path = request.Path.Split("/");
    var sent = false;
  
    int result;
    var inc = 4;

    
    //Adds data to dict if not exist. else Bad request
    if (request.Method.Equals("create")&& request.Body != null)
    {

        if (split_path.Length == 3)
        {
            //mangler
            data.Add(inc, request.Body.ToString());
            inc+=1;
        }
        else 
        {
            listoferror.Add("4 Bad Request");
        }
    }


    //Updates data in dict if exist. else Bad request
    else if (request.Method.Equals("update")&& request.Body != null)
    {
        int i;
        if (int.TryParse(split_path.Last(), out i) && data.ContainsKey(i))
        {
            data[i] = request.Body;
            Response r = CreateReponse("3 updated", "");
            SendResponse(stream, r);
            stream.Close();

            
        }
        else if (int.TryParse(split_path.Last(), out i) && !data.ContainsKey(i))
        {
            //mangler body
            listoferror.Add("5 Not found");

        }
        else 
        {
            listoferror.Add("4 Bad Request");
        }
    }





    else if (request.Method.Equals("delete"))
    {
        int i;
        if (int.TryParse(split_path.Last(), out i) && !data.ContainsKey(i)) {
            listoferror.Add("5 not found");
        }
        if (int.TryParse(split_path.Last(), out i) && data.ContainsKey(i))
        {
            Response r = CreateReponse("NA", "W");
            SendResponse(stream, r);
            stream.Close();

        }
        else if (split_path.Length < 4)
        {
            listoferror.Add("4 Bad Request");
        }
        else
        {
            listoferror.Add("1 ok");
            var myKey = data.FirstOrDefault(x => x.Value ==split_path.Last()).Key;
            data.Remove(myKey);
        }
    }





    else if (request.Method.Equals("read"))
    {
        int i;
        if (int.TryParse(split_path.Last(), out i) && data.ContainsKey(i))
        {

            Response r = CreateReponse("1 Ok", data[i]);
            SendResponse(stream, r);
            stream.Close();
        }
        else if (int.TryParse(split_path.Last(), out i) && !data.ContainsKey(i))
        {
            //mangler body

            listoferror.Add("5 Not found");
        }
        else if (split_path[2].Equals("categories") && split_path.Length <= 3)
        {
            //mangler body
            listoferror.Add("1 Ok");
            foreach (var item in data)
            {
                listoferror.Add(item.ToString());
            }

        }
        else
        {
            listoferror.Add("4 Bad Request");
        }

    }
 



    var listofdistincterror = listoferror.Distinct().ToList();
    Response response = CreateReponse(String.Join(" ",listofdistincterror), null);
    SendResponse(stream, response);
    stream.Close();
    }




static void SendResponse(NetworkStream stream, Response response)
{
    var responseText = JsonSerializer.Serialize<Response>(response);
    var responseBuffer = Encoding.UTF8.GetBytes(responseText);
    stream.Write(responseBuffer);
}

static Server.Response CreateReponse(string status, string body)
{
    return new Server.Response
    {
        Status = status,
        Body = body
    };
}

