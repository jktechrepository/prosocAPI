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
        Assert.DoesNotContain("UPDATE_AFFILIE", permissions);
        Assert.Contains("READ_AFFILIE", permissions);
        Assert.Contains("READ_TYPE_ADHESION", permissions);
        Assert.Contains("READ_STATISTIQUES", permissions);
    }

    [Fact]
    public void FinancierRole_DoesNotIncludeUpdateAffilie()
    {
        var permissions = InvokePermissionWhitelist("GetFinancierRolePermissionNames");
        Assert.DoesNotContain("UPDATE_AFFILIE", permissions);
        Assert.Contains("READ_AFFILIE", permissions);
        Assert.Contains("UPDATE_ADHESION", permissions);
    }

    [Fact]
    public void FinancierRole_DoesNotIncludeUpdateWalletVirtuel()
    {
        var permissions = InvokePermissionWhitelist("GetFinancierRolePermissionNames");
        Assert.DoesNotContain("UPDATE_WALLET_VIRTUEL", permissions);
        Assert.Contains("READ_WALLET_VIRTUEL", permissions);
    }

    [Fact]
    public void FinancierRole_IncludesCreateDeviseAndTauxChange()
    {
        var permissions = InvokePermissionWhitelist("GetFinancierRolePermissionNames");
        Assert.Contains("CREATE_DEVISE", permissions);
        Assert.Contains("CREATE_TAUX_CHANGE", permissions);
        Assert.Contains("READ_DEVISE", permissions);
    }

    [Fact]
    public void FinancierRole_IncludesCreateFrais()
    {
        var permissions = InvokePermissionWhitelist("GetFinancierRolePermissionNames");
        Assert.Contains("CREATE_FRAIS", permissions);
        Assert.Contains("READ_FRAIS", permissions);
        Assert.Contains("UPDATE_FRAIS", permissions);
    }

    [Fact]
    public void FinancierRole_IncludesCreateUpdateProduits()
    {
        var permissions = InvokePermissionWhitelist("GetFinancierRolePermissionNames");
        Assert.Contains("CREATE_PRODUIT_MUTUEL", permissions);
        Assert.Contains("UPDATE_PRODUIT_MUTUEL", permissions);
        Assert.Contains("CREATE_PRODUIT_ASSUREUR", permissions);
        Assert.Contains("UPDATE_PRODUIT_ASSUREUR", permissions);
        Assert.Contains("READ_PRODUIT_MUTUEL", permissions);
        Assert.Contains("READ_PRODUIT_ASSUREUR", permissions);
    }

    [Fact]
    public void FinancierRole_IncludesUpdateDeleteSouscriptionPrestation()
    {
        var permissions = InvokePermissionWhitelist("GetFinancierRolePermissionNames");
        Assert.Contains("READ_SOUSCRIPTION_PRESTATION", permissions);
        Assert.Contains("UPDATE_SOUSCRIPTION_PRESTATION", permissions);
        Assert.Contains("DELETE_SOUSCRIPTION_PRESTATION", permissions);
    }

    [Fact]
    public void ItRole_IncludesCreateTauxChange()
    {
        var permissions = InvokePermissionWhitelist("GetItRolePermissionNames");
        Assert.Contains("CREATE_DEVISE", permissions);
        Assert.Contains("CREATE_TAUX_CHANGE", permissions);
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

    [Fact]
    public void CaissierRole_DoesNotIncludeUpdateDeleteSouscriptionPrestation()
    {
        var permissions = InvokePermissionWhitelist("GetCaissierRolePermissionNames");
        Assert.DoesNotContain("UPDATE_SOUSCRIPTION_PRESTATION", permissions);
        Assert.DoesNotContain("DELETE_SOUSCRIPTION_PRESTATION", permissions);
        Assert.Contains("READ_SOUSCRIPTION_PRESTATION", permissions);
    }

    [Fact]
    public void CaissierRole_IncludesUpdateAdhesionEtAffilie()
    {
        var permissions = InvokePermissionWhitelist("GetCaissierRolePermissionNames");
        Assert.Contains("UPDATE_ADHESION", permissions);
        Assert.Contains("UPDATE_AFFILIE", permissions);
        Assert.Contains("READ_ADHESION", permissions);
        Assert.Contains("READ_AFFILIE", permissions);
    }

    [Fact]
    public void CaissierRole_IncludesDemandeRetraitAgentCreateReadValidate()
    {
        var permissions = InvokePermissionWhitelist("GetCaissierRolePermissionNames");
        Assert.Contains("CREATE_DEMANDE_RETRAIT_AGENT", permissions);
        Assert.Contains("READ_DEMANDE_RETRAIT_AGENT", permissions);
        Assert.Contains("VALIDATE_DEMANDE_RETRAIT_AGENT", permissions);
        Assert.Contains("CONFIRM_RETRAIT_AGENT", permissions);
    }

    [Fact]
    public void AgentAtRole_IncludesDemandeRetraitAgentCreateRead()
    {
        var permissions = InvokePermissionWhitelist("GetAgentAtRolePermissionNames");
        Assert.Contains("CREATE_DEMANDE_RETRAIT_AGENT", permissions);
        Assert.Contains("READ_DEMANDE_RETRAIT_AGENT", permissions);
        Assert.DoesNotContain("VALIDATE_DEMANDE_RETRAIT_AGENT", permissions);
    }

    [Fact]
    public void SuperviseurRole_IncludesDemandeRetraitAgentValidate()
    {
        var permissions = InvokePermissionWhitelist("GetSuperviseurRolePermissionNames");
        Assert.Contains("CREATE_DEMANDE_RETRAIT_AGENT", permissions);
        Assert.Contains("READ_DEMANDE_RETRAIT_AGENT", permissions);
        Assert.Contains("VALIDATE_DEMANDE_RETRAIT_AGENT", permissions);
    }
}
