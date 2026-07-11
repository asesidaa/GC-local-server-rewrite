using Application.Interfaces;
using Mediator;
using Shared.Models;

namespace XmlSerializationTests;

public class MediatorMigrationTests
{
    [Fact]
    public void QueryWrapper_UsesNativeQueryContract()
    {
        Assert.Contains(
            typeof(IQuery<ServiceResult<int>>),
            typeof(IQueryWrapper<int>).GetInterfaces());
    }

    [Fact]
    public void CommandWrapper_UsesNativeCommandContract()
    {
        Assert.Contains(
            typeof(ICommand<ServiceResult<int>>),
            typeof(ICommandWrapper<int>).GetInterfaces());
    }
}
