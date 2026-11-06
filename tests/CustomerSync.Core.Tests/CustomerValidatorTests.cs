using CustomerSync.Core.Models;
using CustomerSync.Core.Sync;
using Xunit;

namespace CustomerSync.Core.Tests;

public class CustomerValidatorTests
{
    private static SourceCustomer Valid() =>
        new(1, "Customer 1", "c1@example.test", "DEMO0001A01H501Z", "Roma");

    [Fact]
    public void ValidPerson_HasNoErrors() =>
        Assert.Empty(CustomerValidator.Validate(Valid()));

    [Fact]
    public void ValidCompanyVat_HasNoErrors() =>
        Assert.Empty(CustomerValidator.Validate(Valid() with { TaxCode = "10000000005" }));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingName_IsInvalid(string? name) =>
        Assert.Single(CustomerValidator.Validate(Valid() with { Name = name }));

    [Theory]
    [InlineData("customer7@@example")]
    [InlineData("no-at-sign.test")]
    [InlineData("a@b")]
    [InlineData(null)]
    public void BadEmail_IsInvalid(string? email) =>
        Assert.Single(CustomerValidator.Validate(Valid() with { Email = email }));

    [Theory]
    [InlineData(null)]
    [InlineData("SHORT")]
    [InlineData("123")]
    public void BadTaxCode_IsInvalid(string? taxCode) =>
        Assert.Single(CustomerValidator.Validate(Valid() with { TaxCode = taxCode }));
}