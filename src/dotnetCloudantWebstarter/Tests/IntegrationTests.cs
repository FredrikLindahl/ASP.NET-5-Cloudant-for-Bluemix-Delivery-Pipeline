using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using Microsoft.Extensions.OptionsModel;
using dotnetCloudantWebstarter.Models;
using CloudantDotNet.Controllers;
using Xunit;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace dotnetCloudantWebstarter.Tests {
	
	public class IntegrationTests {
			
		private static readonly string dbName = "todos";

        public Creds cloudantCreds;

		public ToDoItem createItem;
		public ToDoItem updateItem;
		public ToDoItem deleteItem;
		
		public string createItemResponseID;
        public string createItemResponseRev;
        public string updateItemResponseID;
        public string updateItemResponseRev;
        public string deleteItemResponseID;
        public string deleteItemResponseRev;
        
		public bool createTestResult;
		public bool updateTestResult;
		public bool deleteTestResult;

		// -----------------------------------------
		
        public IntegrationTests() {

			cloudantCreds = new Creds();
			string vcapServices = System.Environment.GetEnvironmentVariable("VCAP_SERVICES");
       		
       		if (vcapServices != null) {
       			
            	dynamic json = JsonConvert.DeserializeObject(vcapServices);
            	
            	foreach (dynamic obj in json.Children()) {
                	
                	if (((string)obj.Name).ToLowerInvariant().Contains("cloudant")) {
                    
                    	dynamic credentials = (((JProperty)obj).Value[0] as dynamic).credentials;
                    
                    	if (credentials != null) {
                    			
                       		cloudantCreds.host = credentials.host;
                       		cloudantCreds.username = credentials.username; 
                      		cloudantCreds.password = credentials.password;

                        	break;
                    	}
                	}
            	}
        	}
        	
        	createItem = new ToDoItem();
        	updateItem = new ToDoItem();
        	deleteItem = new ToDoItem();
        	
		}

		// ------------------------------------------------
		
		
		
		[Fact]
		public async void testCreate() {
			
			createTestResult = await create(JsonConvert.DeserializeObject<ToDoItem>("{ 'text': 'Sample 1' }"), "testCreate");
			Assert.True(createTestResult);
		}
		
		[Fact]
		public async void testUpdate() {
			
			await create(JsonConvert.DeserializeObject<ToDoItem>("{ 'text': 'Sample 2' }"), "testUpdate");
			
			updateItem.id = updateItemResponseID;
			updateItem.rev = updateItemResponseRev;
			updateItem.text = "TestUpdate";
			
			updateTestResult = await update(updateItem, "testUpdate");
			Assert.True(updateTestResult);
		}
		
		[Fact]
		public async void testDelete() {
			
			await create(JsonConvert.DeserializeObject<ToDoItem>("{ 'text': 'Sample 3' }"), "testDelete");
			
			deleteItem.id = deleteItemResponseID;
			deleteItem.rev = deleteItemResponseRev;
			
			deleteTestResult = await delete(deleteItem, "testDelete");
			Assert.True(deleteTestResult);
		}
		
    	// ------------------------------------------------

        [HttpPost]
        public async Task<bool> create(ToDoItem item, string invoker) {
          
          using (var client = cloudantClient()) {
           
           	   var response = await client.PostAsJsonAsync(dbName, item);
               
               if (response.IsSuccessStatusCode) {
                    
                    var responseJson = await response.Content.ReadAsAsync<ToDoItem>();
                    
                    if(invoker.Equals("testCreate")) {
                    	this.createItemResponseID = responseJson.id;
               	    	this.createItemResponseRev = responseJson.rev;
                    } else if(invoker.Equals("testUpdate")) {
						this.updateItemResponseID = responseJson.id;
               	   		this.updateItemResponseRev = responseJson.rev;                    	
                    } else if(invoker.Equals("testDelete")) {
                    	this.deleteItemResponseID = responseJson.id;
               	    	this.deleteItemResponseRev = responseJson.rev;
                    }      
                    
                    return true;
                }
                
				string msg = "Failure to POST. Invoked by: " + invoker + ". Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase;
           		Console.WriteLine(msg);
     
            	return false;
            }
        }
        
        // ------------------------------------------------

        [HttpPut]
        public async Task<bool> update(ToDoItem item, string invoker) {
            
            using (var client = cloudantClient()) {
                
                var response = await client.PutAsJsonAsync(dbName + "/" + item.id + "?rev=" + item.rev, item);
  
                if (response.IsSuccessStatusCode) {
                	
                	var responseJson = await response.Content.ReadAsAsync<ToDoItem>();
               	   
               		Console.WriteLine("ITEM ID: " + responseJson.id + "ITEM REV: " + responseJson.rev);
                    
                    return true;
                }
                
				string msg = "Failure to PUT. Invoked by: " + invoker + ". Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase;
           		Console.WriteLine(msg);
                
                return false;
            }
        }
        
        // --------------------------------------------------

        [HttpDelete]
        public async Task<bool> delete(ToDoItem item, string invoker) {
        
        	using (var client = cloudantClient()) {
                
                var response = await client.DeleteAsync(dbName + "/" + item.id + "?rev=" + item.rev);  
                
                if (response.IsSuccessStatusCode) {
                    return true;
                }
                
				string msg = "Failure to POST. Invoked by: " + invoker + ". Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase;
           		Console.WriteLine(msg);
                
                return false;
            }
        }

	// ------------------------------------------------

        private HttpClient cloudantClient() {
            
            if (cloudantCreds.username == null || cloudantCreds.password == null || cloudantCreds.host == null) {
                throw new Exception("Missing Cloudant NoSQL DB service credentials");
            }

            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(cloudantCreds.username + ":" + cloudantCreds.password));

            HttpClient client = HttpClientFactory.Create(new LoggingHandler());
            client.BaseAddress = new Uri("https://" + cloudantCreds.host);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            return client;
        }
    
    // ------------------------------------------------
    
    } // END IntegrationTests CLASS!

	// ------------------------------------------------

	public class Creds {
  		public string username { get; set; }
    	public string password { get; set; }
    	public string host { get; set; }
	}

	// ------------------------------------------------

    class LoggingHandler : DelegatingHandler {
        
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
        	
            Console.WriteLine("{0}\t{1}", request.Method, request.RequestUri);
            var response = await base.SendAsync(request, cancellationToken);
            Console.WriteLine(response.StatusCode);
            return response;
        }
    }
	
	// ------------------------------------------------
		
}
