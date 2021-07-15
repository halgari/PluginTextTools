using Xunit;

namespace PluginTextTools.Differ.Test
{
    
    public class DifferTests
    {
        [Fact]
        public void DiffLists()
        {
            dynamic differ = new Differ();
            var (result, value) = ((Differ.Result, object?))differ.Diff(new[] {"1", "2", "3", "4"}, new[] {"1", "42", "3"});
            Assert.Equal(Differ.Result.Modified, result);
            Assert.Equal(new object[] {"@@inherit@@", "42", "@@inherit@@", "@@deleted@@"}, value);
        }
        
    }
}