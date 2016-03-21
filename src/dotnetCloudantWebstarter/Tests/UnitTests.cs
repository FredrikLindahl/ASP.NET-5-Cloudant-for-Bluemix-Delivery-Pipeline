using dotnetCloudantWebstarter.Models;
using CloudantDotNet.Controllers;
using Xunit;

namespace dotnetCloudantWebstarter.Tests {
    
    public class UnitTests {
    	
    	// Verifies ToDoItem constructor!
    	
    	[Fact]
        public void ToDoItem_ConstrtructorTest() {
        	
            ToDoItem item = new ToDoItem();
            Assert.NotNull(item);
            Assert.Null(item.id);
            Assert.Null(item.rev);
            Assert.Null(item.text);
        }
                
        // Verifies IntegrationTests constructor!
        
        [Fact]
        public void IntegrationTests_ConstrtructorTest() {
        	
        	IntegrationTests integrationTest = new IntegrationTests();
            Assert.NotNull(integrationTest.cloudantCreds);
            Assert.NotNull(integrationTest.createItem);
            Assert.NotNull(integrationTest.updateItem);
            Assert.NotNull(integrationTest.deleteItem);
       	}
        
    }
}