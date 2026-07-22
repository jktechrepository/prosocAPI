using Prosoc.Utilities;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;

namespace Prosoc.Tests.Unit.Utilities;

public class WalletVirtuelMouvementHelpersTests
{
    [Theory]
    [InlineData(WalletVirtuelMouvementSources.AjoutSolde, "Recharge manuelle")]
    [InlineData(WalletVirtuelMouvementSources.Creation, "Solde initial")]
    [InlineData(WalletVirtuelMouvementSources.AjustementSolde, "Ajustement administratif")]
    [InlineData(WalletVirtuelMouvementSources.CollecteCompteVirtuel, "Paiement collecte compte virtuel")]
    public void GetLibelle_RetourneLibelleAttendu(string source, string libelleAttendu)
    {
        Assert.Equal(libelleAttendu, WalletVirtuelMouvementSources.GetLibelle(source));
    }

    [Fact]
    public void ToReadDto_AvecOperateurAgent_RemplitIdAgentFromEtNomAgentFrom()
    {
        var agentBeneficiaire = new Agent { IdAgent = 5, NomComplet = "Agent Bénéficiaire", Matricule = "AT-5" };
        var agentFrom = new Agent { IdAgent = 12, NomComplet = "Superviseur Recharge", Matricule = "SP-12" };
        var operateur = new Utilisateur
        {
            IdUtilisateur = 7,
            NomUtilisateur = "superviseur.sp",
            AgentId = 12,
            Agent = agentFrom
        };
        var wallet = new WalletVirtuelAgent
        {
            IdWalletVirtuelAgent = 3,
            AgentId = 5,
            Agent = agentBeneficiaire
        };
        var mouvement = new WalletVirtuelMouvement
        {
            IdWalletVirtuelMouvement = 1,
            WalletVirtuelId = 3,
            WalletVirtuel = wallet,
            Montant = 100m,
            TypeOperation = "CREDIT",
            Source = WalletVirtuelMouvementSources.AjoutSolde,
            OperateurUtilisateurId = 7,
            OperateurUtilisateur = operateur,
            DateOperation = DateTime.UtcNow
        };

        var dto = WalletVirtuelMouvementHelpers.ToReadDto(mouvement);

        Assert.Equal(5, dto.AgentId);
        Assert.Equal("Agent Bénéficiaire", dto.AgentNom);
        Assert.Equal(12, dto.IdAgentFrom);
        Assert.Equal("Superviseur Recharge", dto.NomAgentFrom);
        Assert.Equal(7, dto.OperateurUtilisateurId);
        Assert.Equal("superviseur.sp", dto.OperateurNom);
    }

    [Fact]
    public void ToReadDto_OperateurSansAgent_IdAgentFromNull()
    {
        var operateur = new Utilisateur
        {
            IdUtilisateur = 9,
            NomUtilisateur = "admin.sys",
            AgentId = null,
            Agent = null
        };
        var mouvement = new WalletVirtuelMouvement
        {
            IdWalletVirtuelMouvement = 2,
            WalletVirtuelId = 3,
            Montant = 50m,
            TypeOperation = "CREDIT",
            Source = WalletVirtuelMouvementSources.AjoutSolde,
            OperateurUtilisateurId = 9,
            OperateurUtilisateur = operateur,
            DateOperation = DateTime.UtcNow
        };

        var dto = WalletVirtuelMouvementHelpers.ToReadDto(mouvement);

        Assert.Null(dto.IdAgentFrom);
        Assert.Null(dto.NomAgentFrom);
        Assert.Equal("admin.sys", dto.OperateurNom);
    }

    [Fact]
    public void ToReadDto_SansOperateur_IdAgentFromNull()
    {
        var mouvement = new WalletVirtuelMouvement
        {
            IdWalletVirtuelMouvement = 3,
            WalletVirtuelId = 3,
            Montant = 20m,
            TypeOperation = "DEBIT",
            Source = WalletVirtuelMouvementSources.CollecteCompteVirtuel,
            DateOperation = DateTime.UtcNow
        };

        var dto = WalletVirtuelMouvementHelpers.ToReadDto(mouvement);

        Assert.Null(dto.IdAgentFrom);
        Assert.Null(dto.NomAgentFrom);
        Assert.Null(dto.OperateurUtilisateurId);
    }
}
