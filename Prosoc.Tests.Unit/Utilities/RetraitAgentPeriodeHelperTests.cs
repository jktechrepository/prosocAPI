using ProsocAPI.Models.Configuration;
using ProsocAPI.Utilities;
using Xunit;

namespace Prosoc.Tests.Unit.Utilities;

public class RetraitAgentPeriodeHelperTests
{
    private static readonly RetraitAgentOptions DefaultOptions = new();

    [Theory]
    [InlineData(2026, 3, 30, 30)]
    [InlineData(2026, 3, 31, 30)]
    [InlineData(2026, 2, 27, 27)]
    [InlineData(2026, 2, 28, 27)]
    [InlineData(2026, 4, 29, 29)]
    [InlineData(2026, 4, 30, 29)]
    public void EstJourAutorise_Fenetre2DerniersJours_AutoriseDerniersJours(
        int year, int month, int day, int expectedFenetre2Debut)
    {
        var date = new DateTime(year, month, day);
        Assert.Equal(expectedFenetre2Debut, RetraitAgentPeriodeHelper.GetFenetre2Debut(year, month, DefaultOptions));
        Assert.True(RetraitAgentPeriodeHelper.EstJourAutorise(date, DefaultOptions));
        Assert.Equal($"{expectedFenetre2Debut}-{DateTime.DaysInMonth(year, month)}",
            RetraitAgentPeriodeHelper.GetPeriodeInfo(date, DefaultOptions));
    }

    [Theory]
    [InlineData(2026, 2, 26)]
    [InlineData(2026, 3, 25)]
    [InlineData(2026, 4, 28)]
    public void EstJourAutorise_HorsFenetres_RetourneFaux(int year, int month, int day)
    {
        var date = new DateTime(year, month, day);
        Assert.False(RetraitAgentPeriodeHelper.EstJourAutorise(date, DefaultOptions));
        Assert.Equal("Hors période", RetraitAgentPeriodeHelper.GetPeriodeInfo(date, DefaultOptions));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(18)]
    [InlineData(20)]
    public void EstJourAutorise_Fenetre1_RetourneVrai(int day)
    {
        var date = new DateTime(2026, 6, day);
        Assert.True(RetraitAgentPeriodeHelper.EstJourAutorise(date, DefaultOptions));
        Assert.Equal("15-20", RetraitAgentPeriodeHelper.GetPeriodeInfo(date, DefaultOptions));
    }

    [Fact]
    public void GetFenetre2Debut_RespecteMinimumUnJour()
    {
        var options = new RetraitAgentOptions { Fenetre2DerniersJours = 0 };
        Assert.Equal(28, RetraitAgentPeriodeHelper.GetFenetre2Debut(2026, 2, options));
    }

    [Fact]
    public void BuildMessage_HorsPeriode_ContientBornesDuMois()
    {
        var date = new DateTime(2026, 3, 10);
        var message = RetraitAgentPeriodeHelper.BuildMessage(date, estAutorise: false, DefaultOptions);
        Assert.Contains("15", message);
        Assert.Contains("20", message);
        Assert.Contains("30", message);
        Assert.Contains("31", message);
        Assert.Contains("10", message);
    }

    [Theory]
    [InlineData(2026, 3, 16, RetraitAgentPeriodeHelper.Fenetre1, RetraitAgentPeriodeHelper.TypePartiel)]
    [InlineData(2026, 3, 30, RetraitAgentPeriodeHelper.Fenetre2, RetraitAgentPeriodeHelper.TypeTotal)]
    [InlineData(2026, 3, 10, null, null)]
    public void GetFenetreActive_Et_GetTypeRetraitAutorise(
        int year, int month, int day, string? expectedFenetre, string? expectedType)
    {
        var date = new DateTime(year, month, day);
        Assert.Equal(expectedFenetre, RetraitAgentPeriodeHelper.GetFenetreActive(date, DefaultOptions));
        Assert.Equal(expectedType, RetraitAgentPeriodeHelper.GetTypeRetraitAutorise(date, DefaultOptions));
    }
}
