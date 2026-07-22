using System.Reflection;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;

namespace Prosoc.Tests.Unit.Data;

public class OperationalRoleCollectePermissionsTests
{
    private static IReadOnlyList<string> InvokePermissionWhitelist(string methodName)
    {
        var method = typeof(SeedData).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (IReadOnlyList<string>)method!.Invoke(null, null)!;
    }

    private static IEnumerable<string> InvokePermissionFilter(string methodName)
    {
        var method = typeof(SeedData).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var all = new[]
        {
            new Permission { Nom = "CREATE_COLLECTE", Statut = true },
            new Permission { Nom = "READ_COLLECTE", Statut = true },
            new Permission { Nom = "UPDATE_COLLECTE", Statut = true },
            new Permission { Nom = "DELETE_COLLECTE", Statut = true },
            new Permission { Nom = "MANAGE_SYSTEM", Statut = true },
        };

        return ((IEnumerable<Permission>)method!.Invoke(null, new object[] { all })!)
            .Select(p => p.Nom);
    }

    [Theory]
    [InlineData("GetAgentAtRolePermissionNames")]
    [InlineData("GetAgentAaRolePermissionNames")]
    [InlineData("GetSuperviseurRolePermissionNames")]
    [InlineData("GetFinancierRolePermissionNames")]
    [InlineData("GetPercepteurRolePermissionNames")]
    [InlineData("GetCaissierRolePermissionNames")]
    [InlineData("GetChefEquipeRolePermissionNames")]
    public void OperationalRoles_DoNotIncludeUpdateCollecte(string methodName)
    {
        var permissions = InvokePermissionWhitelist(methodName);
        Assert.DoesNotContain("UPDATE_COLLECTE", permissions);
    }

    [Fact]
    public void AdminRoleFilter_DoesNotIncludeUpdateCollecte()
    {
        var permissions = InvokePermissionFilter("FilterPermissionsForAdminRole");
        Assert.DoesNotContain("UPDATE_COLLECTE", permissions);
        Assert.Contains("CREATE_COLLECTE", permissions);
    }

    [Fact]
    public void SuperAdminRoleFilter_DoesNotIncludeUpdateCollecte()
    {
        var permissions = InvokePermissionFilter("FilterPermissionsForSuperAdminRole");
        Assert.DoesNotContain("UPDATE_COLLECTE", permissions);
        Assert.Contains("DELETE_COLLECTE", permissions);
    }

    [Fact]
    public void AgentAtRole_StillIncludesCreateAndReadCollecte()
    {
        var permissions = InvokePermissionWhitelist("GetAgentAtRolePermissionNames");
        Assert.Contains("CREATE_COLLECTE", permissions);
        Assert.Contains("READ_COLLECTE", permissions);
    }

    [Fact]
    public void FinancierRole_IncludesAdhesionFlowPermissions()
    {
        var permissions = InvokePermissionWhitelist("GetFinancierRolePermissionNames");
        Assert.Contains("CREATE_ADHESION", permissions);
        Assert.Contains("UPDATE_ADHESION", permissions);
        Assert.Contains("UPDATE_AFFILIE", permissions);
        Assert.Contains("READ_TYPE_ADHESION", permissions);
        Assert.Contains("READ_STATISTIQUES", permissions);
    }

    [Fact]
    public void CaissierRole_IncludesReadStatistiques()
    {
        var permissions = InvokePermissionWhitelist("GetCaissierRolePermissionNames");
        Assert.Contains("READ_STATISTIQUES", permissions);
    }

    [Fact]
    public void AgentAtRole_DoesNotIncludeCreateDependant()
    {
        var permissions = InvokePermissionWhitelist("GetAgentAtRolePermissionNames");
        Assert.DoesNotContain("CREATE_DEPENDANT", permissions);
        Assert.DoesNotContain("UPDATE_DEPENDANT", permissions);
        Assert.DoesNotContain("DELETE_DEPENDANT", permissions);
        Assert.Contains("READ_DEPENDANT", permissions);
    }

    [Fact]
    public void ChefEquipeRole_DoesNotIncludeCreateDependant()
    {
        var permissions = InvokePermissionWhitelist("GetChefEquipeRolePermissionNames");
        Assert.DoesNotContain("CREATE_DEPENDANT", permissions);
        Assert.DoesNotContain("UPDATE_DEPENDANT", permissions);
        Assert.DoesNotContain("DELETE_DEPENDANT", permissions);
    }

    [Fact]
    public void SuperviseurRole_DoesNotIncludeCreateDependant()
    {
        var permissions = InvokePermissionWhitelist("GetSuperviseurRolePermissionNames");
        Assert.DoesNotContain("CREATE_DEPENDANT", permissions);
        Assert.DoesNotContain("UPDATE_DEPENDANT", permissions);
        Assert.DoesNotContain("DELETE_DEPENDANT", permissions);
        Assert.Contains("READ_DEPENDANT", permissions);
    }

    [Fact]
    public void SuperviseurRole_DoesNotIncludeUpdateAdhesionNiAffilie()
    {
        var permissions = InvokePermissionWhitelist("GetSuperviseurRolePermissionNames");
        Assert.DoesNotContain("UPDATE_ADHESION", permissions);
        Assert.DoesNotContain("UPDATE_AFFILIE", permissions);
        Assert.Contains("CREATE_ADHESION", permissions);
        Assert.Contains("READ_ADHESION", permissions);
        Assert.Contains("READ_AFFILIE", permissions);
    }

    [Fact]
    public void SuperviseurRole_DoesNotIncludeAssureurCrud()
    {
        var permissions = InvokePermissionWhitelist("GetSuperviseurRolePermissionNames");
        Assert.DoesNotContain("CREATE_ASSUREUR", permissions);
        Assert.DoesNotContain("READ_ASSUREUR", permissions);
        Assert.DoesNotContain("UPDATE_ASSUREUR", permissions);
        Assert.DoesNotContain("DELETE_ASSUREUR", permissions);
        Assert.DoesNotContain("CREATE_PRODUIT_ASSUREUR", permissions);
        Assert.Contains("READ_PRODUIT_ASSUREUR", permissions);
    }

    [Fact]
    public void AgentAtRole_StillIncludesUpdateAdhesionEtAffilie()
    {
        var permissions = InvokePermissionWhitelist("GetAgentAtRolePermissionNames");
        Assert.Contains("UPDATE_ADHESION", permissions);
        Assert.Contains("UPDATE_AFFILIE", permissions);
    }
}
