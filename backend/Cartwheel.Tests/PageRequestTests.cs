using Cartwheel.Domain.Repositories;

namespace Cartwheel.Tests;

public class PageRequestTests
{
    [Fact]
    public void Constructor_ValidValues_KeepsThem()
    {
        var request = new PageRequest(3, 20);

        Assert.Equal(3, request.Page);
        Assert.Equal(20, request.PageSize);
    }

    [Fact]
    public void Constructor_NoArguments_UsesFirstPageAndDefaultSize()
    {
        var request = new PageRequest();

        Assert.Equal(1, request.Page);
        Assert.Equal(PageRequest.DefaultPageSize, request.PageSize);
    }

    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(2, 20, 20)]
    [InlineData(3, 20, 40)]
    public void Skip_ValidValues_IsItemsBeforeThePage(int page, int pageSize, int expected)
    {
        Assert.Equal(expected, new PageRequest(page, pageSize).Skip);
    }

    [Fact]
    public void Skip_LargestAllowedValues_DoesNotOverflow()
    {
        var request = new PageRequest(PageRequest.MaxPage, PageRequest.MaxPageSize);

        Assert.Equal((PageRequest.MaxPage - 1) * PageRequest.MaxPageSize, request.Skip);
        Assert.True(request.Skip > 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(PageRequest.MaxPage + 1)]
    [InlineData(int.MaxValue)]
    public void Constructor_PageOutOfRange_Throws(int page)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PageRequest(page, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(PageRequest.MaxPageSize + 1)]
    public void Constructor_PageSizeOutOfRange_Throws(int pageSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PageRequest(1, pageSize));
    }

    [Fact]
    public void Equals_SameValues_AreEqual()
    {
        Assert.Equal(new PageRequest(2, 5), new PageRequest(2, 5));
    }
}
